using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum.States {
	public class Slide : PawnStateBehaviour {
		[SerializeField]private float _slideTime;
		[SerializeField]private float _slideDistance;
		[SerializeField]private float _adjustSpeed;
		[SerializeField]private SphereCollider _headCollider;
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			if (Pawn.IsGrounded()) {
				_headCollider.enabled = false;
				Vector3 mainDirection = new Vector3();
				float slideSpeed = _slideDistance / _slideTime;
				Vector3 right = Vector3.right;

				AddStreams(
					// Initial movement
					Pawn.Controller.Movement.Take(1).Subscribe(unit => {
						mainDirection = unit.Direction.normalized;
						Vector3.Cross(mainDirection, Pawn.RigidBody.transform.up);
					}),
					Pawn.RigidBody.FixedUpdateAsObservable()
						.Select(_ => Vector3.zero)
						.Merge(
							Pawn.Controller.Movement
								.Select(unit => Vector3.Dot(unit.Direction, mainDirection) > 0f ? Vector3.ProjectOnPlane(unit.Direction, mainDirection) : unit.Direction)
							)
						.Subscribe(adjust => {
							Pawn.Motor.Move((mainDirection * slideSpeed + adjust * _adjustSpeed) * Time.deltaTime);
						}),
					Observable.Timer(TimeSpan.FromSeconds(Time.timeScale * _slideTime)).Subscribe(_ => {
						animator.SetBool("Dash", false);
					})
				);
			} else {
				animator.SetBool("Dash", false);
			}
		}
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			base.OnStateExit(animator, stateInfo, layerIndex);
			_headCollider.enabled = true;
		}
	}
}