using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum.States {
	public class Jump : PawnStateBehaviour {
		[SerializeField]private float _maxJumpHeight;
		[SerializeField]private float _minJumpHeight;
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			float maxJumpSpeed = Mathf.Sqrt(_maxJumpHeight * 2f * Pawn.Gravity);
			float minJumpSpeed = Mathf.Sqrt(_minJumpHeight * 2f * Pawn.Gravity);

			AddStreams(
				Pawn.Controller.Jump
					.Subscribe(unit => {
						if (unit.Down && Pawn.IsGrounded()) {
							Pawn.RigidBody.AddForce(maxJumpSpeed * Pawn.RigidBody.transform.up, ForceMode.VelocityChange);
						} else if (unit.Release) {
							float jumpSpeed = Vector3.Dot(Pawn.RigidBody.velocity, Pawn.RigidBody.transform.up) - minJumpSpeed;
							if (jumpSpeed > 0f) {
								Pawn.RigidBody.AddForce(jumpSpeed * -Pawn.RigidBody.transform.up, ForceMode.VelocityChange);
							}
						}
					})
			);
		}
	}
}