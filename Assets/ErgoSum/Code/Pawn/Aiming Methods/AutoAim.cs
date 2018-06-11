using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum {
	public class AutoAim : AimingMethod {
		public override Vector3ReactiveProperty Direction { get; protected set; }
		public ReactiveProperty<Transform> Target { get; private set; }
		public float Radius = 1f;
        [SerializeField]private Transform _source;
		[SerializeField]private float _maxDistance = 25f;
		[SerializeField]private LayerMask _targetMask;
		private List<Transform> _collisions = new List<Transform>();

		private void OnValidate() {
			float scaleXY = _maxDistance * Radius / Vector3.Dot(_source.position - transform.position, transform.forward);
			transform.localScale = new Vector3(scaleXY, scaleXY, _maxDistance);
		}

		private void Awake() {
			Target = new ReactiveProperty<Transform>();
			Direction = Target.Select(target => target == null ? transform.forward : (target.position - _source.position).normalized)
				.ToReactiveProperty() as Vector3ReactiveProperty;

			this.OnTriggerStayAsObservable()
				.Where(hit => hit.transform != _source)
				.Subscribe(hit => { _collisions.Add(hit.transform); });

			this.UpdateAsObservable().Subscribe(_ => {
				// Order by distance from source + distance from center of cast
				Target.Value = _collisions.OrderBy(
					hit => //Vector3.Dot(hit.transform.position, transform.forward) +
						Vector3.Cross(hit.transform.position, transform.forward).magnitude
				).FirstOrDefault();
				_collisions.Clear();
			});
		}
	}
}