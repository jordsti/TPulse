﻿/*
TPulse, a server mod for Terraria, forked from TShock
Copyright (C) 2013 StiCode

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System.IO;
using System.IO.Streams;

namespace TPulseAPI.Net
{
	public class SpawnMsg : BaseMsg
	{
		public override PacketTypes ID
		{
			get { return PacketTypes.PlayerSpawn; }
		}

		public int TileX { get; set; }
		public int TileY { get; set; }
		public byte PlayerIndex { get; set; }

		public override void Pack(Stream stream)
		{
			stream.WriteInt8(PlayerIndex);
			stream.WriteInt32(TileX);
			stream.WriteInt32(TileY);
		}
	}
}