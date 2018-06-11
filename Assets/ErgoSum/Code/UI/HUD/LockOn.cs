using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum.UI {
	public class LockOn : MonoBehaviour {
		[SerializeField]private Camera _camera;
		[SerializeField]private AutoAim _autoAim;
		private void Start() {
			RectTransform rectTransform = GetComponent<RectTransform>();
			Image sprite = GetComponent<Image>();
			sprite.enabled = false;
			Transform target = null;
			_camera = _camera ?? Camera.main;
			_autoAim.Target.Subscribe(transform => { target = transform; });
			this.UpdateAsObservable()
				.Subscribe(
					_ => {
						if (target == null) {
							sprite.enabled = false;
						} else {
							sprite.enabled = true;
							rectTransform.position = _camera.WorldToScreenPoint(target.position);
						}
					}
				);
		}
	}
}