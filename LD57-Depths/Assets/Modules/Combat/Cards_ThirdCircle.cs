using System.Linq;
using elZach.Common;
using UnityEngine;

namespace LD57
{
	// GLUTTONY
	public class Mod_Ravenous : Card, IManipulatePower
	{
		public override int circleOfHell => 2;
		public override string Name => "Ravenous";
		public override string Description => "Power increases as health decreases (up to 60% boost at low health)";
		public float powerBoostMax = 0.6f; // 60% boost at minimum health

		public float ManipulatePower(float value)
		{
			// Calculate power boost based on current health percentage
			UnitCombatBehaviour behaviour = CombatManager.instance.playerSquad.FirstOrDefault(x => x.Unit.cards.Contains(this));
			if (behaviour == null) return value;
			
			float healthPercent = behaviour.currentHealth / behaviour.Unit.Health;
			float boost = Mathf.Lerp(powerBoostMax, 0f, healthPercent);
			return value * (1f + boost);
		}
	}

	public class Mod_Bloated : Card, IManipulateHealth
	{
		public override int circleOfHell => 2;
		public override string Name => "Bloated";
		public override string Description => "Increases max health by 40% but decreases speed by 15%";
		public float healthMult = 1.4f;

		public float ManipulateHealth(float value)
		{
			return value * healthMult;
		}
	}

	public class Gorge : ActiveCard
	{
		public override int circleOfHell => 2;
		public override string Name => "Gorge";
		public override string Description => $"Consume an enemy's health to restore your own | Steals ({healthSteal}) health every {cooldown}s";
		public override float cooldown => 4f;
		public float healthSteal = 4f;

		public override void Activate(UnitCombatBehaviour activator)
		{
			var target = CombatManager.GetHealthiestEnemy(activator);
			if (!target) return;
			
			Debug.Log($"{activator.Unit.name} {Name}s on {target.Unit.name}");
			float stolenHealth = Mathf.Min(healthSteal * activator.PowerCalc, target.currentHealth);
			target.Damage(stolenHealth);
			activator.Heal(stolenHealth);
		}
	}
	
	public class Vomit : ActiveCard
	{
		public override int circleOfHell => 2;
		public override string Name => "Vomit";
		public override string Description => $"Violently expel stomach contents | Damages all enemies for ({damage}) every {cooldown}s";
		public override float cooldown => 6f;
		public float damage = 3f;

		public override void Activate(UnitCombatBehaviour activator)
		{
			var targets = CombatManager.GetEnemies(activator);
			Debug.Log($"{activator.Unit.name} {Name}s on everyone");
			
			// Higher damage compared to Scream because of longer cooldown
			foreach(var target in targets) 
			{
				target.Damage(activator.PowerCalc * damage);
			}
		}
	}
	
	public class Feast : ActiveCard
	{
		public override int circleOfHell => 2;
		public override string Name => "Feast";
		public override string Description => $"Host a gluttonous feast | Heals all allies for ({healAmount}) and speeds them up ({speedBoost*100}%) for {boostDuration}s every {cooldown}s";
		public override float cooldown => 8f;
		public float healAmount = 3f;
		public float speedBoost = 0.25f; // 25% speed boost
		public float boostDuration = 4f;

		public override void Activate(UnitCombatBehaviour activator)
		{
			var friends = CombatManager.GetFriends(activator);
			Debug.Log($"{activator.Unit.name} hosts a {Name} for allies");
			
			foreach (var friend in friends)
			{
				friend.Heal(activator.PowerCalc * healAmount);
				
				float SpeedBuff(float original) => original * (1 + speedBoost);
				friend.speedChanges.Add(SpeedBuff);
				async void RemoveSpeedChange()
				{
					await WebTask.Delay(boostDuration);
					friend.speedChanges.Remove(SpeedBuff);
				}
				RemoveSpeedChange();
			}
		}
	}
	
	public class Consume : ActiveCard
	{
		public override int circleOfHell => 2;
		public override string Name => "Consume";
		public override string Description => $"Consume the weakest enemy | Deals ({damage}) damage with ({executeThreshold*100}%) execute chance on low health enemies every {cooldown}s";
		public override float cooldown => 5f;
		public float damage = 5f;
		public float executeThreshold = 0.25f; // Execute enemies below 25% health

		public override void Activate(UnitCombatBehaviour activator)
		{
			// Target the enemy with the lowest health
			var enemies = CombatManager.GetEnemies(activator).ToList();
			if (enemies.Count == 0) return;
			
			var target = enemies.MinBy(x => x.currentHealth);
			if (!target) return;
			
			Debug.Log($"{activator.Unit.name} {Name}s {target.Unit.name}");
			
			// Check if the target is below the execute threshold
			if (target.currentHealth / target.Unit.Health < executeThreshold)
			{
				// Execute (kill instantly)
				target.Damage(target.currentHealth);
			}
			else
			{
				// Regular damage
				target.Damage(activator.PowerCalc * damage);
			}
		}
	}
}