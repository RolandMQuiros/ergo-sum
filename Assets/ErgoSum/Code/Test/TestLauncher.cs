using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class TestLauncher : MonoBehaviour {
	[SerializeField]private GameObject _bulletPrefab;
	[SerializeField]private float _launchSpeed = 10f;
	private Queue<GameObject> _bullets = new Queue<GameObject>();

	private void Start() {
		Observable.Interval(TimeSpan.FromSeconds(1.0))
			.Subscribe(_ => {
				if (_bullets.Count == 0) {
					_bullets.Enqueue(GameObject.Instantiate(_bulletPrefab, transform.position, transform.rotation));
				}
				GameObject bullet = _bullets.Dequeue();
				bullet.transform.position = transform.position;
				bullet.SetActive(true);
				Rigidbody bulletBody = bullet.GetComponent<Rigidbody>();
				bulletBody.velocity = _launchSpeed * transform.forward;
				Observable.Timer(TimeSpan.FromSeconds(5f)).Subscribe(x => {
					bullet.SetActive(false);
					_bullets.Enqueue(bullet);
				});
			});
	}
}
