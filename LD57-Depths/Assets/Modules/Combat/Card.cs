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
		public abstract void Activate(UnitCombatBehaviour unitCombatBehaviour);
		public virtual string animName { get; } = "Attack";
	}
	
	public static class CardManager
	{
		public static Type[] cardTypes;
		private static List<Card> GetAllCardTypes()
		{
			List<Card> allCards = new List<Card>();
            
			// Get all types that inherit from Card
			cardTypes = Assembly.GetAssembly(typeof(Card))
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
}