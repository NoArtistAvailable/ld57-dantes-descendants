using System.Linq;
using elZach.Common;
using UnityEngine;

namespace LD57
{
	// LUST
	public class Mod_Healthy : Card, IManipulateHealth
	{
		public override int circleOfHell => 1;
		public override string Name => "Healthy";
		public override string Description => "Increases Health the deeper you go into hell (30% per circle)";
		public float healthMult = 1.3f;
		public float ManipulateHealth(float value)
		{
			return value * Mathf.Pow(healthMult, PlayerManager.instance.circleOfHell + 1);
		}
	}
	public class Mod_Strong : Card, IManipulatePower
	{
		public override int circleOfHell => 1;
		public override string Name => "Strong";
		public override string Description => "Increases Power the deeper you go into hell (20% per circle)";
		public float powerMult = 1.2f;
		public float ManipulatePower(float value)
		{
			return value * Mathf.Pow(powerMult, PlayerManager.instance.circleOfHell + 1);
		}
	}
	public class Mod_Smart : Card, IManipulateSpeed
	{
		public override int circleOfHell => 1;
		public override string Name => "Smart";
		public override string Description => "Increases Speed by 30%";
		public float speedMult = 1.3f;
		public float ManipulateSpeed(float value)
		{
			return value * speedMult;
		}
	}

	public class Dance : ActiveCard
	{
		public override int circleOfHell => 1;
		public override string Name => "Dance";
		public override string Description => $"Dances to restore sanity to your party | ({value}) every {cooldown}s";
		public override float cooldown => 3;
		public float value = 2f;
		public override void Activate(UnitCombatBehaviour unitCombatBehaviour)
		{
			var friends = CombatManager.GetFriends(unitCombatBehaviour);
			foreach (var friend in friends) friend.Heal(unitCombatBehaviour.PowerCalc * value);
		}
	}
	
	public class Kiss : ActiveCard
	{
		public override int circleOfHell => 1;
		public override string Name => "Kiss";
		public override string Description => $"Throws a kiss to a friend | increases strength by ({value*100}%) every {cooldown}s";
		public override float cooldown => 2.22f;
		public float value = 0.3f;
		public override void Activate(UnitCombatBehaviour unitCombatBehaviour)
		{
			var friends = CombatManager.GetFriends(unitCombatBehaviour).Where(x => x != unitCombatBehaviour).ToList();
			if (friends.Count == 0) return;
			var target = friends.GetRandom();
			target.powerChanges.Add((original) => original + value);
		}
	}
}