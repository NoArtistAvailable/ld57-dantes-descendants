using System;
using System.Collections.Generic;
using System.Linq;
using elZach.Common;
using UnityEngine;
using UnityEngine.UI;

namespace LD57
{
	public class ShopMenu : MonoBehaviour
	{
		public Transform squadPanel;
		public AnimatableChildren cardParent;
		public Button exitButton;
		public Unit selectedUnit
		{
			get => _selectedUnit;
			set
			{
				if (_selectedUnit == value) return;
				if (_selectedUnit != null)
				{
					var unitCard = unitCards.First(x=> x.unit == _selectedUnit);
					unitCard.state = UnitShopBehaviour.State.Finished;
					unitCard.ShopDeselect();
				}

				if (value != null)
				{
					var unitCard = unitCards.First(x=> x.unit == value);
					unitCard.state = UnitShopBehaviour.State.Active;
					unitCard.ShopSelect();
				}
				_selectedUnit = value;
			}
		}
		private Unit _selectedUnit = null;

		private UnitShopBehaviour[] unitCards;
		
		void Start()
		{
			SetRandomCards();
			CardBehaviour.onSubmission += OnCardSubmitted;
		}

		private void OnDestroy()
		{
			CardBehaviour.onSubmission -= OnCardSubmitted;
		}

		public void OpenShop()
		{
			GetComponent<Animatable>()?.PlayAt(1);
			var squad = PlayerManager.GetOrInitSquad();
			unitCards = squadPanel.GetComponentsInChildren<UnitShopBehaviour>();
			for (int i = 0; i < unitCards.Length; i++)
			{
				unitCards[i].Init(squad[i]);
			}
			var next = GetNextUnit(selectedUnit);
			if(next!=null) Debug.Log($"Selected ({PlayerManager.instance.squad.IndexOf(next)}) {next.name}");
			else Debug.Log("no next character??!");
			selectedUnit = next;
		}

		public Unit GetNextUnit(Unit previousUnit)
		{
			if (previousUnit == null)
			{
				Debug.Log("this happens");
				PlayerManager.GetOrInitSquad();
				return PlayerManager.instance.squad[0];
			}
			int index = PlayerManager.instance.squad.IndexOf(previousUnit);
			index++;
			if (index >= PlayerManager.instance.squad.Count) return null;
			return PlayerManager.instance.squad[index];
		}
		
		public void SetRandomCards()
		{
			var cardBehaviours = GetComponentsInChildren<CardBehaviour>();
			// add selection base on circle of hell
			var potentialCards = CardManager.AllCards.Where(x => x.circleOfHell == PlayerManager.instance.circleOfHell).ToList();
			for (int i = 0; i < cardBehaviours.Length; i++)
			{
				var chosen = potentialCards.GetRandom();
				potentialCards.Remove(chosen);
				cardBehaviours[i].Init(chosen);
			}
		}
		
		private async void OnCardSubmitted(CardBehaviour cardBehaviour)
		{
			selectedUnit.cards.Add(cardBehaviour.card);
			Debug.Log($"added {cardBehaviour.card.Name} to {selectedUnit.name}");
			await cardParent.Play(0);

			selectedUnit = GetNextUnit(selectedUnit);
			if (selectedUnit == null)
			{
				Debug.Log("Finished Shop Experience :3");
				GetComponent<Animatable>()?.PlayAt(0);
				exitButton.GetComponent<Animatable>().PlayAt(1);
				return;
			}
			SetRandomCards();
			await cardParent.Play(1);
		}
	}
}