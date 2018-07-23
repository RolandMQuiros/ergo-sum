using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum {
	[RequireComponent(typeof(Rigidbody))]
	public class Bullet : MonoBehaviour {
		public Pawn Source { get; set; }
		public IObservable<Collision> Pierce { get; private set; }
		public int Damage { get { return _damage; } }
		public int Penetration { get { return _penetration; } }

		[Serializable]
		private class BulletEvent : UnityEvent<Bullet> { }

		[SerializeField]private int _damage;
		[SerializeField]private int _penetration;
		[SerializeField]private BulletEvent _pierced;

		private GameObject _source;

		private void Awake() {
			Pierce = this.OnCollisionEnterAsObservable().Where(collision => collision.gameObject != _source);
			Pierce.Subscribe(_ => { _pierced.Invoke(this); });
			Pierce.Take(_penetration).Subscribe(_ => { gameObject.SetActive(false); });
		}

		public void Fire(GameObject source, Vector3 position, Quaternion rotation) {
			_source = source;
			transform.position = position;
			transform.rotation = rotation;
			gameObject.SetActive(true);
		}
	}
}