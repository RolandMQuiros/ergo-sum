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
		[SerializeField]private float _slideTime;
		[SerializeField]private float _slideDistance;
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
			if (Pawn.IsGrounded()) {
				_dashCompoundCollider.SetActive(true);
				_standCompoundCollider.SetActive(false);

				Vector3 mainDirection = new Vector3();
				Vector3 right = Vector3.right;
				float slideSpeed = _slideDistance / _slideTime;

				AddStreams(
					// Initial movement
					Pawn.Controller.Movement
						.Take(1)
						.Subscribe(unit => {
							mainDirection = unit.Direction.normalized;
						}),
					Pawn.Body.FixedUpdateAsObservable()
						.Select(_ => Vector3.zero)
						.Merge(
							Pawn.Controller.Movement
								.Select(unit => Vector3.Dot(unit.Direction, mainDirection) > 0f ? Vector3.ProjectOnPlane(unit.Direction, mainDirection) : unit.Direction)
							)
						.Subscribe(adjust => {
							if (!Pawn.IsGrounded()) {
								stateMachine.SetBool("Dash", false);
							}
							Pawn.Motor.Move((mainDirection * slideSpeed + adjust * _adjustSpeed) * Time.deltaTime);
						}),
					Observable
						.Merge(_dashSustainTrigger.OnTriggerStayAsObservable())
						.Buffer(Pawn.Body.FixedUpdateAsObservable())
						.SkipUntil(Observable.Timer(TimeSpan.FromSeconds(Time.timeScale * _slideTime)))
						.Subscribe(colliders => {
							if (!colliders.Any()) {
								stateMachine.SetBool("Dash", false);
							}
						})
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