﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using System.Timers;
using TMapper.Utilities;

namespace TMapper.Structures
{
	public class World
	{
		// All the parts of a Terraria world.
		private WorldHeader header;
		private Tile[,] tiles;
		private List<Chest> chests;
		private List<Sign> signs;
		private List<NPC> npcs;
		private Footer footer;

		// Various file reading items.
		private BackwardsBinaryReader backReader;
		private BufferedBinaryReader buffReader;
		private BinaryReader reader;
		private FileStream stream;

		// Quick lookup for the world sizes.
		private Int32 MaxX, MaxY;

		// Things to do with showing progress when reading/scanning.
		private Int32 progress;
		private Int64 progressPosition;
		private BackgroundWorker bw;

		// Lookup table for important tiles.
		private Boolean[] tileImportant;

		// Positions of each spot in the file.
		private Int64 posTiles;
		private Int64 posChests;
		private Int64 posSigns;
		private Int64 posNpcs;
		private Int64 posFooter;
		private Int64 posEnd;

		private Single readWorldPerc = 50;

		// List generated from reading in chest tiles.
		private Dictionary<Point, ChestType> chestTypeList;

		#region Structures
		// Helper structure for storing information for tile scanning.
		public class TileImportance
		{
			private Boolean[] tImportant;
			private Boolean[] tKnown;

			public TileImportance()
			{
				Int32 i;

				tImportant = new Boolean[256];
				tKnown = new Boolean[256];

				for (i = 0; i < 256; i++)
				{
					tImportant[i] = false;
					tKnown[i] = false;
				}
			}

			public TileImportance(TileImportance copyFrom)
			{
				Int32 i;

				tImportant = new Boolean[256];
				tKnown = new Boolean[256];

				for (i = 0; i < 256; i++)
				{
					tImportant[i] = copyFrom.tImportant[i];
					tKnown[i] = copyFrom.tKnown[i];
				}
			}

			public Boolean isImportant(Byte i)
			{
				return tImportant[i];
			}

			public void setImportant(Byte i, Boolean v)
			{
				tImportant[i] = v;
			}

			public Boolean isKnown(Byte i)
			{
				return tKnown[i];
			}

			public void setKnown(Byte i, Boolean v)
			{
				tKnown[i] = v;
			}
		}

		// Helper structure for storing all the information
		// for each path when scanning for tiles.
		public class tileReader
		{
			public static Int32 nextId = 0;
			public Int32 id;
			public Int64 filePos;
			public TileImportance tileLookup;
			public Tile tile;
			public List<Byte> tileOrder;
			public Int32 tilesRead;

			public Point startAt;
			public Byte splitAt;

			public tileReader(Int64 fPos)
			{
				id = nextId;
				nextId++;

				filePos = fPos;

				tileLookup = new TileImportance();
				tile = new Tile();
				tileOrder = new List<Byte>();

				tilesRead = 0;
			}

			public tileReader(tileReader copy)
			{
				id = nextId;
				nextId++;

				filePos = copy.filePos;

				tileLookup = new TileImportance(copy.tileLookup);
				tile = new Tile(copy.tile);
				tileOrder = new List<Byte>();

				tilesRead = copy.tilesRead;

				foreach (Byte b in copy.tileOrder)
					tileOrder.Add(b);
			}
		}
		#endregion

		#region Constructors
		public World()
		{
			Clear();

			Initialize();
		}
		#endregion

		private void Initialize()
		{

			tileImportant = new Boolean[256];

			for (Int32 i = 0; i <= 255; i++)
				tileImportant[i] = TileProperties.tileTypeDefs[(Byte) i].IsImportant;
		}

		public void Clear()
		{
			header = new WorldHeader();
			tiles = null;
			chests = null;
			signs = null;
			footer = null;

			posChests = 0;
			posEnd = 0;
			posFooter = 0;
			posNpcs = 0;
			posSigns = 0;
			posTiles = 0;

			MaxX = 0;
			MaxY = 0;
			progress = 0;

			if (reader != null)
			{
				reader.Close();
				reader = null;
			}

			if (stream != null)
				stream = null;

			if (buffReader != null)
				buffReader = null;

			if (bw != null)
				bw = null;
		}

		#region ReadFunctions

		public void ReadWorld(String world, BackgroundWorker worker = null)
		{
			Timer t = null;
#if (DEBUG == false)
			try
			{
#endif
				if (worker != null)
				{
					bw = worker;
					progressPosition = 0;
					t = new Timer(333);
					t.Elapsed += new ElapsedEventHandler(timer_ReadWorld);
					t.Start();
				}

				readWorldPerc = 50;

				stream = new FileStream(world, FileMode.Open, FileAccess.Read);
				reader = new BinaryReader(stream);

				ReadHeader();
				ReadWorldTiles();
				ReadChests();
				ReadSigns();
				ReadNPCs();
				ReadFooter();
#if (DEBUG == false)
			}
			catch (Exception e)
			{
				if (bw != null)
				{
					t.Stop();
					bw = null;
				}

				reader.Close();

				throw e;
			}
#endif

				if (bw != null)
			{
				t.Stop();
				bw = null;
			}

			reader.Close();
		}

