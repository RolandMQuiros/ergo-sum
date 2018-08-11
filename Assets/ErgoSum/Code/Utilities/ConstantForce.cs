using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ErgoSum {
	[RequireComponent(typeof(Rigidbody))]
	public class ConstantForce : MonoBehaviour {
		public ForceMode ForceMode;
		public Vector3 Force;
		public bool IsRelative;
		private Rigidbody _rigidbody;
		private void Awake() {
			_rigidbody = GetComponent<Rigidbody>();
		}
		private void FixedUpdate() {
			if (Force != Vector3.zero) {
				if (IsRelative) {
					_rigidbody.AddRelativeForce(Force, ForceMode);
				} else {
					_rigidbody.AddForce(Force, ForceMode);
				}
			}
		}
	}
}