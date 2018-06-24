using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum {
	public class GroundMotor : PawnMotor {
		[SerializeField]private string _terrainLayerName = "Terrain";
		private Vector3 _accumulator;
		private IEnumerable<ContactPoint> _contacts;
		private Rigidbody _rigidbody;
		private void Start() {
			_rigidbody = GetComponent<Rigidbody>();
			int terrainLayer = LayerMask.NameToLayer(_terrainLayerName);

			var terrainCollisions = this.OnCollisionStayAsObservable()
				.Merge(_rigidbody.OnCollisionEnterAsObservable())
				.SelectMany(collision => collision.contacts)
				.Where(c => c.otherCollider.gameObject.layer == terrainLayer)
				.Buffer(this.LateUpdateAsObservable());
			
			this.FixedUpdateAsObservable()
				.Where(_ => _accumulator != Vector3.zero)
				.WithLatestFrom(terrainCollisions, (_, contact) => contact)
				.Subscribe(contacts => {
					_contacts = contacts;
					// Find a directon that is not being opposed by a terrain surface
					Vector3 offset = !contacts.Any() ? _accumulator : Project(_accumulator);
					Vector3 velocity = _rigidbody.position + offset;
					_rigidbody.MovePosition(velocity);
					_accumulator = Vector3.zero;
				});
		}
		public override void Move(Vector3 velocity) {
			_accumulator += velocity;
		}

		public Vector3 Project(Vector3 toProject) {
			return _contacts.OrderByDescending(contact => Vector3.Dot(contact.normal, _rigidbody.transform.up))
				.Aggregate(
					toProject,
					(projected, contact) => {
						// If we're moving into a surface, we want to project the movement direction on it, so we don't cause physics jitters from
						// overlaps
						if (Vector3.Dot(contact.normal, _rigidbody.transform.up) > 0f) {
							// If surface is a floor, move along it at full movement speed
							return Vector3.ProjectOnPlane(projected, contact.normal).normalized * projected.magnitude;
						} else if (Vector3.Dot(_accumulator, contact.normal) < 0f) {
							// If the surface is a wall, move along it
							return Vector3.ProjectOnPlane(projected, contact.normal);
						} else {
							// If we're moving away from the surface, no need for projections
							return projected;
						}
					}
				);
		}
	}
}