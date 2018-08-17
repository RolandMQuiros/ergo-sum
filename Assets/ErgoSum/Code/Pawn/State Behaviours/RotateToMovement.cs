using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

using ErgoSum;

namespace ErgoSum.States {
	public class RotateToMovement : PawnStateBehaviour {
		[SerializeField]private float _rotationSpeed = 360f;
		public override void OnStateEnter(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
			float speed = 0f;
			Quaternion targetRotation = Pawn.Animator.transform.rotation;

			AddStreams(
				Pawn.Controller.Movement.Subscribe(unit => {
					if (unit.Direction != Vector3.zero) {
						speed = unit.Direction.magnitude;
						targetRotation = Quaternion.LookRotation(unit.Direction.normalized);
					} else {
						speed = 0f;
					}
				}),
				Pawn.UpdateAsObservable()
					.WithLatestFrom(Pawn.IsGrounded, (_, g) => g)
					.Subscribe(isGrounded => {
						Pawn.Animator.transform.rotation = Quaternion.RotateTowards(
							Pawn.Animator.transform.rotation,
							targetRotation,
							_rotationSpeed * Time.deltaTime
						);
					})
			);
		}
	}
}