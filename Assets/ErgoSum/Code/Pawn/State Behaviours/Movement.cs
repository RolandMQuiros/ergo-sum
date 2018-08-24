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
			Vector3 velocity = Vector3.zero;
            AddStreams(
                Pawn.Controller.Movement.Subscribe(unit => {
					velocity = _speed * unit.Direction;
					Pawn.Motor.Move(velocity * Time.deltaTime);
				}),
				Pawn.UpdateAsObservable()
					.Subscribe(_ => {
						float speed = velocity.magnitude;
						stateMachine.SetFloat(PawnAnimationParameters.Speed, speed);
						Pawn.Animator.SetFloat(PawnAnimationParameters.Speed, speed / _speed);
					})
			);
		}
	}
}