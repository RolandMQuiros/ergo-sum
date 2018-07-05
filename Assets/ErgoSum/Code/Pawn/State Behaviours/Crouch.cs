using UnityEngine;
using UniRx;
namespace ErgoSum {
    public class Crouch : PawnStateBehaviour {
        [SerializeField]private float _crouchTime = 0.25f;
        public override void OnStateEnter(Animator animator, UnityEngine.AnimatorStateInfo stateInfo, int layerIndex) {
            AddStreams(
                Pawn.Controller.Crouch
                    .Where(unit => unit.Start)
                    .Subscribe(_ => { Pawn.Animator.SetFloat("Crouching", 1f); }),
                Pawn.Controller.Crouch
                    .Where(unit => unit.End)
                    .Subscribe(_ => { Pawn.Animator.SetFloat("Crouching", 0f); })
            );
        }
    }
}