		private void timer_ReadWorld(object sender, ElapsedEventArgs e)
		{
			progress = (Int32)(((Single)progressPosition / stream.Length) * readWorldPerc);
			bw.ReportProgress(progress);
		}

		private void ReadHeader()
		{
			Int32 version = reader.ReadInt32();
			Int32 x, y, w, h;

			if (bw != null)
				bw.ReportProgress((Int32)(((Single)progressPosition / stream.Length) * readWorldPerc)
					, "Reading Header");

			header = new WorldHeader();

			header.ReleaseNumber = version;
			header.Name = reader.ReadString();
			header.Id = reader.ReadInt32();
			x = reader.ReadInt32();
			w = reader.ReadInt32();
			y = reader.ReadInt32();
			h = reader.ReadInt32();

			header.WorldCoords = new Rect(x, w, y, h);

			y = reader.ReadInt32();
			x = reader.ReadInt32();

			MaxX = x;
			MaxY = y;
			header.MaxTiles = new Point(x, y);

			header.SpawnPoint = new Point(reader.ReadInt32(), reader.ReadInt32());
			header.SurfaceLevel = reader.ReadDouble();
			header.RockLayer = reader.ReadDouble();
			header.TemporaryTime = reader.ReadDouble();
			header.IsDayTime = reader.ReadBoolean();
			header.MoonPhase = reader.ReadInt32();
			header.IsBloodMoon = reader.ReadBoolean();
			header.DungeonPoint = new Point(reader.ReadInt32(), reader.ReadInt32());
			header.IsBoss1Dead = reader.ReadBoolean();
			header.IsBoss2Dead = reader.ReadBoolean();
			header.IsBoss3Dead = reader.ReadBoolean();
			if (version >= 0x24)
			{
				header.IsGoblinSaved = reader.ReadBoolean();
				header.IsWizardSaved = reader.ReadBoolean();
				header.IsMechanicSaved = reader.ReadBoolean();
				header.IsGoblinArmyDefeated = reader.ReadBoolean();
				header.IsClownDefeated = reader.ReadBoolean();
			}
			if (version >= 0x25)
			{
				header.IsFrostDefeated = reader.ReadBoolean();
			}
			header.IsShadowOrbSmashed = reader.ReadBoolean();
			header.IsMeteorSpawned = reader.ReadBoolean();
			header.ShadowOrbsSmashed = reader.ReadByte();

			if (version >= 0x24)
			{
				header.AltarsDestroyed = reader.ReadInt32();
				header.HardMode = reader.ReadBoolean();
			}

			header.InvasionDelay = reader.ReadInt32();
			header.InvasionSize = reader.ReadInt32();
			header.InvasionType = reader.ReadInt32();
			header.InvasionPointX = reader.ReadDouble();


			posTiles = stream.Position;
			progressPosition = stream.Position;
		}

		private void ReadWorldTiles()
		{
			Boolean theB;
			Byte theI;
			Tile theTile;
			Int32 i, j;

			if (bw != null)
				bw.ReportProgress((Int32)(((Single)progressPosition / stream.Length) * readWorldPerc)
					, "Reading Tiles");

			tiles = new Tile[MaxX, MaxY];

			for (i = 0; i < MaxX; i++)
			{
				for (j = 0; j < MaxY; j++)
				{
					theTile = new Tile();

					theB = reader.ReadBoolean();

					theTile.Active = theB;
					if (theB == true)
					{
						theI = reader.ReadByte();

						if (theI > 85)
							theTile.TileType = theI;

						theTile.TileType = theI;

						if (tileImportant[theI] == true)
						{
							theTile.Important = true;
							theTile.Frame = new PointInt16(reader.ReadInt16() ,reader.ReadInt16());
						}
						else
							theTile.Important = false;
					}

					theTile.Lighted = reader.ReadBoolean();

					theB = reader.ReadBoolean();

					theTile.Wall = theB;
					if (theB == true)
						theTile.WallType = reader.ReadByte();

					if (theTile.WallType == 0 && theTile.Wall == true)
						theTile.Wall = true;

					theB = reader.ReadBoolean();
					theTile.Liquid = theB;

					if (theB == true)
					{
						theTile.LiquidLevel = reader.ReadByte();
						theTile.Lava = reader.ReadBoolean();
					}

					tiles[i, j] = theTile;
				}
				progressPosition = stream.Position;
			}

			posChests = stream.Position;
		}

