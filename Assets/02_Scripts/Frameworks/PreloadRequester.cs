// CODER	      : MIN WOOK KIM		
// MODIFIED DATE  : 2023. 08. 31
// IMPLEMENTATION : Preloader를 사용해 필요한 리소스를 사전에 로드하는 컴포넌트
using System;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
	public class PreloadRequester : MonoBehaviour {
		[Title("REQUEST")]
		[SerializeField] private bool IsRequestOnAwake = true;
		[SerializeField] private Request[] requests;
		
		// Properties
		/// <summary>
		/// Do Not Use In Update
		/// </summary>
		private bool IsAllRequestsCompleted => requests.All(request => request.IsCompleted);

		private void Awake() {
			if (!IsRequestOnAwake)
				return;

			RequestAll();
		}

		private void RequestAll() {
			foreach (var request in requests) {
				Preloader.Process(request.key.ToStringEx(), request.RequestComplete);
			}
		}

		public void AddRequest(AddressablesKey key, bool isActive = false) {
			// Requests Array에 포함..?
			Preloader.Process(key.ToStringEx());
		}

		[Serializable]
		public class Request {
			public AddressablesKey key = AddressablesKey.NONE;
			public bool IsCompleted { get; private set; } = false;
			public void RequestComplete() => IsCompleted = true;
		}
	}
}
