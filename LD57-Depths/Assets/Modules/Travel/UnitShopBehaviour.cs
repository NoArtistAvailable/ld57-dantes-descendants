using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LD57
{
    public class UnitShopBehaviour : MonoBehaviour
    {
        public static event Action<UnitShopBehaviour> OnShowShopUnit;
        public enum State{Idle, Active, Finished}
        public State state;
        
        public TextMeshProUGUI namePlate;
        public Image bodySprite;

        public Color regularColor;
        public Color shopSelectedColor;

        public SpriteAnimator.Clip idle;
        public SpriteAnimator.Clip active;
        public SpriteAnimator.Clip finished;
        
        
        public Unit unit { get; set; }

        public void Init(Unit unit)
        {
            if (unit == null)
            {
                Debug.LogWarning("NULL UNIT!", this);
                return;
            }
            this.unit = unit;
            namePlate.text = unit.name;
        }

        void OnEnable()
        {
            var triggersBehaviour = this.GetComponent<EventTrigger>();
            var onSelect = triggersBehaviour.triggers.FirstOrDefault(x => x.eventID == EventTriggerType.Select);
            if(onSelect != null) onSelect.callback.AddListener(evt=>OnShowShopUnit?.Invoke(this));
        }

        private float time = 0;

        void Update()
        {
            time += Time.deltaTime;
            SpriteAnimator.Clip currentClip;
            switch (state)
            {
                case State.Idle:
                    currentClip = idle;
                    break;
                case State.Active:
                    currentClip = active;
                    break;
                case State.Finished:
                    currentClip = finished;
                    break;
                default:
                    currentClip = idle;
                    break;
            }

            time %= currentClip.time * currentClip.frames.Length;
            var frameIndex = Mathf.FloorToInt(time / currentClip.time);
            bodySprite.sprite = currentClip.frames[frameIndex];
        }

        public void ShopSelect()
        {
            var selectable = GetComponent<Selectable>();
            var colors = selectable.colors;
            colors.normalColor = shopSelectedColor;
            selectable.colors = colors;
            OnShowShopUnit?.Invoke(this);
        }

        public void ShopDeselect()
        {
            var selectable = GetComponent<Selectable>();
            var colors = selectable.colors;
            colors.normalColor = regularColor;
            selectable.colors = colors;
        }
    }
}