		private void ReadChests()
		{
			Boolean isChest;
			Byte itemCount;
			Chest theChest = null;
			Item theItem;
			Int32 i, j;
			chests = new List<Chest>();

			if (bw != null)
				bw.ReportProgress((Int32)(((Single)progressPosition / stream.Length) * readWorldPerc)
					, "Reading Chests");
			
			for (i = 0; i < 1000; i++)
			{
				isChest = reader.ReadBoolean();

				if (isChest == true)
				{
					theChest = new Chest();
					theChest.ChestId = i;
					theChest.Active = isChest;

					theChest.Coordinates = new Point(reader.ReadInt32(), reader.ReadInt32());

					if (chestTypeList != null)
					{
						if (chestTypeList.ContainsKey(theChest.Coordinates))
							theChest.Type = chestTypeList[theChest.Coordinates];
					}
					else
					{
						theChest.Type = ChestType.Chest;
					}

					for (j = 0; j < 20; j++)
					{
						itemCount = reader.ReadByte();

						if (itemCount > 0)
						{
							theItem = new Item();
							theItem.Id = j;
							theItem.Count = itemCount;

							if (header.ReleaseNumber >= 0x26)
							{
								theItem.Name = Global.Instance.Info.GetItemName(reader.ReadInt32());
							}
							else
							{
								theItem.Name = reader.ReadString();
							}

							if (header.ReleaseNumber >= 0x24)
								theItem.Prefix = reader.ReadByte();

							theChest.Items.Add(theItem);
						}
					}
					chests.Add(theChest);
				}

				progressPosition = stream.Position;
			}

			posSigns = stream.Position;
		}

		private void ReadSigns()
		{
			Boolean isSign;
			Sign theSign;
			signs = new List<Sign>(1000);

			if (bw != null)
				bw.ReportProgress((Int32)(((Single)progressPosition / stream.Length) * readWorldPerc)
					, "Reading Signs");

			for (Int32 i = 0; i < 1000; i++)
			{
				isSign = reader.ReadBoolean();
				if (isSign == true)
				{
					theSign = new Sign();
					theSign.Id = i;
					theSign.Active = isSign;
				
					theSign.Text = reader.ReadString();
					theSign.Position = new Point(reader.ReadInt32(), reader.ReadInt32());

					signs.Add(theSign);
				}

				progressPosition = stream.Position;
			}

			posNpcs = stream.Position;
		}

		private void ReadNPCs()
		{
			Boolean nextNPC;
			NPC theNPC;
			String nameCrunch;
			String[] nameArray;
			NPCType npcType;
			Int32 i;

			npcs = new List<NPC>(20);
			i = 0;

			if (bw != null)
				bw.ReportProgress((Int32)(((Single)progressPosition / stream.Length) * readWorldPerc)
					, "Reading NPCs");

			nextNPC = reader.ReadBoolean();
			while (nextNPC == true)
			{
				theNPC = new NPC();

				theNPC.Id = i;

				theNPC.Active = nextNPC;
				theNPC.Name = reader.ReadString();
				theNPC.Position = new PointSingle(reader.ReadSingle(), reader.ReadSingle());
				theNPC.Homeless = reader.ReadBoolean();
				theNPC.HomeTile = new Point(reader.ReadInt32(), reader.ReadInt32());

				nameArray = theNPC.Name.Split(' ');
				nameCrunch = "";
				for (Int32 j = 0; j < nameArray.Length; j++)
					nameCrunch += nameArray[j];

				if (Enum.TryParse<NPCType>(nameCrunch, out npcType))
					theNPC.Type = npcType;
				else
					theNPC.Type = NPCType.Unknown;
					
				npcs.Add(theNPC);
				i++;

				nextNPC = reader.ReadBoolean();

				progressPosition = stream.Position;
			}

			posFooter = stream.Position;
		}

		private void ReadNPCNames()
		{
			if (header.ReleaseNumber >= 0x24)
			{
				header.MerchantsName = reader.ReadString();
				header.NursesName = reader.ReadString();
				header.ArmsDealersName = reader.ReadString();
				header.DryadsName = reader.ReadString();
				header.GuidesName = reader.ReadString();
				header.ClothiersName = reader.ReadString();
				header.DemolitionistsName = reader.ReadString();
				header.TinkerersName = reader.ReadString();
				header.WizardsName = reader.ReadString();
				header.MechanicsName = reader.ReadString();
			}
			else
			{
				header.MerchantsName = "Not set";
				header.NursesName = "Not Set";
				header.ArmsDealersName = "Not Set";
				header.DryadsName = "Not Set";
				header.GuidesName = "Not Set";
				header.ClothiersName = "Not Set";
				header.DemolitionistsName = "Not Set";
				header.TinkerersName = "Not Set";
				header.WizardsName = "Not Set";
				header.MechanicsName = "Not Set";
			}
		}

		private void ReadFooter()
		{
			footer = new Footer();

			if (bw != null)
				bw.ReportProgress((Int32)(((Single)progressPosition / stream.Length) * readWorldPerc)
					, "Reading Footer");

			footer.Active = reader.ReadBoolean();
			footer.Name = reader.ReadString();
			footer.Id = reader.ReadInt32();

			posEnd = stream.Position;
			progressPosition = stream.Position;
		}

