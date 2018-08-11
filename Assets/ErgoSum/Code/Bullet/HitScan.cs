using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum {
	public class HitScan : Bullet {
        public override IObservable<PierceUnit> Pierce {
            get {
                return this.OnEnableAsObservable()
                    .Select(_ => {
                        RaycastHit hit;
                        Physics.Linecast(
                            transform.position,
                            transform.position + _scanDistance * transform.forward,
                            out hit,
                            _layerMask
                        );
                        return hit;
                    })
                    .Where(hit => hit.collider != null)
                    .Select(hit => new PierceUnit {
                        Source = _source,
                        Target = hit.collider.gameObject,
                        Point = hit.point,
                        Normal = hit.normal,
                        Damage = _damage
                    });
            }
        }
        [Tooltip("How far to scan for collisions")]
        [SerializeField]private float _scanDistance = 50f;
        [Tooltip("Which collision layers to cast against")]
        [SerializeField]private LayerMask _layerMask;
        [SerializeField]private Transform _facade;

        protected override void Awake() {
            base.Awake();
            _facade.parent = transform.parent;
            this.OnEnableAsObservable()
                .DelayFrame(1)
                .Subscribe(_ => { gameObject.SetActive(false); });
        }

        public override IObservable<PierceUnit> Fire(Pawn source, Vector3 position, Quaternion rotation) {
            if (_facade != null) {
                _facade.position = position;
                _facade.rotation = rotation;
            }
            return base.Fire(source, position, rotation);
        }
	}
}