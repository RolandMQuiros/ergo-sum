using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum {
    public class Crouch : PawnStateBehaviour {
        [SerializeField]private float _crouchDuration = 0.25f;
        public override void OnStateEnter(Animator stateMachine, UnityEngine.AnimatorStateInfo stateInfo, int layerIndex) {
            float crouchTime = 0f;
            float crouch = Pawn.Animator.GetFloat(PawnAnimationParameters.Crouching);
            AddStreams(
                Pawn.UpdateAsObservable()
                    .WithLatestFrom(Pawn.Controller.Crouch, (_, unit) => unit.Start)
                    .WithLatestFrom(
                        Pawn.Collider.OnTriggerStayAsObservable()
                            .Buffer(Pawn.Collider.FixedUpdateAsObservable())
                            .Select(triggers => !triggers.Any()),
                        (crouching, canStand) => new {
                            IsCrouching = crouching,
                            CanStand = canStand
                        }
                    )
                    .Subscribe(crouchState => {
                        if ((crouchState.IsCrouching || !crouchState.CanStand) && crouchTime < _crouchDuration) {
                            crouchTime += Time.deltaTime;
                            crouch = crouchTime / _crouchDuration;
                        } else if (!crouchState.IsCrouching && crouchState.CanStand && crouchTime > 0f) {
                            crouchTime -= Time.deltaTime;
                            crouch = crouchTime / _crouchDuration;
                        }

                        stateMachine.SetFloat(PawnStateParameters.Crouch, Mathf.Round(crouch));
                        Pawn.Animator.SetFloat(PawnAnimationParameters.Crouching, crouch);
                    })
            );
        }

        public override void OnStateExit(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(stateMachine, stateInfo, layerIndex);
            Pawn.Animator.SetFloat(PawnAnimationParameters.Crouching, 0f);
            stateMachine.SetBool(PawnStateParameters.Crouch, false);
        }
    }
}