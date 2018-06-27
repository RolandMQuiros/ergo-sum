using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum {
	public class GroundMotor : PawnMotor {
		[SerializeField]private string _terrainLayerName = "Terrain";
		private IEnumerable<ContactPoint> _contacts;
		private Rigidbody _rigidbody;
		private Subject<Vector3> _movement = new Subject<Vector3>();
		private void Start() {
			_rigidbody = GetComponent<Rigidbody>();
			int terrainLayer = LayerMask.NameToLayer(_terrainLayerName);

			var terrainCollisions = this.OnCollisionStayAsObservable()
				.Merge(_rigidbody.OnCollisionEnterAsObservable())
				.SelectMany(collision => collision.contacts)
				.Where(c => c.otherCollider.gameObject.layer == terrainLayer)
				.Buffer(this.LateUpdateAsObservable());
			
			_movement
				.WithLatestFrom(this.FixedUpdateAsObservable(), (move, _) => move)
				.WithLatestFrom(terrainCollisions, (move, contacts) => new { Move = move, Contacts = contacts })
				.Subscribe(unit => {
					_contacts = unit.Contacts;
					// Find a directon that is not being opposed by a terrain surface
					Vector3 offset = !unit.Contacts.Any() ? unit.Move : Project(unit.Move);
					Vector3 velocity = _rigidbody.position + offset;
					_rigidbody.MovePosition(velocity);
				});
		}
		public override void Move(Vector3 velocity) {
			_movement.OnNext(velocity);
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
						} else if (Vector3.Dot(toProject, contact.normal) < 0f) {
							// If the surface is a wall, move along it
							return Vector3.ProjectOnPlane(projected, contact.normal);
						} else {
							// If we're moving away from the surface, no need for projections
							return projected;
						}
					}
				);
		}

		private void OnDrawGizmos() {
			Vector3 origin = transform.position + Vector3.up;
			if (_contacts != null) {
				Gizmos.color = Color.green;
				Vector3 average = new Vector3();
				foreach (var point in _contacts) {
					Gizmos.DrawLine(origin, origin + point.normal);
					average += point.normal;
				}
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(origin, origin + average);
				Gizmos.color = Color.red;
			}
		}
	}
}