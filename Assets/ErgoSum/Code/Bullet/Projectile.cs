using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum {
	[RequireComponent(typeof(Rigidbody))]
	public class Projectile : Bullet {
        public override IObservable<PierceUnit> Pierce {
            get {
                return this.OnCollisionEnterAsObservable()
                    .Where(collision => collision.gameObject != _source.Body.gameObject)
                    .Select(collision => new PierceUnit {
                        Source = _source,
                        Target = collision.gameObject,
                        Point = collision.contacts[0].point,
                        Normal = collision.contacts[0].normal,
                        Damage = _damage
                    });
            }
        }
        protected override void Awake() {
            base.Awake();
            Pierce.Take(_penetration).Subscribe(_ => { gameObject.SetActive(false); });
        }
	}
}