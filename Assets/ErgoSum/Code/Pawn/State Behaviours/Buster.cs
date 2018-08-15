using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Diagnostics;
using UniRx.Triggers;
using ErgoSum.Utilities;


namespace ErgoSum.States {
    public class Buster : PawnStateBehaviour {
        public struct FireUnit {
            public PawnAimUnit Aim;
            public BusterCharge.ChargeUnit Charge;
        }

        public IObservable<FireUnit> Fire { get; private set; }

        [Header("Physics Parameters")]
        [SerializeField]private float _holsterTime = 1f;
        [SerializeField]private float _unholsterTime = 0.1f;

        private Vector3 _aimDirection;
        private Vector3 _projectedAim;
        private float _currentAimX;
        
        public override void OnStateEnter(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
            var charge = stateMachine.GetBehaviour<BusterCharge>();

            var fireInput = Pawn.Controller.Aim
                .Where(unit => unit.FireStart || unit.FireEnd)
                .WithLatestFrom(
                    charge.Charge,
                    (unit, level) => new FireUnit {
                        Aim = unit,
                        Charge = level
                    }
                )
                .Where(unit => (unit.Charge.Level > 0 && unit.Aim.FireEnd) ||
                    (unit.Charge.Level == 0 && unit.Aim.FireStart));

            var holsterTime = TimeSpan.FromSeconds(Time.timeScale * _holsterTime);
            var unholsterTime = TimeSpan.FromSeconds(Time.timeScale * _unholsterTime);

            // Everything in this #region is to create a firing stream that delays the first shot after unholstering weapon
            #region First Shot Delay
            // Emits after some time passes without shooting. Used to play the holstering animation.
            var holster = fireInput.Throttle(holsterTime);

            // Emits on the first fire event after holstering
            var draw = fireInput.Select(unit => true).Delay(unholsterTime)
                .Merge(holster.Select(unit => false))
                .DistinctUntilChanged(); // Emit only when there's a change (from holstered to unholstered)
            
            // Buffers the fire inputs received while unholstering weapon
            var drawFire = fireInput.Sample(draw.Where(drawn => drawn));

            // Omits the fire inputs received while unholstering weapon
            var remainingFire = fireInput
                .CombineLatest(draw, (unit, drawn) => new {
                    Unit = unit,
                    Drawn = drawn
                })
                .Where(fire => fire.Drawn)
                .Select(fire => fire.Unit);
            
            // Combine the delayed first shot with the remaining shots
            Fire = drawFire.Merge(remainingFire);
            #endregion

            AddStreams(
                // Aiming animation
                fireInput.Subscribe(_ => { Pawn.Animator.SetBool(PawnAnimationParameters.Aiming, true); }),
                // Holstering animation
                holster.Subscribe(_ => { Pawn.Animator.SetBool(PawnAnimationParameters.Aiming, false); }),
                // Fire animation
                Pawn.UpdateAsObservable().Select(_ => false)
                    .Merge(fireInput.Select(_ => true))
                    .Subscribe(firing => {
                        Pawn.Animator.SetBool(PawnAnimationParameters.Firing, firing);
                    }),
                // Spawn bullets
                Fire.Subscribe(unit => {
                    var bullet = unit.Charge.Pool.Get().GetComponent<Bullet>();
                    bullet.Fire(Pawn, unit.Aim.Source.position, Quaternion.Euler(unit.Aim.Eulers));
                })
            );

            AimingAnimationStreams();
        }

        protected override void OnPawnAttach(Pawn pawn) {
            _aimDirection = pawn.Animator.transform.forward;
            _projectedAim  = pawn.Animator.transform.forward;
            _currentAimX = 0f;
        }
        
        private void AimingAnimationStreams() {
            AddStreams(
                Pawn.Controller.Aim
                    .Subscribe(unit => {
                        Vector3 up = Pawn.Body.transform.up;
                        _aimDirection = unit.Direction;
                        _projectedAim = Vector3.ProjectOnPlane(_aimDirection, up);
                    }),
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