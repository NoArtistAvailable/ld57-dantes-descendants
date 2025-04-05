using System;
using elZach.Common;
using TMPro;
using UnityEngine;

namespace LD57
{
	public class UnitInspectorBehaviour : MonoBehaviour
	{
		public Animatable anim;
		public TextMeshProUGUI unitNameField;
		public Transform cardContentParent;
		public CardBehaviour cardBehaviourPrefab;
		public Unit toShow { get; private set; }
		void Start()
		{
			UnitShopBehaviour.OnShowShopUnit += Show;
		}

		private void OnDestroy()
		{
			UnitShopBehaviour.OnShowShopUnit -= Show;
		}

		
		private async void Show(UnitShopBehaviour obj)
		{
			toShow = obj.unit;
			await anim.Play(0);
			if (toShow != obj.unit) return;
			unitNameField.text = $"{obj.unit.name} <size=16> HP: {obj.unit.Health} SPD: {obj.unit.Speed*100}% POW: {obj.unit.Power*100}% CRT: {obj.unit.Crit * 100}% </size>" ;
			
			cardContentParent.ClearChildren();
			foreach (var card in obj.unit.cards)
			{
				var clone = Instantiate(cardBehaviourPrefab, cardContentParent);
				clone.Init(card);
			}
			
			anim.PlayAt(1);
		}
	}
}