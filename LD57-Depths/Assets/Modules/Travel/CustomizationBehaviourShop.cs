using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LD57
{
	public class CustomizationBehaviourShop : MonoBehaviour
	{
		public Image target;
		public List<SpriteOffset> offsets = new List<SpriteOffset>();
		public float multiplier = 100;
		public SpriteOffset current { get; set; }
		public new RawImage renderer;

		private void OnEnable()
		{
			renderer = GetComponent<RawImage>();
			// Application.onBeforeRender += Do;
		}

		private void OnDisable()
		{
			// Application.onBeforeRender -= Do;
		}

		void LateUpdate()
		{
			if (current == null || target.sprite != current.sprite)
			{
				var found = offsets.FirstOrDefault(x => x.sprite == target.sprite);
				if (found != null)
				{
					renderer.rectTransform.localPosition = found.positionOffset * multiplier;
					transform.localRotation = Quaternion.Euler(found.rotationOffset);
					current = found;
					renderer.enabled = true;
				}
				else
				{
					current = null;
					renderer.enabled = false;
				}
			}
		}
	}
}