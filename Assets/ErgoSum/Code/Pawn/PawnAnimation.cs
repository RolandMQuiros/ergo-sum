using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum {
	public class PawnAnimation : MonoBehaviour {
		public Pawn Pawn { get { return _pawn; } }
		[SerializeField]private Pawn _pawn;
		[SerializeField]private float _aimLag;
		[SerializeField]private float _rotationSpeed;
		[SerializeField]private Transform _leftFoot;
		[SerializeField]private Transform _rightFoot;
		[SerializeField]private LayerMask _ikMask;
		private void Start() {
			Animator animator = GetComponent<Animator>();

			Vector3 up = _pawn.Body.transform.up;			
			
			float speed = 0f;
			Vector3 facingDirection = Vector3.zero;

			Vector3 aimDirection = transform.forward;
			Vector3 projectedAim = transform.forward;
			Vector3 aimEulers = transform.rotation.eulerAngles;
			Quaternion moveRotation = Quaternion.identity;
			
			bool controllerFiring = false;
			bool actuallyFiring = false;
			
			_pawn.Controller.Movement.Subscribe(unit => {
				if (unit.Direction != Vector3.zero) {
					facingDirection = unit.Direction.normalized;
					speed = unit.Direction.magnitude;
					moveRotation = Quaternion.LookRotation(facingDirection);
				} else {
					speed = 0f;
				}
			});
			_pawn.Controller.Aim.Subscribe(unit => {
				aimDirection = unit.Direction;
				controllerFiring = unit.FireEnd ? false : controllerFiring || unit.FireStart;
				aimEulers = unit.Eulers;
				projectedAim = Vector3.ProjectOnPlane(aimDirection, up);
			});
			_pawn.Controller.Aim.Where(unit => unit.FireEnd)
				.Throttle(TimeSpan.FromSeconds(Time.timeScale * _aimLag))
				.Subscribe(_ => {
					actuallyFiring = false;
					animator.SetBool("Firing", actuallyFiring);
				});

			float currentAimX = 0f;
			float targetAimX = 0f;

			this.UpdateAsObservable().Subscribe(_ => {
				up = _pawn.Body.transform.up;
				Vector3 right = Vector3.Cross(aimDirection, projectedAim);
				
				targetAimX = Vector3.SignedAngle(facingDirection, projectedAim, up);
				float aimY = Vector3.Dot(aimDirection, up) * 90f;
				currentAimX += (targetAimX - currentAimX) / 10f;
				
				if (controllerFiring) {
					actuallyFiring = true;
					animator.SetBool("Firing", actuallyFiring);
				}
				bool isGrounded = Pawn.IsGrounded();

				transform.rotation = Quaternion.Lerp(
					transform.rotation,
					moveRotation,
					_rotationSpeed * Time.deltaTime
				);

				animator.SetFloat("Aim X", currentAimX);
				animator.SetFloat("Aim Y", aimY);
				animator.SetFloat("Speed", speed);
				animator.SetBool("Is Grounded", isGrounded);
				animator.SetFloat("Air Speed", Vector3.Dot(_pawn.Body.velocity, up));
			});

			this.OnAnimatorIKAsObservable().Subscribe(_ => {
				RaycastHit leftHit, rightHit;
				if (Physics.Linecast(_leftFoot.position, _leftFoot.position - up, out leftHit, _ikMask.value)) {
					animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
					animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftHit.point);
					_leftHit = leftHit.point;
				}
				if (Physics.Linecast(_rightFoot.position, _rightFoot.position - up, out rightHit, _ikMask.value)) {
					animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
					animator.SetIKPosition(AvatarIKGoal.RightFoot, rightHit.point);
					_rightHit = rightHit.point;
				}
			});
		}

		[SerializeField]float _lookAtWeight;

		private Vector3 _leftHit, _rightHit;

		private void OnDrawGizmos() {
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(_leftHit, 0.1f);
			Gizmos.DrawSphere(_rightHit, 0.1f);
		}
	}
}