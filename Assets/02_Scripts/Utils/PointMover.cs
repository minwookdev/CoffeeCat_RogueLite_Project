/// CODER	      :		
/// MODIFIED DATE : 
/// IMPLEMENTATION: 
using System;
using UnityEngine;

namespace CoffeeCat.Prototype {
	public class PointMover : MonoBehaviour {
		[SerializeField] private Transform pointTr = null;
		[SerializeField] private Transform tr = null;
		private Camera mainCam = null;

		private void Start() {
			mainCam = Camera.main;
		}

		private void MoveByPoint(Vector2 pos) {
			Vector2 destPosition = pos - (Vector2)(pointTr.position - tr.position);
			tr.position = destPosition;
		}

		private void Update() {
			// Click Mouse Left
			if (!Input.GetMouseButton(0) || !mainCam) return;
			
			var mouseClickPosition = Input.mousePosition;
			mouseClickPosition.z = mainCam.nearClipPlane;
			var mouseClickWorldPosition = mainCam.ScreenToWorldPoint(mouseClickPosition);
			MoveByPoint(mouseClickWorldPosition);
		}
	}
}
