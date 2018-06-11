using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoSum {
	public class VerticalMeter : MonoBehaviour {
		private const int MAX_PIPS = 50;
		private const float PIP_HEIGHT = 4f;

		public int Pips {
			get { return _pips; }
			set {
				if (_pips != value) {
					int oldPips = _pips;
					_pips = Math.Max(0, Math.Min(value, _maxPips));
					if (_currentPipMove != null) {
						StopCoroutine(_currentPipMove);
					}
					_currentPipMove = StartCoroutine(MovePips(oldPips, _pips));
				}
			}
		}
		public int MaxPips {
			get { return _maxPips; }
			set {
				if (_maxPips != value) {
					int oldMax = _maxPips;
					_maxPips = Math.Max(0, Math.Min(value, MAX_PIPS));
					if (_currentPipMove != null) {
						StopCoroutine(_currentPipMove);
					}
					_currentPipMove = StartCoroutine(MoveMax(oldMax, _maxPips));
				}
			}
		}

		[SerializeField]private int _pips = 20;
		[SerializeField]private int _maxPips = 20;
		[SerializeField]private RectTransform _bounds;
		[SerializeField]private RectTransform _pipRect;
		[SerializeField]private float _pipChangeDuration;
		[SerializeField]private float _maxChangeDuration;
		private Coroutine _currentPipMove;
		private RectTransform _rectTransform;

		private void Awake() {
			_rectTransform = GetComponent<RectTransform>();
		}

		private void OnValidate() {
			_pips = Math.Max(0, Math.Min(_pips, _maxPips));
			_maxPips = Math.Max(0, Math.Min(_maxPips, MAX_PIPS));
			_rectTransform = GetComponent<RectTransform>();

			_pipRect.anchoredPosition = new Vector2(0f, -PIP_HEIGHT * (_maxPips - _pips));
			_bounds.offsetMax = new Vector2(0f, _rectTransform.offsetMax.y - PIP_HEIGHT * (MAX_PIPS - _maxPips));
		}

		private IEnumerator MovePips(int from, int to) {
			if (to > from) {
				float delay = _pipChangeDuration / (from - to);
				for (int i = from; i < to; i++) {
					_pipRect.anchoredPosition = new Vector2(0f, -PIP_HEIGHT * (_maxPips - i));
					yield return new WaitForSeconds(delay);
				}
			}
			_pipRect.anchoredPosition = new Vector2(0f, -PIP_HEIGHT * (_maxPips - _pips));
		}

		private IEnumerator MoveMax(int from, int to) {
			int delta = Math.Abs(from - to);
			float sign = Math.Sign(from - to);
			float delay = _pipChangeDuration / delta;

			for (int offset = 0; offset < delta; offset++) {
				_bounds.offsetMax = new Vector2(0f, _rectTransform.offsetMax.y - PIP_HEIGHT * sign * offset);
				yield return new WaitForSeconds(delay);
			}
			_bounds.offsetMax = new Vector2(0f, _rectTransform.offsetMax.y - PIP_HEIGHT * (MAX_PIPS - _maxPips));
		}
	}
}