using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using ErgoSum.Utilities;

namespace ErgoSum.States {

	public class BusterCharge : PawnStateBehaviour {
		[Serializable]
		public struct ChargeUnit {
			[HideInInspector]public int Level;
			public GameObjectPool Pool;
			public float ChargeDelay;
		}
		public ReactiveProperty<ChargeUnit> Charge { get { return _charge; } }
		[SerializeField]private ChargeUnit[] _levels;
		private ReactiveProperty<ChargeUnit> _charge = new ReactiveProperty<ChargeUnit>();

		protected override void OnPawnAttach(Pawn pawn) {
			for (int i = 0; i < _levels.Length; i++) {
				_levels[i].Level = i;
			}
			_charge.Value = _levels[0];
		}
		
		public override void OnStateEnter(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
			var buster = stateMachine.GetBehaviour<Buster>();
			var fireInput = Pawn.Controller.Aim.Where(unit => unit.FireStart || unit.FireEnd);
			float chargeTime = 0f;
			int level = 0;
			AddStreams(
				// Hold
				Pawn.UpdateAsObservable()
                    .WithLatestFrom(fireInput, (_, unit) => unit)
					.Where(unit => unit.FireStart)
                    .Subscribe(unit => {
						int newLevel = 0;
						chargeTime += Time.deltaTime;
						for (
							newLevel = _levels.Length - 1;
							newLevel > 0 && chargeTime < _levels[newLevel].ChargeDelay;
							newLevel--
						);
						if (level != newLevel) {
							level = newLevel;
							_charge.Value = _levels[newLevel];
						}
                    }),
				// Charge animation
				_charge.Subscribe(
					charge => {
						Pawn.Animator.SetInteger(PawnAnimationParameters.CHARGE, charge.Level);
					}
				),
				// Reset on fire
				buster.Fire.Subscribe(_ => {
					level = 0;
					chargeTime = 0f;
					_charge.Value = _levels[0];
				})
			);
		}

		public override void OnStateExit(Animator stateMachine, AnimatorStateInfo stateInfo, int layerIndex) {
			int x = 0;
		}
	}
}