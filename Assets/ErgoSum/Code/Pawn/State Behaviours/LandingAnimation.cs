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
			float squatTime = 0f;
			float squat = 0f;

			AddStreams(
				Pawn.UpdateAsObservable().Subscribe(_ => {
					Vector3 up = Pawn.Body.transform.up;
					float verticalSpeed = Vector3.Dot(Pawn.Body.velocity, up);
					float dvy = Mathf.Abs(previousVerticalSpeed - verticalSpeed);
					if (dvy > _landingThreshold) { // Jumping or landing
						squat = dvy / _landingSpeed;
						squatTime = 0f;
					}
					if (squat > 0f) {
						squatTime += Time.deltaTime;
						squat = _landingCurve.Evaluate(squatTime / _landingDuration);
					}
					previousVerticalSpeed = verticalSpeed;
					Pawn.Animator.SetFloat(PawnAnimationParameters.Squatting, squat);
					Pawn.Animator.SetFloat(PawnAnimationParameters.AirSpeed, verticalSpeed);
				})
			);
		}
	}
}