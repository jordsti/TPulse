﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMapper.Structures
{
	public enum MarkerType
	{
		Unknown = -1, 
		Altar = 0,
		Heart,
		Hellforge,
		ShadowOrb,
		Torch,
		Amethyst = 5,
		Diamond,
		Emerald,
		Ruby,
		Sapphire,
		Topaz = 10,
		Copper,
		Iron,
		Silver,
		Gold,
		Demonite = 15,
		Obsidian, 
		Meteorite,
		Hellstone,
		Cobalt,
		Mythril = 20,
		Adamantite,
		Statue = 22,

		// Extra space here for improvement room.
		ArmsDealer = 30,
		Clothier,
		Demolitionist,
		Dryad,
		GoblinTinkerer,
		Guide = 35,
		Mechanic,
		Merchant,
		Nurse,
		OldMan,
		SantaClaus = 40,
		Wizard,
		Spawn = 42,

		// Extra space again.
		Chest = 50,
		GoldChest,
		LockedGoldChest,
		ShadowChest,
		LockedShadowChest,
		Barrel = 55,
		TrashCan = 56,

		// Extra space again.

		Sign = 70,
		Tombstone,
	}
}
