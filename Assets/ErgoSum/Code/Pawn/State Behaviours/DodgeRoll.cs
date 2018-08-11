using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum {
	public class DodgeRoll : PawnStateBehaviour {
		[Header("Physics Properties")]
		[SerializeField]private AnimationCurve _speedCurve;
		[SerializeField]private AnimationCurve _boostCurve;
		[SerializeField]private float _dodgeDistance;
		[SerializeField]private float _dodgeTime;
		[SerializeField]private float _rotationSpeed = 10f;

		public override void OnStateEnter(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
			Pawn.Animator.SetBool(PawnAnimationParameters.FIRING, false);
			Pawn.Animator.SetBool(PawnAnimationParameters.AIMING, false);
			Pawn.Animator.SetBool(PawnAnimationParameters.DASHING, true);

			Vector3 mainDirection = new Vector3();
			Vector3 right = Vector3.right;
			float slideMagnitude = _dodgeDistance / _dodgeTime;
			float slideTimer = 0f;
			float curveTime = 0f;
			Quaternion mainRotation = Quaternion.identity;

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
						slideTimer += Time.deltaTime;
						curveTime = slideTimer / _dodgeTime;
						
						float curve = _speedCurve.Evaluate(curveTime);
						Vector3 velocity = curve * mainDirection * slideMagnitude;
						Pawn.Motor.Move(velocity * Time.deltaTime);
						
						Pawn.Animator.transform.rotation = Quaternion.Lerp(
							Pawn.Animator.transform.rotation,
							mainRotation,
							_rotationSpeed * Time.deltaTime
						);

						if (slideTimer > _dodgeTime) {
							stateMachine.SetBool(PawnStateParameters.Dodge, false);
						}
					}),
				Pawn.Controller.Jump
					.Subscribe(jump => {
						if (jump.Down && _boostCurve.Evaluate(curveTime) > 0.5f) {
							stateMachine.SetBool(PawnStateParameters.Dash, true);
							stateMachine.SetBool(PawnStateParameters.Dodge, false);
						}
					})
			);
		}

		public override void OnStateExit(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
			base.OnStateExit(stateMachine, stateInfo, layerIndex);
			Pawn.Animator.SetBool(PawnAnimationParameters.DASHING, false);
		}
	}
}