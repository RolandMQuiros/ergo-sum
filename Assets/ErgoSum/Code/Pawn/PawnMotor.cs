using UnityEngine;

namespace ErgoSum {
	[RequireComponent(typeof(Rigidbody))]
	public abstract class PawnMotor : MonoBehaviour {
		public abstract void Move(Vector3 velocity);
	}
}