		public Byte[,] ReadAndProcessWorld(String worldPath, BackgroundWorker worker)
		{
			Int32 col, row;
			bool isTileActive;
			Byte tileType = 0;
			Byte[,] retTiles;
			byte wallType, liquidLevel;
			bool isLighted, isLava;
			bool isWall, isLiquid;
			bool hasWire;
			Timer t = null;

#if (DEBUG == false)
			try
			{
#endif
				if (worker != null)
				{
					bw = worker;
					progressPosition = 0;
					t = new Timer(333);
					t.Elapsed += new ElapsedEventHandler(timer_ReadWorld);
					t.Start();
				}

				if (SettingsManager.Instance.ShowChestTypes == true)
					chestTypeList = new Dictionary<Point, ChestType>();
				else
					chestTypeList = null;

				readWorldPerc = 45;

				stream = new FileStream(worldPath, FileMode.Open, FileAccess.Read);
				reader = new BinaryReader(stream);


				ReadHeader();

				MaxX = header.MaxTiles.X;
				MaxY = header.MaxTiles.Y;

				// Reset MapTile List
				retTiles = new Byte[MaxX, MaxY];

				if (bw != null)
					bw.ReportProgress(0, "Reading and Processing Tiles");

				if (header.ReleaseNumber < 0x24)
				{
					//Read all the tile data using the pre-1.1 format.
					for (col = 0; col < MaxX; col++)
					{
						progressPosition = stream.Position;

						for (row = 0; row < MaxY; row++)
						{
							isTileActive = reader.ReadBoolean();

							if (isTileActive)
							{
								tileType = reader.ReadByte();

								if (TileProperties.tileTypeDefs[tileType].IsImportant)
								{
									if ((tileType == TileProperties.Chest) && (chestTypeList != null))
									{
										Int16 typeX = reader.ReadInt16();
										Int16 typeY = reader.ReadInt16();

										// We need to be sure we're only capturing the upper left square.
										if ((typeX % 36 == 0) && (typeY == 0))
										{
											if ((typeX / 36) <= (Int32)ChestType.TrashCan)
												chestTypeList.Add(new Point(col, row), (ChestType)(typeX / 36));
											else
												chestTypeList.Add(new Point(col, row), ChestType.Unknown);
										}
									}
									else
									{
										reader.ReadInt16();
										reader.ReadInt16();
									}
								}
							}
							else
							{
								if (row < header.SurfaceLevel)
									tileType = TileProperties.BackgroundOffset;
								else if (row == header.SurfaceLevel)
									tileType = (Byte)(TileProperties.BackgroundOffset + 1); // Dirt Transition
								else if (row < (header.RockLayer + 38))
									tileType = (Byte)(TileProperties.BackgroundOffset + 2); // Dirt
								else if (row == (header.RockLayer + 38))
									tileType = (Byte)(TileProperties.BackgroundOffset + 4); // Rock Transition
								else if (row < (header.MaxTiles.Y - 202))
									tileType = (Byte)(TileProperties.BackgroundOffset + 3); // Rock 
								else if (row == (header.MaxTiles.Y - 202))
									tileType = (Byte)(TileProperties.BackgroundOffset + 6); // Underworld Transition
								else
									tileType = (Byte)(TileProperties.BackgroundOffset + 5); // Underworld
							}

							isLighted = reader.ReadBoolean();

							isWall = reader.ReadBoolean();
							if (isWall)
							{
								wallType = reader.ReadByte();

								if (tileType >= TileProperties.Unknown)
								{
									if (wallType + TileProperties.WallOffset > 255)
									{
										tileType = TileProperties.Unknown;
									}
									else
									{
										tileType = (Byte)(wallType + TileProperties.WallOffset);
									}
								}
							}

							isLiquid = reader.ReadBoolean();
							if (isLiquid)
							{
								liquidLevel = reader.ReadByte();
								isLava = reader.ReadBoolean();

								if (tileType > TileProperties.Unknown)
								{
									tileType = isLava ? TileProperties.Lava : TileProperties.Water;
								}

							}

							retTiles[col, row] = tileType;
						}
					}
				}
				else
				{
					Int16 RLERemaining = 0;

					//Read all the tile data using the RLE format.
					for (col = 0; col < MaxX; col++)
					{
						progressPosition = stream.Position;

						if (col == 201)
							col = 201;

						for (row = 0; row < MaxY; row++)
						{
							if (RLERemaining == 0)
							{
								isTileActive = reader.ReadBoolean();

								if (isTileActive)
								{
									tileType = reader.ReadByte();

									if (TileProperties.tileTypeDefs[tileType].IsImportant)
									{
										if ((tileType == TileProperties.Chest) && (chestTypeList != null))
										{
											Int16 typeX = reader.ReadInt16();
											Int16 typeY = reader.ReadInt16();

											// We need to be sure we're only capturing the upper left square.
											if ((typeX % 36 == 0) && (typeY == 0))
											{
												if ((typeX / 36) <= (Int32)ChestType.TrashCan)
													chestTypeList.Add(new Point(col, row), (ChestType)(typeX / 36));
												else
													chestTypeList.Add(new Point(col, row), ChestType.Unknown);
											}
										}
										else
										{
											reader.ReadInt16();
											reader.ReadInt16();
										}
									}
								}
								else
								{
									if (row < header.SurfaceLevel)
										tileType = TileProperties.BackgroundOffset;
									else if (row == header.SurfaceLevel)
										tileType = (Byte)(TileProperties.BackgroundOffset + 1); // Dirt Transition
									else if (row < (header.RockLayer + 38))
										tileType = (Byte)(TileProperties.BackgroundOffset + 2); // Dirt
									else if (row == (header.RockLayer + 38))
										tileType = (Byte)(TileProperties.BackgroundOffset + 4); // Rock Transition
									else if (row < (header.MaxTiles.Y - 202))
										tileType = (Byte)(TileProperties.BackgroundOffset + 3); // Rock 
									else if (row == (header.MaxTiles.Y - 202))
										tileType = (Byte)(TileProperties.BackgroundOffset + 6); // Underworld Transition
									else
										tileType = (Byte)(TileProperties.BackgroundOffset + 5); // Underworld

								}

								isWall = reader.ReadBoolean();
								if (isWall)
								{
									wallType = reader.ReadByte();

									if (tileType >= TileProperties.Unknown)
									{
										if (wallType + TileProperties.WallOffset > 255)
										{
											tileType = TileProperties.Unknown;
										}
										else
										{
											tileType = (Byte)(wallType + TileProperties.WallOffset);
										}
									}
								}

								isLiquid = reader.ReadBoolean();
								if (isLiquid)
								{
									liquidLevel = reader.ReadByte();
									isLava = reader.ReadBoolean();

									if (tileType > TileProperties.Unknown)
									{
										tileType = isLava ? TileProperties.Lava : TileProperties.Water;
									}

								}

								hasWire = reader.ReadBoolean();

								if ((hasWire == true) && (SettingsManager.Instance.DrawWires))
									tileType = TileProperties.Wire;

								RLERemaining = reader.ReadInt16();

								retTiles[col, row] = tileType;
							}
							else
							{
								if ((tileType >= TileProperties.BackgroundOffset)
									&& (tileType <= TileProperties.BackgroundOffset + 6))
								{
									if (row < header.SurfaceLevel)
										tileType = TileProperties.BackgroundOffset;
									else if (row == header.SurfaceLevel)
										tileType = (Byte)(TileProperties.BackgroundOffset + 1); // Dirt Transition
									else if (row < (header.RockLayer + 38))
										tileType = (Byte)(TileProperties.BackgroundOffset + 2); // Dirt
									else if (row == (header.RockLayer + 38))
										tileType = (Byte)(TileProperties.BackgroundOffset + 4); // Rock Transition
									else if (row < (header.MaxTiles.Y - 202))
										tileType = (Byte)(TileProperties.BackgroundOffset + 3); // Rock 
									else if (row == (header.MaxTiles.Y - 202))
										tileType = (Byte)(TileProperties.BackgroundOffset + 6); // Underworld Transition
									else
										tileType = (Byte)(TileProperties.BackgroundOffset + 5); // Underworld
								}

								retTiles[col, row] = tileType;
								RLERemaining--;
							}
						}
					}
				}

				ReadChests();
				ReadSigns();
				ReadNPCs();
				ReadNPCNames();
				ReadFooter();
#if (DEBUG == false)
			}
			catch (Exception e)
			{
				if (bw != null)
				{
					t.Stop();
					bw = null;
				}

				reader.Close();
				retTiles = null;
				throw e;
			}
#endif

			if (bw != null)
			{
				t.Stop();
				bw = null;
			}

			reader.Close();

			return retTiles;
		}

