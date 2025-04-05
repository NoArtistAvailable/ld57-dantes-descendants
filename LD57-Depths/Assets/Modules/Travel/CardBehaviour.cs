using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LD57
{
	public class CardBehaviour : MonoBehaviour
	{
		public static event Action<CardBehaviour> onSubmission;
		
		public Card card;
		public TextMeshProUGUI titleText;
		public TextMeshProUGUI descriptionText;
		

		public void Init(Card card)
		{
			this.card = card;
			titleText.text = card.Name;
			descriptionText.text = card.Description;
		}

		public void Start()
		{
			var eventTrigger = GetComponent<EventTrigger>();
			if (!eventTrigger) return;
			var submissionTrigger = eventTrigger.triggers.FirstOrDefault(x => x.eventID == EventTriggerType.Submit);
			submissionTrigger.callback.AddListener(evt => onSubmission?.Invoke(this));
		}
	}
}