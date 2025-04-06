﻿using System.Collections.Generic;
using elZach.Common;
using UnityEngine;

namespace LD57
{
	public class PlayerManager
	{
		public static PlayerManager instance => _instance ??= new PlayerManager();
		private static PlayerManager _instance = null;

		public static List<Unit> GetOrInitSquad(int circleLevel = -1)
		{
			Debug.Log($"{instance.squad.Count}");
			if (instance.squad.Count > 0) return instance.squad;
			instance.squad = CharacterCreator.GetSquadAtCircle(circleLevel);
			if (instance.playerUnit != null) instance.squad[0] = instance.playerUnit;
			else instance.playerUnit = instance.squad[0];
			return instance.squad;
		}
		
		public Unit playerUnit;
		
		public int circleOfHell = 0;
		public List<Unit> squad = new List<Unit>();
		
	}
}