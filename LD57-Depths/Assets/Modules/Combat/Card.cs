using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LD57
{
	[Serializable]
	public abstract class Card
	{
		public abstract int circleOfHell { get; }
		public abstract string Name { get; }
		public abstract string Description { get; }
	}

	public abstract class ActiveCard : Card
	{
		public abstract float cooldown { get; }
		public abstract void Activate(Unit activator);
	}
	
	public static class CardManager
	{
		private static List<Card> GetAllCardTypes()
		{
			List<Card> allCards = new List<Card>();
            
			// Get all types that inherit from Card
			Type[] cardTypes = Assembly.GetAssembly(typeof(Card))
				.GetTypes()
				.Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Card)))
				.ToArray();
            
			// Create an instance of each card type
			foreach (Type cardType in cardTypes)
			{
				try
				{
					Card cardInstance = (Card)Activator.CreateInstance(cardType);
					allCards.Add(cardInstance);
				}
				catch (Exception e)
				{
					Debug.LogError($"Failed to create instance of {cardType.Name}: {e.Message}");
				}
			}
            
			return allCards;
		}
		public static List<Card> AllCards = GetAllCardTypes();
	}

	public class Mod_Healthy : Card, IManipulateHealth
	{
		public override int circleOfHell => 1;
		public override string Name => "Healthy";
		public override string Description => "Increases Health";
		public float healthMult = 1.5f;
		public float ManipulateHealth(float value)
		{
			return value * healthMult;
		}
	}
	public class Mod_Strong : Card, IManipulatePower
	{
		public override int circleOfHell => 1;
		public override string Name => "Strong";
		public override string Description => "Increases Power";
		public float powerMult = 1.3f;
		public float ManipulatePower(float value)
		{
			return value * powerMult;
		}
	}
	public class Mod_Smart : Card, IManipulateSpeed
	{
		public override int circleOfHell => 1;
		public override string Name => "Smart";
		public override string Description => "Increases Speed";
		public float speedMult = 1.2f;
		public float ManipulateSpeed(float value)
		{
			return value * speedMult;
		}
	}
}