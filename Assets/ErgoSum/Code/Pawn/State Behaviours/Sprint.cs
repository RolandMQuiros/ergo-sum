using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum {
	public class Sprint : PawnStateBehaviour {
		[SerializeField]private float _speed;
		[SerializeField]private float _acceleration;
		[SerializeField]private float _rotationSpeed;
		public override void OnStateEnter(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
			Vector3 direction = Vector3.zero;
			Quaternion rotation = Quaternion.identity;
			float speed = stateMachine.GetFloat(PawnStateParameters.Speed);
			bool endDash = false;

			Pawn.Animator.SetBool(PawnAnimationParameters.Aiming, false);
			Pawn.Animator.SetBool(PawnAnimationParameters.Firing, false);
			Pawn.Animator.SetBool(PawnAnimationParameters.Sprint, true);

			AddStreams(
				Pawn.Controller.Movement
					.Where(unit => unit.Direction != Vector3.zero)
					.Take(1)
					.Subscribe(unit => {
						direction = unit.Direction.normalized;
						rotation = Quaternion.LookRotation(unit.Direction, Pawn.Body.transform.up);
					}),
				Pawn.Controller.Movement
					.Where(unit => !endDash)
					.Subscribe(unit => {
						endDash = Vector3.Dot(direction, unit.Direction.normalized) <= 0f || unit.DashEnd;
					}),
				Pawn.UpdateAsObservable()
					.WithLatestFrom(Pawn.Controller.Movement, (_, unit) => unit)
					.Subscribe(unit => {
						if (endDash && Pawn.IsGrounded.Value) {
							stateMachine.SetBool(PawnStateParameters.Dash, false);
						} else {
							var targetRotation = Quaternion.LookRotation(unit.Direction, Pawn.Body.transform.up);

							direction = Vector3.RotateTowards(direction, unit.Direction, Mathf.Deg2Rad * _rotationSpeed * Time.deltaTime, 1f);
							rotation = Quaternion.RotateTowards(rotation, targetRotation, _rotationSpeed * Time.deltaTime);
							speed = Mathf.SmoothStep(speed, _speed, _acceleration * Time.deltaTime);

							Pawn.Animator.transform.rotation = rotation;
							Pawn.Motor.Move(speed * direction * Time.deltaTime);

							stateMachine.SetFloat(PawnStateParameters.Speed, speed);
							Pawn.Animator.SetFloat(PawnAnimationParameters.Speed, speed);
						}
					})
			);
		}

		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			base.OnStateExit(animator, stateInfo, layerIndex);
			Pawn.Animator.SetBool(PawnAnimationParameters.Sprint, false);
		}
	}
}