using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TestMeter : MonoBehaviour {
	public float SliderValue {
		get { return _sliderValue; }
		set {
			_sliderValue = value;
			_onChange.Invoke((int)_sliderValue);
		}
	}
	[Serializable]
	private class IntEvent : UnityEvent<int> { }
	[SerializeField]private IntEvent _onChange;
	private float _sliderValue;
}
