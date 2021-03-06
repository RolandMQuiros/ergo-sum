﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum {
	public class GroundMotor : PawnMotor {
		public override IObservable<MotorMoveUnit> Movement {
			get { return _movement; }
		}

		[SerializeField]private string _terrainLayerName = "Terrain";
		private IEnumerable<ContactPoint> _contacts;
		private Rigidbody _rigidbody;
		private Subject<Vector3> _moves = new Subject<Vector3>();
		private IObservable<MotorMoveUnit> _movement;

		[SerializeField]private Vector3[] _contactNormals;
		private void Awake() {
			_rigidbody = GetComponent<Rigidbody>();
			int terrainLayer = LayerMask.NameToLayer(_terrainLayerName);

			var terrainCollisions = this.OnCollisionStayAsObservable()
				.Merge(_rigidbody.OnCollisionEnterAsObservable())
				.SelectMany(collision => collision.contacts)
				.Where(c => c.otherCollider.gameObject.layer == terrainLayer)
				.Buffer(this.UpdateAsObservable());
				
			_movement = _moves
				.WithLatestFrom(terrainCollisions, (move, contacts) => new { Move = move, Contacts = contacts })
				.Select(unit => {
					_contacts = unit.Contacts;
					_contactNormals = unit.Contacts.Select(c => c.normal.normalized).ToArray();
					// Find a directon that is not being opposed by a terrain surface
					Vector3 offset = !unit.Contacts.Any() ? unit.Move : Project(unit.Move);
					Vector3 position = _rigidbody.position + offset;
					return new MotorMoveUnit {
						OldPosition = _rigidbody.position,
						NewPosition = _rigidbody.position + offset,
						Velocity = offset,
						Contacts = _contacts
					};
				});
			_movement
				.Sample(this.FixedUpdateAsObservable())
				.Subscribe(unit => {
					_rigidbody.MovePosition(unit.NewPosition);
				});
		}
		public override void Move(Vector3 velocity) {
			_moves.OnNext(velocity);
		}

		public Vector3 Project(Vector3 toProject) {
			return _contacts.OrderByDescending(contact => Vector3.Dot(contact.normal, _rigidbody.transform.up))
				.Aggregate(
					toProject,
					(projected, contact) => {
						// If we're moving into a surface, we want to project the movement direction on it, so we don't cause physics jitters from
						// overlaps
						if (Vector3.Dot(contact.normal, _rigidbody.transform.up) > 0.3f) {
							// If surface is a floor, move along it at full movement speed
							return Vector3.ProjectOnPlane(projected, contact.normal).normalized * projected.magnitude;
						} else if (Vector3.Dot(toProject, contact.normal) < 0f) {
							// If the surface is a wall, and we're moving into it, move along it instead
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