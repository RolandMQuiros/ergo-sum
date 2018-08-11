using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ErgoSum;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum.States {
	public class Damage : PawnStateBehaviour {
		public override void OnStateEnter(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
			AddStreams(
				Pawn.Pierced.Subscribe(pierce => { Pawn.Health.Value -= pierce.Damage; })
			);
		}
	}
}