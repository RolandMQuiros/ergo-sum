using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum {
	public struct PierceUnit {
		public Pawn Source;
		public GameObject Target;
		public Vector3 Point;
		public Vector3 Normal;
		public int Damage;
	}
	public abstract class Bullet : MonoBehaviour {
		public abstract IObservable<PierceUnit> Pierce { get; }

		[Serializable]
		protected class BulletEvent : UnityEvent<Bullet> { }

		[Tooltip("How much damage this Bullet inflicts on Pawns")]
		[SerializeField]protected int _damage;
		[Tooltip("How many objects this bullet can penetrate before disabling")]
		[SerializeField]protected int _penetration;
		[Tooltip("Notifies child objects that this bullet has pierced an object. Use to leave decals or other FX, for example.")]
		[SerializeField]protected BulletEvent _pierceEvent;
		protected Pawn _source;

		protected virtual void Awake() {
			Pierce.Subscribe(unit => {
				var targetPawn = unit.Target.GetComponent<Pawn>();
				if (targetPawn != null) {
					targetPawn.Pierce(unit);
				}
				_pierceEvent.Invoke(this);
			});
		}

		public virtual IObservable<PierceUnit> Fire(Pawn source, Vector3 position, Quaternion rotation) {
			_source = source;
			transform.position = position;
			transform.rotation = rotation;
			gameObject.SetActive(true);
			return Pierce;
		}
	}
}