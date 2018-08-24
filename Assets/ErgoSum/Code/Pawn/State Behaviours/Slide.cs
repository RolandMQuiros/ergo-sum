using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum.States {
	public class Slide : PawnStateBehaviour {
		[Header("Physics values")]
		[SerializeField]private float _speed;
		[SerializeField]private float _rotationSpeed;

		public override void OnStateEnter(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
			Pawn.Animator.SetBool(PawnAnimationParameters.Dash, true);
			Pawn.Animator.SetBool(PawnAnimationParameters.Firing, false);
			Pawn.Animator.SetBool(PawnAnimationParameters.Aiming, false);

			Vector3 direction = new Vector3();
			Quaternion rotation = Quaternion.identity;

			AddStreams(
				// Initial movement
				Pawn.Controller.Movement
					.Where(unit => unit.Direction != Vector3.zero)
					.Take(1)
					.Subscribe(unit => {
						direction = unit.Direction.normalized;
						rotation = Quaternion.LookRotation(direction, Pawn.Body.transform.up);
					}),
				Pawn.Body.UpdateAsObservable()
					.WithLatestFrom(Pawn.Controller.Movement, (_, unit) => unit)
					.Subscribe(unit => {
						if (unit.Direction == Vector3.zero) {
							stateMachine.SetBool(PawnStateParameters.Dash, false);
							stateMachine.SetFloat(PawnStateParameters.Speed, 0f);
							stateMachine.SetFloat(PawnAnimationParameters.Speed, 0f);
						} else {
							var targetRotation = Quaternion.LookRotation(unit.Direction.normalized, Pawn.Body.transform.up);
							direction = Vector3.RotateTowards(direction, unit.Direction.normalized, Mathf.Deg2Rad * _rotationSpeed * Time.deltaTime, 1f);
							rotation = Quaternion.RotateTowards(rotation, targetRotation, _rotationSpeed * Time.deltaTime);
							
							Pawn.Animator.transform.rotation = rotation;
							Pawn.Motor.Move(direction * _speed * Time.deltaTime);
						}
					}),
				Pawn.IsGrounded.Where(i => !i).Subscribe(_ => { stateMachine.SetBool(PawnStateParameters.Dash, false); }),
				Pawn.Motor.Movement.Where(m => m == Vector3.zero).Subscribe(_ => { stateMachine.SetBool(PawnStateParameters.Dash, false); })
			);

		}
		public override void OnStateExit(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
			base.OnStateExit(stateMachine, stateInfo, layerIndex);
			
			stateMachine.SetBool(PawnStateParameters.Dash, false);
			Pawn.Animator.SetBool(PawnAnimationParameters.Dash, false);
		}
	}
}