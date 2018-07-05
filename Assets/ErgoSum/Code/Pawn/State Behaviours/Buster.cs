using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using ErgoSum.Utilities;

namespace ErgoSum {
    public class Buster : PawnStateBehaviour {
        [SerializeField]private GameObjectPool _lemonPool;
        [SerializeField]private GameObjectPool _halfCharge;
        [SerializeField]private GameObjectPool  _fullCharge;
        [SerializeField]private float _holsterTime = 1f;
        [SerializeField]private float _halfChargeTime = 1f;
        [SerializeField]private float _fullChargeTime = 2f;

        private Vector3 _aimDirection;
        private Vector3 _projectedAim;
        private float _currentAimX;

        public override void OnStateEnter(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
            AddLogicStreams(stateMachine);
            AddAnimationStreams();
        }

        private void AddLogicStreams(Animator stateMachine) {
            AddStreams(
                // Fire lemon
                Pawn.Controller.Aim.Where(unit => unit.FireStart)
                    .Subscribe(unit => {
                        GameObject lemon = _lemonPool.Get();
                        lemon.transform.position = unit.Source;
                        lemon.transform.rotation = Quaternion.Euler(unit.Eulers);
                        lemon.SetActive(true);
                        lemon.GetComponent<Rigidbody>().AddForce(unit.Direction * 50f, ForceMode.VelocityChange);
                    })
            );
        }

        protected override void OnPawnAttach(Pawn pawn) {
            _aimDirection = pawn.Animator.transform.forward;
            _projectedAim  = pawn.Animator.transform.forward;
            _currentAimX = 0f;
        }
        
        private void AddAnimationStreams() {
            AddStreams(
                Pawn.Controller.Aim.Subscribe(unit => {
                    Vector3 up = Pawn.Body.transform.up;
                    _aimDirection = unit.Direction;
                    _projectedAim = Vector3.ProjectOnPlane(_aimDirection, up);
                    
                    if (unit.FireStart) {
                        Pawn.Animator.SetBool(PawnAnimationParameters.FIRING, true);
                    }
                }),
                Pawn.Controller.Aim.Where(unit => unit.FireStart)
                    .Throttle(TimeSpan.FromSeconds(Time.timeScale * _holsterTime))
                    .Subscribe(_ => { Pawn.Animator.SetBool(PawnAnimationParameters.FIRING, false); }),
                Pawn.UpdateAsObservable()
                    .Subscribe(moveUnit => {
                        Vector3 up = Pawn.Body.transform.up;
                        float targetAimX = Vector3.SignedAngle(Pawn.Animator.transform.forward, _projectedAim, up);
                        float aimY = Vector3.Dot(_aimDirection, up) * 90f;
                        _currentAimX += (targetAimX - _currentAimX) / 10f;
                        Pawn.Animator.SetFloat("Aim X", _currentAimX);
					    Pawn.Animator.SetFloat("Aim Y", aimY);
                    })
            );
        }
    }
}