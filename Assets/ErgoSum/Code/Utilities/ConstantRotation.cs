using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ErgoSum.Utilities {
	[RequireComponent(typeof(Rigidbody))]
	public class ConstantRotation : MonoBehaviour {
		public Vector3 Velocity;
		public bool IsRelative;
		private Rigidbody _rigidbody;
		private void Awake() {
			_rigidbody = GetComponent<Rigidbody>();
		}

		private void FixedUpdate() {
			Vector3 move = Velocity * Time.deltaTime;
			if (IsRelative) {
				_rigidbody.transform.localRotation *= Quaternion.Euler(move);
			} else {
				_rigidbody.transform.rotation *= Quaternion.Euler(move);
			}
		}
	}
}