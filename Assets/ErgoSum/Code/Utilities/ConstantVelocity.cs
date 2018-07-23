using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ErgoSum.Utilities {
	[RequireComponent(typeof(Rigidbody))]
	public class ConstantVelocity : MonoBehaviour {
		public Vector3 Velocity;
		public bool IsRelative;
		private Rigidbody _rigidbody;
		private void Awake() {
			_rigidbody = GetComponent<Rigidbody>();
		}

		private void FixedUpdate() {
			Vector3 move = Velocity;
			if (IsRelative) {
				move = Velocity.x * _rigidbody.transform.right +
					Velocity.y * _rigidbody.transform.up +
					Velocity.z * _rigidbody.transform.forward;
			}
			_rigidbody.MovePosition(_rigidbody.transform.position + move * Time.deltaTime);
		}
	}
}