		private void ScanPastWorldTiles()
		{
			Boolean theB;
			Byte theI;
			Int32 i, j;
			Int16 RLE = 0;

			if (bw != null)
				bw.ReportProgress((Int32)(((Single)progressPosition / stream.Length) * readWorldPerc)
					, "Skipping Tiles");

			if (header.ReleaseNumber < 0x24)
			{
				for (i = 0; i < MaxX; i++)
				{
					for (j = 0; j < MaxY; j++)
					{
						theB = reader.ReadBoolean();

						if (theB == true)
						{
							theI = reader.ReadByte();

							if (tileImportant[theI] == true)
							{
								reader.ReadInt16();
								reader.ReadInt16();
							}
						}

						reader.ReadBoolean();

						theB = reader.ReadBoolean();

						if (theB == true)
							reader.ReadByte();

						theB = reader.ReadBoolean();

						if (theB == true)
						{
							reader.ReadByte();
							reader.ReadBoolean();
						}
					}
					progressPosition = stream.Position;
				}
			}
			else
			{
				for (i = 0; i < MaxX; i++)
				{
					for (j = 0; j < MaxY; j++)
					{
						if (RLE == 0)
						{
							theB = reader.ReadBoolean();

							if (theB == true)
							{
								theI = reader.ReadByte();

								if (tileImportant[theI] == true)
								{
									reader.ReadInt16();
									reader.ReadInt16();
								}
							}

							theB = reader.ReadBoolean();

							if (theB == true)
								reader.ReadByte();

							theB = reader.ReadBoolean();

							if (theB == true)
							{
								reader.ReadByte();
								reader.ReadBoolean();
							}

							reader.ReadBoolean();

							RLE = reader.ReadInt16();
						}
						else
						{
							RLE--;
						}
					}
					progressPosition = stream.Position;
				}
			}
			posChests = stream.Position;
		}


