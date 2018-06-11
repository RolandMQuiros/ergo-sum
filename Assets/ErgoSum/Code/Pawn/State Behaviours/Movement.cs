using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum.States {
	public class Movement : PawnStateBehaviour {
		[SerializeField]private string _contactMaterials;
		[SerializeField]private string _terrainLayerName = "Terrain";
		[SerializeField]private float _speed;
		private Vector3 _moveDirection;
		private float _maxJumpSpeed;
		private float _minJumpSpeed;
		private ContactPoint[] _contacts;
		private int _terrainLayer;
		protected override void OnPawnAttach(Pawn pawn) {
			_maxJumpSpeed = Mathf.Sqrt(Pawn.MaxJumpHeight * 2f * Pawn.Gravity);
			_minJumpSpeed = Mathf.Sqrt(Pawn.MinJumpheight * 2f * Pawn.Gravity);
			_terrainLayer = LayerMask.NameToLayer(_terrainLayerName);
		}
		protected override void OnStateEnter() {
			AddStreams(
				Pawn.Controller.Movement.Subscribe(unit => {
					// Find a directon that is not being opposed by a terrain surface
					Vector3 direction = _contacts == null ? unit.Direction : 
					// Process the floors first
						_contacts.OrderByDescending(contact => Vector3.Dot(contact.normal, Pawn.RigidBody.transform.up))
							.Aggregate(
								unit.Direction,
								(projected, contact) => {
									// If we're moving into a surface, we want to project the movement direction on it, so we don't cause physics jitters from
									// overlaps
									if (Vector3.Dot(contact.normal, Pawn.RigidBody.transform.up) > 0f) {
										// If surface is a floor, move along it at full movement speed
										return Vector3.ProjectOnPlane(projected, contact.normal).normalized * unit.Direction.magnitude;
									} else if (Vector3.Dot(unit.Direction, contact.normal) < 0f) {
										// If the surface is a wall, move along it
										return Vector3.ProjectOnPlane(projected, contact.normal);
									} else {
										// If we're moving away from the surface, no need for projections
										return projected;
									}
								}
							);
					Vector3 velocity = Pawn.RigidBody.position + direction * Pawn.MoveSpeed * Time.deltaTime;
					Pawn.RigidBody.MovePosition(velocity);
					_speed = velocity.magnitude;
					_moveDirection = direction;
				}),
				Pawn.Controller.Jump
					.Subscribe(unit => {
						if (unit.Down && Pawn.IsGrounded()) {
							Pawn.RigidBody.AddForce(_maxJumpSpeed * Pawn.RigidBody.transform.up, ForceMode.VelocityChange);
						} else if (unit.Release) {
							float jumpSpeed = Vector3.Dot(Pawn.RigidBody.velocity, Pawn.RigidBody.transform.up) - _minJumpSpeed;
							if (jumpSpeed > 0f) {
								Pawn.RigidBody.AddForce(jumpSpeed * -Pawn.RigidBody.transform.up, ForceMode.VelocityChange);
							}
						}
					}),
				Observable.EveryFixedUpdate().Subscribe(_ => Pawn.RigidBody.AddRelativeForce(Vector3.down * Pawn.Gravity, ForceMode.Acceleration)),
				Pawn.RigidBody.OnCollisionStayAsObservable().Subscribe(
					collision => {
						_contacts = collision.contacts.Where(c => c.otherCollider.gameObject.layer == _terrainLayer).ToArray();
						_contactMaterials = string.Join(", ", _contacts.Select(c => c.thisCollider?.sharedMaterial?.name).Distinct());
					}
				),
				Pawn.RigidBody.OnCollisionExitAsObservable().Subscribe(Collider => { _contacts = null; })
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