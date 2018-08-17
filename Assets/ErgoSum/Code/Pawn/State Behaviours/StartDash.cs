using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using ErgoSum;
using ErgoSum.Utilities;

namespace ErgoSum.States {
	public class StartDash : PawnStateBehaviour {
		[SerializeField]private bool _canDashWhileAirborne;
		public override void OnStateEnter(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
            AddStreams(
                Pawn.Controller.Movement.Subscribe(unit => {
					if (unit.DashStart && (Pawn.IsGrounded.Value || !_canDashWhileAirborne)) {
						stateMachine.SetBool(PawnStateParameters.Dash, true);
					}
				})
			);
		}
	}
}