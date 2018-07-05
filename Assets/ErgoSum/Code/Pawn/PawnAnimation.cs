using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum {
	public class PawnAnimation : MonoBehaviour {
		public Pawn Pawn { get { return _pawn; } }
		[SerializeField]private Pawn _pawn;
		[SerializeField]private float _holsterTime;
		[SerializeField]private float _rotationSpeed;
		[Header("Landing Animation Parameters")]
		[SerializeField]private float _landingThreshold;
		[SerializeField]private float _landingSpeed;
		[SerializeField]private float _landingDuration;
		[SerializeField]private AnimationCurve _landingCurve;
		[Header("Skeleton IK")]
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
			
			_pawn.Controller.Movement.Subscribe(unit => {
				if (unit.Direction != Vector3.zero) {
					facingDirection = unit.Direction.normalized;
					speed = unit.Direction.magnitude;
					moveRotation = Quaternion.LookRotation(facingDirection);
				} else {
					speed = 0f;
				}
			});

			// Set aim directions and rotations
			_pawn.Controller.Aim.Subscribe(unit => {
				aimDirection = unit.Direction;
				aimEulers = unit.Eulers;
				projectedAim = Vector3.ProjectOnPlane(aimDirection, up);
			});
			
			_pawn.Controller.Aim.Where(unit => unit.FireStart).Subscribe(_ => { animator.SetBool("Firing", true); });
			_pawn.Controller.Aim.Where(unit => unit.FireStart ^ unit.FireEnd)
				.Throttle(TimeSpan.FromSeconds(Time.timeScale * _holsterTime))
				.Subscribe(_ => { animator.SetBool("Firing", false); });

			float currentAimX = 0f;
			float targetAimX = 0f;
			float previousVerticalSpeed = 0f;
			float crouch = 0f;
			float crouchTime = 0f;
			this.UpdateAsObservable()
				.WithLatestFrom(_pawn.IsGrounded, (_, isGrounded) => isGrounded)
				.Subscribe(isGrounded => {
					up = _pawn.Body.transform.up;
					Vector3 right = Vector3.Cross(aimDirection, projectedAim);
					
					targetAimX = Vector3.SignedAngle(facingDirection, projectedAim, up);
					float aimY = Vector3.Dot(aimDirection, up) * 90f;
					currentAimX += (targetAimX - currentAimX) / 10f;

					transform.rotation = Quaternion.Lerp(
						transform.rotation,
						moveRotation,
						_rotationSpeed * Time.deltaTime
					);
					
					float verticalSpeed = Vector3.Dot(_pawn.Body.velocity, up);
					_vspeed = _pawn.Body.velocity;
					float dvy = Mathf.Abs(previousVerticalSpeed - verticalSpeed);
					if (dvy > _landingThreshold) { // Jumping or landing
						crouch = dvy / _landingSpeed;
						crouchTime = 0f;
						Debug.Log("Jumped or Landed " + verticalSpeed);
					}
					if (crouch > 0f) {
						//crouch = 1f / (1f + Mathf.Exp(-_landingCoefficient * (-_landingScale * crouchTime + 0.5f)));
						crouchTime += Time.deltaTime;
						crouch = _landingCurve.Evaluate(crouchTime / _landingDuration);
					}

					animator.SetFloat("Aim X", currentAimX);
					animator.SetFloat("Aim Y", aimY);
					animator.SetFloat("Speed", speed);
					animator.SetBool("Is Grounded", isGrounded);
					animator.SetFloat("Air Speed", verticalSpeed);
					animator.SetFloat("Crouching", crouch);

					previousVerticalSpeed = verticalSpeed;
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
		[SerializeField]Vector3 _vspeed;

		private Vector3 _leftHit, _rightHit;

		private void OnDrawGizmos() {
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(_leftHit, 0.1f);
			Gizmos.DrawSphere(_rightHit, 0.1f);
		}
	}
}