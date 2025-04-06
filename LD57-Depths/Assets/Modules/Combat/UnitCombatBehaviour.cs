using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using elZach.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace LD57
{
    public class UnitCombatBehaviour : MonoBehaviour, IPointerEnterHandler
    {
        public static event Action<UnitCombatBehaviour> OnShowCombatUnit;
        public static event Action<UnitCombatBehaviour> OnDeath;
        public static event Action<UnitCombatBehaviour, float> OnHurt, OnHeal;
        public event Action<float> OnIGotHurt, OnIGotHeal;
        public event Action<UnitCombatBehaviour, ActiveCard> OnCardActivated;
        public event Action OnCrit;
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

        public float PowerCalc
        {
            get
            {
                var power = cardPower;
                foreach (var func in powerChanges) power = func.Invoke(power);
                var crit = random.NextFloat() < CritCalc;
                if (crit)
                {
                    power *= 2f;
                    OnCrit?.Invoke();
                }
                return power;
            }
        }
        public List<Func<float, float>> powerChanges = new List<Func<float, float>>();
        
        public float SpeedCalc
        {
            get
            {
                var value = cardSpeed;
                foreach (var func in speedChanges) value = func.Invoke(value);
                return value;
            }
        }
        public List<Func<float, float>> speedChanges = new List<Func<float, float>>();
        public float CritCalc
        {
            get
            {
                var value = cardCrit;
                foreach (var func in critChanges) value = func.Invoke(value);
                return value;
            }
        }
        public List<Func<float, float>> critChanges = new List<Func<float, float>>();
        public float ReceiveDamageCalc
        {
            get
            {
                var value = 1f;
                foreach (var func in receiveDamageChanges) value = func.Invoke(value);
                return value;
            }
        }
        public List<Func<float, float>> receiveDamageChanges = new List<Func<float, float>>();

        public TextMeshProUGUI name1, name2;
        public RectMask2D healthMask;
        public TextMeshProUGUI ability1, ability2;
        public RectMask2D abilityMask;
        public SpriteAnimator animator;
        public CustomizationBehaviour customization;
        private AudioSource voice;
        private Unity.Mathematics.Random random;
        
        public void Init(Unit unit)
        {
            this.Unit = unit;
            name1.text = unit.name;
            name2.text = unit.name;
            currentHealth = unit.Health;
            cardPower = unit.Power;
            cardSpeed = unit.Speed;
            cardCrit = unit.Crit;
            customization.renderer.material.SetTexture("_MainTex", unit.faceTexture);
            voice = GetComponentInChildren<AudioSource>();
            random = new Unity.Mathematics.Random((uint)unit.seed);
            voice.pitch = random.NextFloat(0.85f, 1.1f);

            foreach(var toInit in unit.cards.OfType<IInitializeOnCombat>()) toInit?.OnCombatStart(this);
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
                OnCardActivated?.Invoke(this, NextActiveCard);
                chargeTime = 0;
                PlayAnimation("Attack", 0.6f);
                GetNextAbilityCard();
                voice.clip = AudioLibrary.GetSwear();
                voice.Play();
            }
        }

        public void Damage(float damage)
        {
            damage *= ReceiveDamageCalc;
            currentHealth -= damage;
            if (currentHealth <= 0) Die();
            else
            {
                PlayAnimation("Hurt", 0.36f);
                OnIGotHurt?.Invoke(damage);
                OnHurt?.Invoke(this, damage);
            }
        }
        public void Heal(float value)
        {
            if (currentHealth <= 0) return;
            currentHealth += value;
            OnIGotHeal?.Invoke(value);
            OnHeal?.Invoke(this, value);
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