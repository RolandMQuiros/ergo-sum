using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum.States {
	public class Gravity : PawnStateBehaviour {
		public float Acceleration;
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			AddStreams(
				Pawn.RigidBody.FixedUpdateAsObservable().Subscribe(
					_ => Pawn.RigidBody.AddRelativeForce(Pawn.RigidBody.transform.up * Acceleration, ForceMode.Acceleration)
				)
			);
		}
	}
}