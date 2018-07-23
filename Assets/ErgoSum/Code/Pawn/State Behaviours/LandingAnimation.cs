using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum.States {
	public class LandingAnimation : PawnStateBehaviour {
		[SerializeField]private float _landingThreshold;
		[SerializeField]private float _landingSpeed;
		[SerializeField]private float _landingDuration;
		[SerializeField]private AnimationCurve _landingCurve;

		public override void OnStateEnter(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
			float previousVerticalSpeed = 0f;
			float crouchTime = 0f;
			float crouch = 0f;

			AddStreams(
				Pawn.UpdateAsObservable().Subscribe(_ => {
					Vector3 up = Pawn.Body.transform.up;
					float verticalSpeed = Vector3.Dot(Pawn.Body.velocity, up);
					float dvy = Mathf.Abs(previousVerticalSpeed - verticalSpeed);
					if (dvy > _landingThreshold) { // Jumping or landing
						crouch = dvy / _landingSpeed;
						crouchTime = 0f;
					}
					if (crouch > 0f) {
						crouchTime += Time.deltaTime;
						crouch = _landingCurve.Evaluate(crouchTime / _landingDuration);
					}
					previousVerticalSpeed = verticalSpeed;

					Pawn.Animator.SetFloat(PawnAnimationParameters.CROUCHING, crouch);
					Pawn.Animator.SetFloat(PawnAnimationParameters.AIR_SPEED, verticalSpeed);
				})
			);
		}
	}
}