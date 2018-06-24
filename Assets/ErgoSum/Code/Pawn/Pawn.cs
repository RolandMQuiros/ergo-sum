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
		public Rigidbody RigidBody { get { return _rigidBody; } }
		public PawnMotor Motor { get { return _motor; } }
		public float Gravity { get { return _gravity; } }
		public float PoisedMass { get { return _poisedMass; } }
		public float StunnedMass { get { return _stunnedMass; } }

		#region Editor Fields
		[SerializeField]private Rigidbody _rigidBody;
		[SerializeField]private float _gravity = -50f;
		[SerializeField]private float _groundCheck;
		[SerializeField]private float _groundCheckRadius;
		[SerializeField]private float _poisedMass = 50f;
		[SerializeField]private float _stunnedMass = 10f;
		#endregion
		private Animator _stateMachine;
		private PawnMotor _motor;
		private IEnumerable<PawnStateBehaviour> _states;

		#region MonoBehaviour
		private void Awake() {
			Controller = GetComponent<PawnController>();
			_stateMachine = GetComponent<Animator>();
			_states = _stateMachine.GetBehaviours<PawnStateBehaviour>();
			foreach (var state in _states) { state.Pawn = this; }
		}

		private void Start() {
			_rigidBody = _rigidBody ?? GetComponentInChildren<Rigidbody>();
			_motor = _rigidBody.GetComponent<PawnMotor>();
		}

		private void OnDrawGizmos() {
			if (_states != null) {
				foreach (var state in _states) { state.OnDrawGizmos(); }
			}
			Gizmos.color = IsGrounded() ? Color.red : Color.white;
			Gizmos.DrawWireSphere(RigidBody.position - _groundCheck * RigidBody.transform.up, _groundCheckRadius);
		}
		#endregion

		public bool IsGrounded() {
			return Physics.OverlapSphere(RigidBody.position - _groundCheck * RigidBody.transform.up, _groundCheckRadius, ~(1 << RigidBody.gameObject.layer)).Any();
		}
	}

}