		// This is used to get only the chests from the file.  For the LoadInformation button.
		public List<Chest> GetChests(String world, BackgroundWorker worker = null)
		{
			Timer t = null;
			if (worker != null)
			{
				bw = worker;
				t = new Timer(333);
				t.Elapsed += new ElapsedEventHandler(timer_ReadWorld);
				t.Start();
				progressPosition = 0;
			}

			readWorldPerc = 100;

			stream = new FileStream(world, FileMode.Open, FileAccess.Read);
			reader = new BinaryReader(stream);

#if (DEBUG == false)
			try
			{
#endif
				ReadHeader();

				posChests = new BackwardsScanner(stream, header).SeekToChestsBackwards();
				
				if (posChests != 0)
				{
					ReadChests();
				}
				else
				{
					stream.Seek(posTiles, SeekOrigin.Begin);
					ScanPastWorldTiles();
					ReadChests();
				}
#if (DEBUG == false)
			}
			catch (Exception e)
			{
				if (bw != null)
				{
					t.Stop();
					bw = null;
				}
				throw e;
			}
#endif

			reader.Close();


			if (bw != null)
			{
				t.Stop();
				bw = null;
			}

			reader.Close();

			return chests;
		}
		#endregion

		#region SaveFunctions
		public void SaveWorld(String world)
		{
			FileStream stream = new FileStream(world, FileMode.Create, FileAccess.Write);
			BinaryWriter writer = new BinaryWriter(stream);

			SaveHeader(writer);
			SaveWorldTiles(writer);
			SaveChests(writer);
			SaveSigns(writer);
			SaveNPCs(writer);
			SaveFooter(writer);

			writer.Close();
		}

		private void SaveHeader(BinaryWriter writer)
		{
			writer.Write(header.ReleaseNumber);
			writer.Write(header.Name);
			writer.Write(header.Id);
			writer.Write(header.WorldCoords.TopLeft.X);
			writer.Write(header.WorldCoords.TopLeft.Y);
			writer.Write(header.WorldCoords.BottomRight.X);
			writer.Write(header.WorldCoords.BottomRight.Y);
			writer.Write(header.MaxTiles.Y);
			writer.Write(header.MaxTiles.X);
			writer.Write(header.SpawnPoint.X);
			writer.Write(header.SpawnPoint.Y);
			writer.Write(header.SurfaceLevel);
			writer.Write(header.RockLayer);
			writer.Write(header.TemporaryTime);
			writer.Write(header.IsDayTime);
			writer.Write(header.MoonPhase);
			writer.Write(header.IsBloodMoon);
			writer.Write(header.DungeonPoint.X);
			writer.Write(header.DungeonPoint.Y);
			writer.Write(header.IsBoss1Dead);
			writer.Write(header.IsBoss2Dead);
			writer.Write(header.IsBoss3Dead);
			writer.Write(header.IsShadowOrbSmashed);
			writer.Write(header.IsMeteorSpawned);
			writer.Write(header.ShadowOrbsSmashed);
			writer.Write(header.InvasionDelay);
			writer.Write(header.InvasionSize);
			writer.Write(header.InvasionType);
			writer.Write(header.InvasionPointX);
		}

		private void SaveWorldTiles(BinaryWriter writer)
		{
			Tile theTile;
			Int32 i, j;

			for (i = 0; i < MaxX; i++)
			{
				for (j = 0; j < MaxY; j++)
				{
					theTile = tiles[i, j];


					writer.Write(theTile.Active);

					if (theTile.Active)
					{
						writer.Write(theTile.TileType);

						if (tileImportant[theTile.TileType] == true)
						{
							writer.Write(theTile.Frame.X);
							writer.Write(theTile.Frame.Y);
						}
					}

					writer.Write(theTile.Lighted);

					writer.Write(theTile.Wall);

					if (theTile.Wall)
						writer.Write(theTile.WallType);

					writer.Write(theTile.Liquid);

					if (theTile.Liquid)
					{
						writer.Write(theTile.LiquidLevel);
						writer.Write(theTile.Lava);
					}
				}
			}
		}

		private void SaveChests(BinaryWriter writer)
		{
			Chest nextChest;
			Item nextItem;
			Int32 i, j;

			List<Item>.Enumerator iEnum;
			List<Chest>.Enumerator cEnum;

			cEnum = chests.GetEnumerator();

			cEnum.MoveNext();
			nextChest = cEnum.Current;

			for (i = 0; i < 1000; i++)
			{
				if (nextChest != null && i == nextChest.ChestId)
				{
					writer.Write(nextChest.Active);

					writer.Write(nextChest.Coordinates.X);
					writer.Write(nextChest.Coordinates.Y);

					iEnum = nextChest.Items.GetEnumerator();

					iEnum.MoveNext();
					nextItem = iEnum.Current;

					for (j = 0; j < 20; j++)
					{
						if (nextItem != null && j == nextItem.Id)
						{
							writer.Write(nextItem.Count);
							writer.Write(nextItem.Name);

							iEnum.MoveNext();
							nextItem = iEnum.Current;
						}
						else
						{
							writer.Write((Byte)0);
						}
					}

					cEnum.MoveNext();
					nextChest = cEnum.Current;
				}
				else
				{
					writer.Write(false);
				}
			}
		}

