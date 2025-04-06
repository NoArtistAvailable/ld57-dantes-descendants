using System.Collections.Generic;
using UnityEngine;

namespace LD57
{
	public class MapBehaviour : MonoBehaviour
	{
		public List<GameObject> stations;
		private Camera cam;
		public Vector3 offset = new Vector3(0,0,-17);
		void Start()
		{
			cam = Camera.main;
			
			stations[PlayerManager.instance.circleOfHell].GetComponentInChildren<SpriteRenderer>(true).gameObject.SetActive(true);
			
			if (PlayerManager.instance.circleOfHell == 0) return;
			cam.transform.position = stations[PlayerManager.instance.circleOfHell - 1].transform.position + offset;
		}

		void Update()
		{
			cam.transform.position = Vector3.Lerp(cam.transform.position, stations[PlayerManager.instance.circleOfHell].transform.position + offset, Time.deltaTime * 3f);
		}
	}
}