using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace ErgoSum {
	[Serializable]
	[RequireComponent(typeof(PawnController))]
	[RequireComponent(typeof(Animator))]
	public class Pawn : MonoBehaviour {
		public IntReactiveProperty Health;
		public PawnController Controller { get; private set; }
		public Rigidbody Body { get { return _body; } }
		public Animator Animator { get { return _animator; } }
		public PawnMotor Motor { get { return _motor; } }

		#region Editor Fields
		[SerializeField]private Rigidbody _body;
		[SerializeField]private Animator _animator;
		[SerializeField]private float _groundCheck;
		[SerializeField]private float _groundCheckRadius;
		#endregion
		private Animator _stateMachine;
		private PawnMotor _motor;
		private IEnumerable<PawnStateBehaviour> _states;

		#region MonoBehaviour
		private void Awake() {
			Controller = GetComponent<PawnController>();
			_stateMachine = GetComponent<Animator>();
			_states = _stateMachine.GetBehaviours<PawnStateBehaviour>();
		}

		private void Start() {
			_body = _body ?? GetComponentInChildren<Rigidbody>();
			_motor = _body.GetComponent<PawnMotor>();
			foreach (var state in _states) { state.Pawn = this; }
		}

		private void OnDrawGizmos() {
			if (_states != null) {
				foreach (var state in _states) { state.OnDrawGizmos(); }
			}
			Gizmos.color = IsGrounded() ? Color.red : Color.white;
			Gizmos.DrawWireSphere(Body.position - _groundCheck * Body.transform.up, _groundCheckRadius);
		}
		#endregion

		public bool IsGrounded() {
			return Physics.OverlapSphere(Body.position - _groundCheck * Body.transform.up, _groundCheckRadius, ~(1 << Body.gameObject.layer)).Any();
		}
	}

}