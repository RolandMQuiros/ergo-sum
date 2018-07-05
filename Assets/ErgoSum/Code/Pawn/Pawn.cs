using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum {
	[Serializable]
	[RequireComponent(typeof(PawnController))]
	[RequireComponent(typeof(Animator))]
	public class Pawn : MonoBehaviour {
		public IntReactiveProperty Health { get; private set; }
		public BoolReactiveProperty IsGrounded { get; private set; }
		public PawnController Controller { get; private set; }
		public Rigidbody Body { get { return _body; } }
		public Animator Animator { get { return _animator; } }
		public PawnMotor Motor { get { return _motor; } }

		#region Editor Fields
		[SerializeField]private int _initialHealth;
		[SerializeField]private Rigidbody _body;
		[SerializeField]private PawnMotor _motor;
		[SerializeField]private Animator _animator;
		[SerializeField]private Collider _groundCheck;
		#endregion
		private Animator _stateMachine;
		private IEnumerable<PawnStateBehaviour> _states;

		#region MonoBehaviour
		private void Awake() {
			Controller = GetComponent<PawnController>();
			_stateMachine = GetComponent<Animator>();
			_states = _stateMachine.GetBehaviours<PawnStateBehaviour>();

			Health = new IntReactiveProperty(_initialHealth);
			IsGrounded = new BoolReactiveProperty();

			_groundCheck.OnTriggerStayAsObservable().Select(_ => true)
				.Merge(_groundCheck.OnTriggerExitAsObservable().Select(_ => false))
				.Subscribe(collided => { IsGrounded.Value = collided; });
		}

		private void Start() {
			_body = _body ?? GetComponentInChildren<Rigidbody>();
			_motor = _motor ?? _body.GetComponent<PawnMotor>();
			foreach (var state in _states) { state.Pawn = this; }
		}
		#endregion
	}

}