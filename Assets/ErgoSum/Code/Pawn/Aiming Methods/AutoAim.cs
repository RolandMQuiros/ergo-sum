using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum {
	public class AutoAim : AimingMethod {
		public ReactiveProperty<Transform> Target { get; private set; }
		public override IObservable<PawnAimUnit> Aim { get { return _aim; } }
		public float Radius = 1f;
        [SerializeField]private Transform _source;
		[SerializeField]private float _maxDistance = 25f;
		[SerializeField]private LayerMask _obstructionMask;
		private IObservable<PawnAimUnit> _aim;

		private void OnValidate() {
			float scaleXY = _maxDistance * Radius / Vector3.Dot(_source.position - transform.position, transform.forward);
			transform.localScale = new Vector3(scaleXY, scaleXY, _maxDistance);
		}

		private void Awake() {
			Target = new ReactiveProperty<Transform>();
			var aimStream = this.OnTriggerStayAsObservable()
				.Where(
					collider => collider != _source && !Physics.Linecast(_source.position, collider.transform.position, _obstructionMask.value)
				)
				.Buffer(this.UpdateAsObservable())
				.Select(
					collisions => collisions.OrderBy(
						hit => Vector3.Cross(hit.transform.position, transform.forward).magnitude
					).FirstOrDefault()
				);
			_aim = aimStream.Select(
					target => new PawnAimUnit {
						Direction = target == null ? transform.forward : (target.transform.position - _source.position).normalized,
						Source = _source.position
					}
				);
			aimStream.Subscribe(target => { Target.Value = target != null ? target.transform : null; });
		}
	}
}