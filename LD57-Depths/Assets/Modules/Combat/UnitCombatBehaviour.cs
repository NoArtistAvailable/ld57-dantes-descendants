using System;
using System.Linq;
using System.Threading;
using elZach.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LD57
{
    public class UnitCombatBehaviour : MonoBehaviour, IPointerEnterHandler
    {
        public static event Action<UnitCombatBehaviour> OnShowCombatUnit;
        public static event Action<UnitCombatBehaviour> OnDeath;
        public Unit Unit { get; set; }
        public ActiveCard NextActiveCard { get; set; }
        public bool isPlayerSquad;

        public float currentHealth
        {
            get => _currentHealth;
            set
            {
                healthMask.padding = Vector3.forward * (1 - (value / Unit.Health)) * healthMask.rectTransform.rect.width;
                _currentHealth = value;
            }
        }
        private float _currentHealth;
        public float chargeTime { get; set; }

        private float cardPower, cardSpeed, cardCrit;
        
        public float PowerCalc => cardPower;
        public float SpeedCalc => cardSpeed;
        public float CritCalc => cardCrit;

        public TextMeshProUGUI name1, name2;
        public RectMask2D healthMask;
        public TextMeshProUGUI ability1, ability2;
        public RectMask2D abilityMask;
        public SpriteAnimator animator;
        
        public void Init(Unit unit)
        {
            this.Unit = unit;
            name1.text = unit.name;
            name2.text = unit.name;
            currentHealth = unit.Health;
            cardPower = unit.Power;
            cardSpeed = unit.Speed;
            cardCrit = unit.Crit;

            GetNextAbilityCard();
        }

        public void GetNextAbilityCard()
        {
            var potentialCards = Unit.cards.OfType<ActiveCard>().ToList();
            if (NextActiveCard == null) NextActiveCard = potentialCards[0];
            else
            {
                var index = potentialCards.IndexOf(NextActiveCard);
                index++;
                index %= potentialCards.Count;
                NextActiveCard = potentialCards[index];
            }
            ability1.text = NextActiveCard.Name;
            ability2.text = NextActiveCard.Name;
        }

        public void Update()
        {
            if (Unit == null || !CombatManager.Active || currentHealth <= 0 || NextActiveCard == null) return;
            chargeTime += Time.deltaTime * SpeedCalc;
            abilityMask.padding = Vector3.forward * (1-(chargeTime / NextActiveCard.cooldown)) * abilityMask.rectTransform.rect.width;
            if (chargeTime >= NextActiveCard.cooldown)
            {
                NextActiveCard.Activate(this);
                chargeTime = 0;
                PlayAnimation("Attack", 0.6f);
                GetNextAbilityCard();
            }
        }

        public void Damage(float damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0) Die();
            else PlayAnimation("Hurt", 0.36f);
        }

        public void Die()
        {
            animationCancel?.Cancel();
            animationCancel?.Dispose();
            animationCancel = null;
            animator.Play("Dead");
            OnDeath?.Invoke(this);
        }

        private CancellationTokenSource animationCancel;
        public async void PlayAnimation(string clipName, float duration)
        {
            animationCancel?.Cancel();
            animationCancel?.Dispose();
            var cancel = new CancellationTokenSource();
            animationCancel = cancel;
            animator.Play(clipName);
            await WebTask.Delay(duration);
            if (cancel.IsCancellationRequested) return;
            cancel.Dispose();
            animationCancel = null;
            if(animator.current.name == clipName) animator.Play("Idle");
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnShowCombatUnit?.Invoke(this);
        }

    }
}