		private void SaveSigns(BinaryWriter writer)
		{
			Sign theSign;
			Int32 i;

			for (i = 0; i < 1000; i++)
			{
				theSign = signs[i];

				writer.Write(theSign.Active);

				if (theSign.Active == true)
				{
					writer.Write(theSign.Text);
					writer.Write(theSign.Position.X);
					writer.Write(theSign.Position.Y);
				}
			}
		}

		private void SaveNPCs(BinaryWriter writer)
		{
			Int32 i = 0;
			NPC theNPC = npcs[i];

			while (theNPC != null)
			{
				writer.Write(theNPC.Active);
				writer.Write(theNPC.Name);
				writer.Write(theNPC.Position.X);
				writer.Write(theNPC.Position.Y);
				writer.Write(theNPC.Homeless);
				writer.Write(theNPC.HomeTile.X);
				writer.Write(theNPC.HomeTile.Y);

				i++;
				theNPC = npcs[i];
			}
			writer.Write(false);
		}

		private void SaveFooter(BinaryWriter writer)
		{
			writer.Write(footer.Active);
			writer.Write(footer.Name);
			writer.Write(footer.Id);
		}
		#endregion

		#region ScanningFunctions
		// Ok so this is how this works.
		// It starts out by walking the file backwards to find the ending offset.
		// It then sets up the original path and starts scanning along the tiles.
		// When it finds an unknown tile type the program duplicates the current
		// path and makes one follow the path of it being non-important while
		// the other follows the path of being important.
		// When a path turns out not to fit the data it gets removed from the list.
		// All paths get checked in parallel.
		public TileImportance[] ScanWorld(String world, BackgroundWorker worker = null)
		{
			Timer t = null;
			TileImportance[] retList;

			if (worker != null)
			{
				bw = worker;
				t = new Timer(333);
				t.Elapsed += new ElapsedEventHandler(timer_ScanWorld);
				t.Enabled = true;
				t.Start();
			}

			stream = new FileStream(world, FileMode.Open, FileAccess.Read);
			reader = new BinaryReader(stream);
			buffReader = new BufferedBinaryReader(stream, 5000, 1000);

			posChests = new BackwardsScanner(stream, header).SeekToChestsBackwards();

			stream.Seek(0, SeekOrigin.Begin);
			ReadHeader();
			retList = ScanWorldTiles();

			if (bw != null)
			{
				t.Stop();
				bw = null;
			}

			reader.Close();

			return retList;
		}

		private void timer_ScanWorld(object sender, ElapsedEventArgs e)
		{
			bw.ReportProgress(progress);
		}

