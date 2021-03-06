﻿using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum.UI {
	public class LockOn : MonoBehaviour {
		[SerializeField]private Camera _camera;
		[SerializeField]private AutoAim _autoAim;
		[SerializeField]private GameObject _display;
		private void Start() {
			RectTransform rectTransform = GetComponent<RectTransform>();
			_display.SetActive(false);
			_camera = _camera ?? Camera.main;
			this.UpdateAsObservable()
				.WithLatestFrom(_autoAim.Target, (_, target) => target)
				.Subscribe(
					target => {
						if (target == null) {
							_display.SetActive(false);
						} else {
							_display.SetActive(true);
							rectTransform.position = _camera.WorldToScreenPoint(target.position);
						}
					}
				);
		}
	}
}