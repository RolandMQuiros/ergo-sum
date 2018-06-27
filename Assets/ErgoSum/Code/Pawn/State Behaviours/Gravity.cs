using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum.States {
	public class Gravity : PawnStateBehaviour {
		public float Acceleration;
		public override void OnStateEnter(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
			AddStreams(
				Pawn.Body.FixedUpdateAsObservable().Subscribe(
					_ => Pawn.Body.AddRelativeForce(Pawn.Body.transform.up * Acceleration, ForceMode.Acceleration)
				)
			);
		}
	}
}