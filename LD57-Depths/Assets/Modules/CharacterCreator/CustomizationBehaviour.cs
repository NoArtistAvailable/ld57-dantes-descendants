using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LD57
{
	public class CustomizationBehaviour : MonoBehaviour
	{
		public SpriteRenderer target;
		public List<SpriteOffset> offsets = new List<SpriteOffset>();
		public SpriteOffset current { get; set; }
		public new Renderer renderer;

		private void OnEnable()
		{
			renderer = GetComponent<Renderer>();
			Application.onBeforeRender += Do;
		}

		private void OnDisable()
		{
			Application.onBeforeRender -= Do;
		}

		void Do()
		{
			if (current == null || target.sprite != current.sprite)
			{
				var found = offsets.FirstOrDefault(x => x.sprite == target.sprite);
				if (found != null)
				{
					transform.localPosition = found.positionOffset;
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

	[Serializable]
	public class SpriteOffset
	{
		public Sprite sprite;
		public Vector3 positionOffset;
		public Vector3 rotationOffset;
	}
}