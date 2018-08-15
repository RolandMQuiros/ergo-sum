using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum.States {
	public class Slide : PawnStateBehaviour {
		[Header("Compound collider search tags")]
		[SerializeField]private string _dashTag;
		[SerializeField]private string _standTag;
		[Header("Physics values")]
		[SerializeField]private float _slideSpeed;
		[SerializeField]private float _slideTime;
		[SerializeField]private float _rotationSpeed;
		[SerializeField]private float _adjustSpeed;

		private GameObject _dashCompoundCollider;
		private Collider _dashSustainTrigger;
		private GameObject _standCompoundCollider;

		protected override void OnPawnAttach(Pawn pawn) {
			_dashCompoundCollider = pawn.Body.transform
				.Cast<Transform>()
				.Where(t => t.tag.Equals(_dashTag))
				.First().gameObject;
			_dashSustainTrigger = _dashCompoundCollider.GetComponentsInChildren<Collider>()
				.Where(c => c.isTrigger)
				.First();
			_standCompoundCollider = pawn.Body.transform
				.Cast<Transform>()
				.Where(t => t.tag.Equals(_standTag))
				.First().gameObject;
		}

		public override void OnStateEnter(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
			if (Pawn.IsGrounded.Value) {
				Pawn.Animator.SetBool(PawnAnimationParameters.Dash, true);
				Pawn.Animator.SetBool(PawnAnimationParameters.Firing, false);
				Pawn.Animator.SetBool(PawnAnimationParameters.Aiming, false);

				_dashCompoundCollider.SetActive(true);
				_standCompoundCollider.SetActive(false);

				Vector3 mainDirection = new Vector3();
				Vector3 right = Vector3.right;
				float slideTimer = 0f;
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
					Pawn.Body.UpdateAsObservable()
						.Select(_ => Vector3.zero)
						.Merge(
							Pawn.Controller.Movement
								.Select(unit => Vector3.Dot(unit.Direction, mainDirection) > 0f ? Vector3.ProjectOnPlane(unit.Direction, mainDirection) : unit.Direction)
						)
						.Subscribe(adjust => {
							slideTimer += Time.deltaTime;
							Vector3 velocity = mainDirection * _slideSpeed + adjust * _adjustSpeed;
							Pawn.Motor.Move(velocity * Time.deltaTime);
							Pawn.Animator.transform.rotation = Quaternion.Lerp(
								Pawn.Animator.transform.rotation,
								mainRotation,
								_rotationSpeed * Time.deltaTime
							);
						}),
					Pawn.Controller.Movement.Where(unit => unit.DashEnd).Select(_ => true)
						.Merge(Pawn.IsGrounded.Where(i => !i).Select(_ => true))
						.Subscribe(_ => { stateMachine.SetBool(PawnStateParameters.Dash, false); })
				);
			} else {
				stateMachine.SetBool("Dash", false);
			}

			_dashSustainTrigger
				.OnTriggerEnterAsObservable()
				.Merge(_dashSustainTrigger.OnTriggerStayAsObservable())
				.Buffer(Observable.EveryFixedUpdate())
				.Subscribe(colliders => { _triggers = colliders; });
		}
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			base.OnStateExit(animator, stateInfo, layerIndex);
			Pawn.Animator.SetBool(PawnAnimationParameters.Dash, false);
			_dashCompoundCollider.SetActive(false);
			_standCompoundCollider.SetActive(true);
		}

		private IEnumerable<Collider> _triggers = Enumerable.Empty<Collider>();
		public override void OnDrawGizmos() {
			Gizmos.color = Color.cyan;
			foreach (var collider in _triggers) {
				Gizmos.DrawLine(_dashSustainTrigger.transform.position, collider.transform.position);
			}
		}
	}
}