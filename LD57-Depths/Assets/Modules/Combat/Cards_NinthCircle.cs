using System;
using System.Collections.Generic;
using System.Linq;
using elZach.Common;
using UnityEngine;

namespace LD57
{
	// TREACHERY
	public class Mod_Traitorous : Card, IInitializeOnCombat
	{
		public override int circleOfHell => 8;
		public override string Name => "Traitorous";
		public override string Description => $"Betrays allies | All allies lose {healthPenalty*100}% Health at the start of combat";
		private float healthPenalty = 0.2f; // 20% health penalty to allies
		
		public void OnCombatStart(UnitCombatBehaviour behaviour)
		{
			if (behaviour == null) return;
			
			// Get all allies
			var allies = CombatManager.GetFriends(behaviour).Where(x => x != behaviour);
			
			// Apply health penalty to all allies
			foreach (var ally in allies)
			{
				ally.currentHealth *= (1 - healthPenalty);
				Debug.Log($"{behaviour.Unit.name}'s Traitorous nature reduces {ally.Unit.name}'s health!");
			}
		}
	}
	
	public class Mod_Oathbreaker : Card, IInitializeOnCombat
	{
		public override int circleOfHell => 8;
		public override string Name => "Oathbreaker";
		public override string Description => $"Breaking oaths has consequences | Loses {healthLossPerSecond} Health per second during combat";
		private float healthLossPerSecond = 1.5f; // 1.5 health lost per second
		
		public void OnCombatStart(UnitCombatBehaviour behaviour)
		{
			if (behaviour == null) return;
			
			// Apply continuous health drain
			async void DrainHealth()
			{
				while (CombatManager.Active && behaviour.currentHealth > 0)
				{
					await WebTask.Delay(1f); // Wait 1 second
					if (behaviour.currentHealth > 0)
					{
						behaviour.Damage(healthLossPerSecond);
						Debug.Log($"{behaviour.Unit.name} loses {healthLossPerSecond} health from Oathbreaker's curse!");
					}
				}
			}
			DrainHealth();
		}
	}
	
	public class Mod_Judas : Card, IInitializeOnCombat
	{
		public override int circleOfHell => 8;
		public override string Name => "Judas";
		public override string Description => $"The betrayer's curse | Takes {damageIncrease*100}% more damage from all attacks";
		private float damageIncrease = 0.6f; // 60% more damage taken
		
		public void OnCombatStart(UnitCombatBehaviour behaviour)
		{
			if (behaviour == null) return;
			
			// Take increased damage from attacks
			behaviour.receiveDamageChanges.Add((original) => original * (1 + damageIncrease));
		}
	}
	
	public class Mod_SoulSeller : Card, IInitializeOnCombat
	{
		public override int circleOfHell => 8;
		public override string Name => "Soul Seller";
		public override string Description => $"Sold soul to darkness | Each ability use costs {healthCost} Health";
		private float healthCost = 6f; // 6 health per ability use
		
		public void OnCombatStart(UnitCombatBehaviour behaviour)
		{
			if (behaviour == null) return;
			
			// Subscribe to card activation
			behaviour.OnCardActivated += (unit, card) => {
				// Sacrifice health
				unit.Damage(healthCost);
				Debug.Log($"{unit.Unit.name} pays {healthCost} health as the price for using an ability!");
			};
		}
	}
	
	public class Betrayal : ActiveCard
	{
		public override int circleOfHell => 8;
		public override string Name => "Betrayal";
		public override string Description => $"Ultimate treachery | Damages all allies for ({allyDamage}) and reduces their speed by {speedReduction*100}% for {debuffDuration}s every {cooldown}s";
		public override float cooldown => 10f;
		public float allyDamage = 6f; // Damage to allies
		public float speedReduction = 0.4f; // 40% speed reduction
		public float debuffDuration = 5f; // 5 seconds duration
		
		public override void Activate(UnitCombatBehaviour activator)
		{
			Debug.Log($"{activator.Unit.name} commits {Name} against all allies!");
			
			// Damage all allies (including self) and apply speed debuff
			var allies = CombatManager.GetFriends(activator);
			foreach (var ally in allies)
			{
				// Deal damage
				ally.Damage(activator.PowerCalc * allyDamage);
				
				// Apply speed reduction
				float SpeedDebuff(float original) => original * (1 - speedReduction);
				ally.speedChanges.Add(SpeedDebuff);
				
				// Remove debuff after duration
				async void RemoveDebuff()
				{
					await WebTask.Delay(debuffDuration);
					ally.speedChanges.Remove(SpeedDebuff);
				}
				RemoveDebuff();
				
				Debug.Log($"{activator.Unit.name} betrays {ally.Unit.name}, reducing their speed!");
			}
		}
	}
}