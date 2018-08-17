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
					Vector3 velocity = _speed * unit.Direction;
					float speed = velocity.magnitude;
					Pawn.Motor.Move(velocity * Time.deltaTime);

					stateMachine.SetFloat(PawnAnimationParameters.Speed, speed);
					Pawn.Animator.SetFloat(PawnAnimationParameters.Speed, speed);
				})
			);
		}
	}
}