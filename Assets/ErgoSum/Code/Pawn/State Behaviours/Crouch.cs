using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum {
    public class Crouch : PawnStateBehaviour {
        [SerializeField]private float _crouchTime = 0.25f;
        public override void OnStateEnter(Animator stateMachine, UnityEngine.AnimatorStateInfo stateInfo, int layerIndex) {
            float crouch = 0f;
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
                        if (crouchState.IsCrouching && crouch < _crouchTime) {
                            crouch += Time.deltaTime;
                            stateMachine.SetFloat(PawnStateParameters.Crouch, 1f);
                            Pawn.Animator.SetFloat(PawnAnimationParameters.Crouching, crouch / _crouchTime);
                        } else if (!crouchState.IsCrouching && crouchState.CanStand && crouch > 0f) {
                            crouch -= Time.deltaTime;
                            stateMachine.SetFloat(PawnStateParameters.Crouch, 0f);
                            Pawn.Animator.SetFloat(PawnAnimationParameters.Crouching, crouch / _crouchTime);
                        }
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