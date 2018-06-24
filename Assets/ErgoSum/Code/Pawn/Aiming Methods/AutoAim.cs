using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoSum {
	public class AutoAim : AimingMethod {
		public Transform Target { get; private set; }
		public override Vector3 Direction { get { return _direction; } } 
		public override Vector3 Source { get { return _source.position; } }
		public float Radius = 1f;
        [SerializeField]private Transform _source;
		[SerializeField]private float _maxDistance = 25f;
		[SerializeField]private LayerMask _obstructionMask;
		private List<Transform> _collisions = new List<Transform>();
		private Vector3 _direction = new Vector3();

		private void OnValidate() {
			float scaleXY = _maxDistance * Radius / Vector3.Dot(_source.position - transform.position, transform.forward);
			transform.localScale = new Vector3(scaleXY, scaleXY, _maxDistance);
		}
		
		private void OnTriggerStay(Collider collider) {
			if (collider.transform != _source && !Physics.Linecast(_source.position, collider.transform.position, _obstructionMask.value)) {
				_collisions.Add(collider.transform);
			}
		}

		private void FixedUpdate() {
			Target = _collisions.OrderBy(
					hit => //Vector3.Dot(hit.transform.position, transform.forward) +
						Vector3.Cross(hit.transform.position, transform.forward).magnitude
				).FirstOrDefault();
			_collisions.Clear();
			_direction = Target == null ? transform.forward : (Target.position - _source.position).normalized;
		}
	}
}