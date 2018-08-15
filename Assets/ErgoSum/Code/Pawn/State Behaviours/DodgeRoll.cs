using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum {
	public class DodgeRoll : PawnStateBehaviour {
		[Header("Physics Properties")]
		[SerializeField]private float _dodgeSpeed;
		[SerializeField]private float _rotationSpeed = 10f;

		private bool _sustainDash;

		public override void OnStateEnter(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
			Pawn.Animator.SetBool(PawnAnimationParameters.Firing, false);
			Pawn.Animator.SetBool(PawnAnimationParameters.Aiming, false);
			Pawn.Animator.SetBool(PawnAnimationParameters.Dash, true);

			Vector3 mainDirection = new Vector3();
			Vector3 right = Vector3.right;
			Quaternion mainRotation = Quaternion.identity;

			_sustainDash = true;
			AddStreams(
				// Initial movement
				Pawn.Controller.Movement
					.Where(unit => unit.Direction != Vector3.zero)
					.Take(1)
					.Subscribe(unit => {
						mainDirection = unit.Direction.normalized;
						mainRotation = Quaternion.LookRotation(mainDirection, Pawn.Body.transform.up);
					}),
				Pawn.UpdateAsObservable()
					.Subscribe(jump => {
						Vector3 velocity = mainDirection * _dodgeSpeed;
						Pawn.Motor.Move(velocity * Time.deltaTime);
						
						Pawn.Animator.transform.rotation = Quaternion.Lerp(
							Pawn.Animator.transform.rotation,
							mainRotation,
							_rotationSpeed * Time.deltaTime
						);
					}),
				Pawn.Controller.Movement.Where(unit => unit.DashEnd)
					.Subscribe(_ => { _sustainDash = false; })
			);
		}

		public override void OnStateExit(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
			base.OnStateExit(stateMachine, stateInfo, layerIndex);
			stateMachine.SetBool(PawnStateParameters.Dash, _sustainDash);
			Pawn.Animator.SetBool(PawnAnimationParameters.Dash, _sustainDash);
		}
	}
}