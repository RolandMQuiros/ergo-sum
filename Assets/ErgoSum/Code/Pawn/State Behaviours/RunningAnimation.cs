using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

using ErgoSum;

namespace ErgoSum.States {
	public class RunningAnimation : PawnStateBehaviour {
		[SerializeField]private float _rotationSpeed;
		public override void OnStateEnter(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
			float speed = 0f;
			Quaternion moveRotation = Pawn.Animator.transform.rotation;

			AddStreams(
				Pawn.Controller.Movement.Subscribe(unit => {
					if (unit.Direction != Vector3.zero) {
						speed = unit.Direction.magnitude;
						moveRotation = Quaternion.LookRotation(unit.Direction.normalized);
					} else {
						speed = 0f;
					}
				}),
				Pawn.UpdateAsObservable()
					.WithLatestFrom(Pawn.IsGrounded, (_, g) => g)
					.Subscribe(isGrounded => {
						Pawn.Animator.transform.rotation = Quaternion.Lerp(
							Pawn.Animator.transform.rotation,
							moveRotation,
							_rotationSpeed * Time.deltaTime
						);

						Pawn.Animator.SetFloat(PawnAnimationParameters.SPEED, speed);
						Pawn.Animator.SetBool(PawnAnimationParameters.IS_GROUNDED, isGrounded);
					})
			);
		}
	}
}