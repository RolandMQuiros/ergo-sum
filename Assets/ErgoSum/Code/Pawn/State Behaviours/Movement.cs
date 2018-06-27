using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace ErgoSum.States {
	public class Movement : PawnStateBehaviour {
		[SerializeField]private string _terrainLayerName = "Terrain";
		[SerializeField]private float _speed;
		public override void OnStateEnter(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
			AddStreams(
				Pawn.Controller.Movement.Subscribe(unit => {
					if (unit.DashStart) {
						stateMachine.SetBool("Dash", true);
					} else {
						Pawn.Motor.Move(_speed * unit.Direction * Time.deltaTime);
					}
				})
			);
		}
	}
}