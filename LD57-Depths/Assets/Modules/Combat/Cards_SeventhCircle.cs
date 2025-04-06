using System;
using System.Collections.Generic;
using System.Linq;
using elZach.Common;
using UnityEngine;

namespace LD57
{
	// VIOLENCE
	public class Mod_Bloodthirsty : Card, IManipulatePower
	{
		public override int circleOfHell => 6;
		public override string Name => "Bloodthirsty";
		public override string Description => $"Increases Power by {powerBoost*100}% but reduces Health by {healthReduction*100}%";
		private float powerBoost = 0.35f;
		private float healthReduction = 0.20f;

		public float ManipulatePower(float value)
		{
			return value * (1 + powerBoost);
		}
	}
	
	public class Mod_Brutal : Card, IInitializeOnCombat
	{
		public override int circleOfHell => 6;
		public override string Name => "Brutal";
		public override string Description => $"Attacks deal {powerBoost*100}% bonus damage when below {healthThreshold*100}% health";
		private float powerBoost = 0.45f; // 45% power boost
		private float healthThreshold = 0.4f; // Triggers below 40% health

		public void OnCombatStart(UnitCombatBehaviour behaviour)
		{
			if (behaviour == null) return;
			
			// Add power calculation function directly using the available behaviour reference
			behaviour.powerChanges.Add((original) => {
				// Check if health is below threshold
				float healthPercent = behaviour.currentHealth / behaviour.Unit.Health;
				if (healthPercent < healthThreshold)
				{
					return original * (1 + powerBoost);
				}
				
				return original;
			});
		}
	}
	
	public class Mod_Enraged : Card, IInitializeOnCombat
	{
		public override int circleOfHell => 6;
		public override string Name => "Enraged";
		public override string Description => $"Taking damage grants {speedBoost*100}% speed for {boostDuration}s (stacks up to {maxStacks} times)";
		private float speedBoost = 0.08f; // 8% speed boost per stack
		private float boostDuration = 5f; // 5 seconds duration
		private int maxStacks = 4; // Maximum 4 stacks (32% total)
		private Dictionary<UnitCombatBehaviour, int> damageCounter = new Dictionary<UnitCombatBehaviour, int>();

		public void OnCombatStart(UnitCombatBehaviour behaviour)
		{
			if (behaviour == null) return;
			
			void OnDamaged(float damage)
			{
				if (!damageCounter.ContainsKey(behaviour))
					damageCounter[behaviour] = 0;
					
				// Only add boost if below max stacks
				if (damageCounter[behaviour] < maxStacks)
				{
					damageCounter[behaviour]++;
					
					// Create a new speed boost function for this damage event
					float SpeedBoostFunc(float original) => original * (1 + speedBoost);
					behaviour.speedChanges.Add(SpeedBoostFunc);
					
					// Remove the speed boost after duration
					async void RemoveBoost()
					{
						await WebTask.Delay(boostDuration);
						behaviour.speedChanges.Remove(SpeedBoostFunc);
						damageCounter[behaviour]--;
					}
					RemoveBoost();
					
					Debug.Log($"{behaviour.Unit.name} is Enraged! Speed boost stacks: {damageCounter[behaviour]}");
				}
			}
			
			// Subscribe to damage events
			behaviour.OnIGotHurt += OnDamaged;
		}
	}
	
	public class Mod_Merciless : Card, IManipulateHealth, IManipulateSpeed
	{
		public override int circleOfHell => 6;
		public override string Name => "Merciless";
		public override string Description => $"Sacrifices defense for offense: {healthReduction*100}% less Health but {speedBoost*100}% more Speed";
		private float healthReduction = 0.25f; // 25% health reduction
		private float speedBoost = 0.4f; // 40% speed boost

		public float ManipulateHealth(float value)
		{
			return value * (1 - healthReduction);
		}

		public float ManipulateSpeed(float value)
		{
			return value * (1 + speedBoost);
		}
	}
	
	public class Massacre : ActiveCard
	{
		public override int circleOfHell => 6;
		public override string Name => "Massacre";
		public override string Description => $"Unleashes a violent barrage | Deals ({baseDamage}) damage to all enemies, plus ({bonusDamage}) extra to the weakest one every {cooldown}s";
		public override float cooldown => 10f;
		public float baseDamage = 5f;
		public float bonusDamage = 7f;

		public override void Activate(UnitCombatBehaviour activator)
		{
			var enemies = CombatManager.GetEnemies(activator).ToList();
			if (enemies.Count == 0) return;
			
			Debug.Log($"{activator.Unit.name} performs a {Name} on all enemies!");
			
			// Deal base damage to all enemies
			foreach (var enemy in enemies)
			{
				enemy.Damage(activator.PowerCalc * baseDamage);
			}
			
			// Find the weakest enemy and deal bonus damage
			var weakestEnemy = enemies.MinBy(x => x.currentHealth);
			if (weakestEnemy != null && weakestEnemy.currentHealth > 0)
			{
				weakestEnemy.Damage(activator.PowerCalc * bonusDamage);
				Debug.Log($"{activator.Unit.name} deals extra damage to {weakestEnemy.Unit.name}!");
			}
		}
	}
}