using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using ErgoSum;

namespace ErgoSum.States {
	public class Movement : PawnStateBehaviour {
		[SerializeField]private float _speed;
		public override void OnStateEnter(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
            AddStreams(
                Pawn.Controller.Movement.Subscribe(unit => {
					if (unit.DashStart) {
						stateMachine.SetBool("Dash", true);
					} else {
                        Pawn.Motor.Move(this._speed * unit.Direction * Time.deltaTime);
					}
				})
			);
		}
	}
}