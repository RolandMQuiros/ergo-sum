using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ErgoSum;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum.States {
	public class Damage : PawnStateBehaviour {
		[SerializeField]private float _damageThreshold;
		public override void OnStateEnter(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
			AddStreams(
				Pawn.Body.OnCollisionEnterAsObservable()
					.Select(collision => collision.impulse.magnitude - _damageThreshold)
					.Where(damage => damage > 0f)
					.Subscribe(damage => { Pawn.Health.Value = Pawn.Health.Value - (int)damage; })
			);
		}
	}
}