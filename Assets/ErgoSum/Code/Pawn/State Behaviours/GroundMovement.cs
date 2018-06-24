using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum.States {
	public class GroundMovement : PawnStateBehaviour {
		[SerializeField]private string _terrainLayerName = "Terrain";
		[SerializeField]private float _speed;
		private Vector3 _moveDirection;
		private ContactPoint[] _contacts;

		private struct MoveUnit {
			public PawnMoveUnit Move;
			public IEnumerable<ContactPoint> Contacts;
		};
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
			int terrainLayer = LayerMask.NameToLayer(_terrainLayerName);

			var terrainCollisions = Pawn.RigidBody.OnCollisionStayAsObservable()
				.Merge(Pawn.RigidBody.OnCollisionEnterAsObservable())
				.SelectMany(collision => collision.contacts)
				.Where(c => c.otherCollider.gameObject.layer == terrainLayer)
				.Buffer(Pawn.RigidBody.LateUpdateAsObservable());
			
			terrainCollisions.Subscribe(contacts => { _contacts = contacts.ToArray(); });
			
			AddStreams(
				Pawn.Controller.Movement
					.WithLatestFrom(terrainCollisions, (move, contacts) => new MoveUnit(){ Move = move, Contacts = contacts })
					//.Select(m => new MoveUnit(){ Move = m, Contacts = null })
					.Subscribe(unit => {
						if (unit.Move.DashStart) {
							animator.SetBool("Dash", true);
							return;
						}

						// Find a directon that is not being opposed by a terrain surface
						Vector3 direction = !unit.Contacts.Any() ? unit.Move.Direction : 
						// Process the floors first
							unit.Contacts.OrderByDescending(contact => Vector3.Dot(contact.normal, Pawn.RigidBody.transform.up))
								.Aggregate(
									unit.Move.Direction,
									(projected, contact) => {
										// If we're moving into a surface, we want to project the movement direction on it, so we don't cause physics jitters from
										// overlaps
										if (Vector3.Dot(contact.normal, Pawn.RigidBody.transform.up) > 0f) {
											// If surface is a floor, move along it at full movement speed
											return Vector3.ProjectOnPlane(projected, contact.normal).normalized * unit.Move.Direction.magnitude;
										} else if (Vector3.Dot(unit.Move.Direction, contact.normal) < 0f) {
											// If the surface is a wall, move along it
											return Vector3.ProjectOnPlane(projected, contact.normal);
										} else {
											// If we're moving away from the surface, no need for projections
											return projected;
										}
									}
								);
						Vector3 velocity = Pawn.RigidBody.position + direction * _speed * Time.deltaTime;
						Pawn.RigidBody.MovePosition(velocity);
						_moveDirection = direction;
					})
			);
		}
		
		public override void OnDrawGizmos() {
			Vector3 origin = Pawn.RigidBody.position + Vector3.up;
			
			if (_contacts != null) {
				Gizmos.color = Color.green;
				Vector3 average = new Vector3();
				for (int i = 0; i < _contacts.Length; i++) {
					ContactPoint point = _contacts[i];
					Gizmos.DrawLine(origin, origin + point.normal);
					average += point.normal;
				}
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(origin, origin + average);
				Gizmos.color = Color.red;
				Gizmos.DrawLine(origin, origin + _moveDirection);
			}
		}
	}
}