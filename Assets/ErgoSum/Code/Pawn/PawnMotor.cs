using UnityEngine;
using UniRx;

namespace ErgoSum {
	[RequireComponent(typeof(Rigidbody))]
	public abstract class PawnMotor : MonoBehaviour {
		public abstract IObservable<Vector3> Movement { get; }
		public abstract void Move(Vector3 velocity);
	}
}