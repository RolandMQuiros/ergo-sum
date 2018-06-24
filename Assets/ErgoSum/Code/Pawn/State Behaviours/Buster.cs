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
        [SerializeField]private float _halfChargeTime = 1f;
        [SerializeField]private float _fullChargeTime = 2f;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            AddStreams(
                // Fire lemon
                Pawn.Controller.Aim.Where(unit => unit.FireStart)
                    .Subscribe(unit => {
                        Debug.Log(unit.Direction);
                        GameObject lemon = _lemonPool.Get();
                        lemon.transform.position = unit.Source;
                        lemon.transform.rotation = Quaternion.Euler(unit.Eulers);
                        lemon.SetActive(true);
                        lemon.GetComponent<Rigidbody>().AddForce(unit.Direction * 50f, ForceMode.VelocityChange);
                    })
            );
        }
    }
}