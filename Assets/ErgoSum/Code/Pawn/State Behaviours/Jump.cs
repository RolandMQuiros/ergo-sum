using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum.States {
	public class Jump : PawnStateBehaviour {
		[SerializeField]private float _gravity;
		[SerializeField]private float _maxJumpHeight;
		[SerializeField]private float _minJumpHeight;
		public override void OnStateEnter(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
			float maxJumpSpeed = Mathf.Sqrt(_maxJumpHeight * 2f * _gravity);
			float minJumpSpeed = Mathf.Sqrt(_minJumpHeight * 2f * _gravity);

			AddStreams(
				Pawn.Controller.Jump
					.Subscribe(unit => {
						if (unit.Down && Pawn.IsGrounded()) {
							Pawn.Body.AddForce(maxJumpSpeed * Pawn.Body.transform.up, ForceMode.VelocityChange);
						} else if (unit.Release) {
							float jumpSpeed = Vector3.Dot(Pawn.Body.velocity, Pawn.Body.transform.up) - minJumpSpeed;
							if (jumpSpeed > 0f) {
								Pawn.Body.AddForce(jumpSpeed * -Pawn.Body.transform.up, ForceMode.VelocityChange);
							}
						}
					})
			);
		}
	}
}