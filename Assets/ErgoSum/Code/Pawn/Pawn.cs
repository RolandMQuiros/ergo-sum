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
		public IObservable<PierceUnit> Pierced { get { return _pierced; } }
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
		private Subject<PierceUnit> _pierced = new Subject<PierceUnit>();

		#region MonoBehaviour
		private void Awake() {
			Controller = GetComponent<PawnController>();
			_stateMachine = GetComponent<Animator>();
			_states = _stateMachine.GetBehaviours<PawnStateBehaviour>();

			Health = new IntReactiveProperty(_initialHealth);
			IsGrounded = new BoolReactiveProperty();

			int collisions = 0;
			_groundCheck.OnTriggerStayAsObservable().Subscribe(collided => { collisions++; });
			_groundCheck.FixedUpdateAsObservable().Subscribe(_ => {
				IsGrounded.Value = collisions > 0;
				collisions = 0;
			});
		}
		private void Start() {
			_body = _body ?? GetComponentInChildren<Rigidbody>();
			_motor = _motor ?? _body.GetComponent<PawnMotor>();
			foreach (var state in _states) { state.Pawn = this; }
		}
		#endregion

		public void Pierce(PierceUnit unit) {
			_pierced.OnNext(unit);
		}
	}

}