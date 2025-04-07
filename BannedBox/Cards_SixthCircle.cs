using System;
using System.Collections.Generic;
using System.Linq;
using elZach.Common;
using UnityEngine;

namespace LD57
{
	// HERESY
	public class Mod_Iconoclast : Card, IInitializeOnCombat
	{
		public override int circleOfHell => 5;
		public override string Name => "Iconoclast";
		public override string Description => $"Deals {maxPowerBoost*100}% more damage if enemies are healthier";
		private float maxPowerBoost = 0.6f;

		public void OnCombatStart(UnitCombatBehaviour behaviour)
		{
			if (behaviour == null) return;
			float CalculatePowerBoost(float original)
			{
				var friends = CombatManager.GetFriends(behaviour);
				var enemies = CombatManager.GetEnemies(behaviour);
				var friendAverage = friends.Sum(x => x.currentHealth / x.cardHealth) / 4f;
				var enemyAverage = enemies.Sum(x => x.currentHealth / x.cardHealth) / 4f;
				return friendAverage < enemyAverage ? original + (1 * maxPowerBoost) : original;
			}
			behaviour.powerChanges.Add(CalculatePowerBoost);
		}
	}

	public class Mod_Faithless : Card, IManipulateCrit
	{
		public override int circleOfHell => 5;
		public override string Name => "Faithless";
		public override string Description => $"Rejects dogma, gaining {critBoost*100}% critical hit chance";
		private float critBoost = 0.35f; // 15% crit chance

		public float ManipulateCrit(float value)
		{
			return value + critBoost;
		}
	}

	public class Mod_Apostate : Card, IInitializeOnCombat
	{
		public override int circleOfHell => 5;
		public override string Name => "Apostate";
		public override string Description => $"Each critical hit increases speed by {speedBoostPerCrit*100}% (stacks up to {maxCritStacks} times)";
		private Dictionary<UnitCombatBehaviour, int> critCounter = new Dictionary<UnitCombatBehaviour, int>();
		private float speedBoostPerCrit = 0.1f;
		private int maxCritStacks = 5;

		public void OnCombatStart(UnitCombatBehaviour behaviour)
		{
			if (behaviour == null) return;
			
			// Initialize if not already tracked
			if (!critCounter.ContainsKey(behaviour))
			{
				critCounter[behaviour] = 0;
				behaviour.OnCrit += () => OnCritical(behaviour);
			}
		}

		private void OnCritical(UnitCombatBehaviour unit)
		{
			if (!critCounter.ContainsKey(unit)) return;
			
			// Only increment if below max stacks
			if (critCounter[unit] < maxCritStacks)
			{
				critCounter[unit]++;
				
				// Add a new speed boost for this stack
				unit.speedChanges.Add((original) => original * (1 + speedBoostPerCrit));
				
				Debug.Log($"{unit.Unit.name} gains Apostate speed boost: {critCounter[unit] * speedBoostPerCrit * 100}%");
			}
		}
	}

	public class Mod_Branded : Card, IManipulatePower, IManipulateHealth
	{
		public override int circleOfHell => 5;
		public override string Name => "Branded";
		public override string Description => $"Marked by heresy - takes {damageTakenIncrease*100}% more damage but deals {damageDealtIncrease*100}% more damage";
		private float damageTakenIncrease = 0.15f;
		private float damageDealtIncrease = 0.4f;

		public float ManipulatePower(float value)
		{
			return value * (1 + damageDealtIncrease);
		}

		public float ManipulateHealth(float value)
		{
			return value * (1 - damageTakenIncrease);
		}
	}

	public class Inquisition : ActiveCard
	{
		public override int circleOfHell => 5;
		public override string Name => "Inquisition";
		public override string Description => $"Purges with righteous fire | Deals ({damage}) damage and reduces target power by {powerReduction*100}% for {debuffDuration}s every {cooldown}s";
		public override float cooldown => 6f;
		public float damage = 10f;
		public float powerReduction = 0.25f; // 25% power reduction
		public float debuffDuration = 4f;

		public override void Activate(UnitCombatBehaviour activator)
		{
			var target = CombatManager.GetHealthiestEnemy(activator);
			if (!target) return;
			
			Debug.Log($"{activator.Unit.name} uses {Name} on {target.Unit.name}");
			
			// Deal damage
			target.Damage(activator.PowerCalc * damage);
			
			// Apply power reduction debuff
			float PowerDebuff(float original) => original * (1 - powerReduction);
			target.powerChanges.Add(PowerDebuff);
			
			// Remove debuff after duration
			async void RemoveDebuff()
			{
				await WebTask.Delay(debuffDuration);
				target.powerChanges.Remove(PowerDebuff);
			}
			RemoveDebuff();
		}
	}
}