		private TileImportance[] ScanWorldTiles()
		{
			Byte tryByte;
			Byte tileType;
			Tile curTile;
			Int32 i, j, k;
			tileReader curReader;
			tileReader splitReader;
			TileImportance curList;
			TileImportance[] returnList;

			List<tileReader> readerList = new List<tileReader>();

			buffReader.Seek(stream.Position);

			// We set up the lookup we can change here and we'll simply change it.
			// We might not even need this lookup at all.  Work the logic later.
			curReader = new tileReader(stream.Position);
			curReader.startAt = new Point(0, 0);
			curReader.splitAt = 255;
			curList = curReader.tileLookup;
			curTile = curReader.tile;
			readerList.Add(curReader);

			for (i = 0; i < MaxX; i++)
			{
				progress = (Int32)(((i * MaxY) / (double)(MaxX * MaxY)) * 100);

				for (j = 0; j < MaxY; j++)
				{
					// If somehow we manage to knock out every reader as bad then we need to quit.
					if (readerList.Count == 0)
						return null;

					for (k = 0; k < readerList.Count; k++)
					{
						// No reason to keep resetting these if we only have one path going.
						if (readerList.Count > 1)
						{
							curReader = readerList[k];
							curList = curReader.tileLookup;
							curTile = curReader.tile;

							buffReader.Seek(curReader.filePos);
						}

						tryByte = buffReader.ReadByte();

						if (tryByte > 1)
							goto badPath;

						if (tryByte == 1)
						{
							curTile.Active = true;

							tileType = buffReader.ReadByte();
							curTile.TileType = tileType;

							if (curList.isKnown(tileType) == false)
							{
								// Here we need to split the lists.
								curReader.tileOrder.Add(tileType);
								curList.setKnown(tileType, true);
								splitReader = new tileReader(curReader);
								splitReader.startAt = new Point(i, j);
								splitReader.splitAt = tileType;
								curList.setImportant(tileType, false);
								splitReader.tileLookup.setImportant(tileType, true);
								readerList.Add(splitReader);
								bw.ReportProgress(progress, String.Format("Split #{0} {1}", splitReader.id, readerList.Count));

								curTile.Important = false;
							}
							else
							{
								curTile.Important = curList.isImportant(tileType);
							}
						}
						else
						{
							curTile.Active = false;
							curTile.Important = false;
						}

						if (curTile.Important == true)
						{
							curTile.Frame = new PointInt16(buffReader.ReadInt16(), buffReader.ReadInt16());
						}

						// isLighted
						tryByte = buffReader.ReadByte();

						if (tryByte > 1)
							goto badPath;

						if (tryByte == 1)
							curTile.Lighted = true;
						else
							curTile.Lighted = false;

						// isWall
						tryByte = buffReader.ReadByte();

						if (tryByte > 1)
							goto badPath;

						if (tryByte == 1)
						{
							curTile.Wall = true;
							// wallType
							tryByte = buffReader.ReadByte();

							// It turns out there will never be a wall type 0.
							if (tryByte == 0)
								goto badPath;

							curTile.WallType = tryByte;
						}
						else
						{
							curTile.Wall = false;
						}

						// isWater
						tryByte = buffReader.ReadByte();

						if (tryByte > 1)
							goto badPath;

						if (tryByte == 1)
						{
							curTile.Liquid = true;

							// waterLevel
							tryByte = buffReader.ReadByte();
							curTile.LiquidLevel = tryByte;

							// We can have any water value besides zero, if the isWater bit is set.
							if (tryByte == 0)
								goto badPath;

							// isLava
							tryByte = buffReader.ReadByte();

							if (tryByte > 1)
								goto badPath;

							if (tryByte == 1)
								curTile.Lava = true;
							else
								curTile.Lava = false;
						}
						else
						{
							curTile.Liquid = false;
						}

						curReader.filePos = buffReader.Position;

						// This path passed over the end of the tile range.  Bad path.
						// We might not always have a valid chest position though, if they
						// have changed something in a new version.
						if (posChests != 0 && curReader.filePos > posChests)
							goto badPath;

						curReader.tilesRead++;
						continue;

					badPath:
						bw.ReportProgress(progress, String.Format("Path #{0} Terminated {1}", readerList[k].id, readerList.Count-1));

						// Erase the bad path from the list to process.
						readerList.RemoveAt(k);

						// Now that we removed one we need to move the loop back one to 
						// compensate for how it shifts them over.  Otherwise we could
						// have a 0,1,2 then remove 1 which leaves 0,1 and our next
						// loop is for 2 so the old 2 (new 1) gets skipped.
						k--;
						// If we took it back to one we'll need to set up for the next loop
						// as we put the skip in to speed it up.
						if (readerList.Count == 1)
						{
							curReader = readerList[0];
							curList = curReader.tileLookup;
							curTile = curReader.tile;

							buffReader.Seek(curReader.filePos);
						}
					}
				}
			}

			// Time to prep for the return.
			returnList = new TileImportance[readerList.Count];

			// Find the first one that matched up with the end of the tile position.
			for (k = 0; k < readerList.Count; k++)
			{
				if (readerList[k].filePos == posChests)
					break;
			}

			// Now generate the list but put the one that matched first, if one existed.
			if (readerList.Count == k)
			{
				for (i = 0; i < readerList.Count; i++)
					returnList[i] = readerList[i].tileLookup;
			} else {
				returnList[0] = readerList[k].tileLookup;

				j = 1;
				for (i = 0; i < readerList.Count; i++)
				{
					if (i != k)
					{
						returnList[j] = readerList[k].tileLookup;
						j++;
					}
				}
			}

			//bw.ReportProgress(progress, String.Format("Path #{0} Terminated {1}", readerList[k].id, readerList.Count - 1));

			return returnList;
		}
		#endregion

		#region GetSet Functions
		public WorldHeader Header
		{
			get
			{
				return header;
			}
		}

		public Tile[,] Tiles
		{
			get
			{
				return tiles;
			}
		}

		public List<Chest> Chests
		{
			get
			{
				return chests;
			}
		}

		public List<Sign> Signs
		{
			get
			{
				return signs;
			}
		}

		public List<NPC> Npcs
		{
			get
			{
				return npcs;
			}
		}

		public Footer Footer
		{
			get
			{
				return footer;
			}
		}
		#endregion

		public String GetWorldName(String worldFile)
		{
			String worldName;

			stream = new FileStream(worldFile, FileMode.Open, FileAccess.Read);
			reader = new BinaryReader(stream);
			backReader = new BackwardsBinaryReader(stream);
			
			ReadHeader();

			if (CompareFooter(header.Name, header.Id) == true)
				worldName = header.Name;
			else
				worldName = "Not a valid World file";

			reader.Close();

			return worldName;

		}

		private Boolean CompareFooter(String worldName, Int32 worldId)
		{
			Boolean returnVal = false;

			long position = this.stream.Position;

			this.stream.Seek(0, SeekOrigin.End);

#if (DEBUG == false)
			try
			{
#endif
				footer = new Footer();

				footer.Id = backReader.ReadBackwardsInt32();
				footer.Name = backReader.ReadBackwardsString();
				footer.Active = backReader.ReadBackwardsBoolean();
	
				if (footer.Active == true && footer.Name == worldName && footer.Id == worldId)
					returnVal = true;
#if (DEBUG == false)
			}
			catch (EndOfStreamException e)
			{
				// We don't need to do this but I do just so the compiler will not throw an warning
				// for never using it.
				e.GetType();

				// If we read past the end of the stream we know it did not match.
				returnVal = false;
			}
#endif
			// We set the position back to where it was when we were called.
			this.stream.Seek(position, SeekOrigin.Begin);
			return returnVal;
		}
	
	}
}
