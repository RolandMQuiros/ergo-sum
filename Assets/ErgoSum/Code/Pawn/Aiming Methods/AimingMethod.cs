using UnityEngine;
using UniRx;

namespace ErgoSum {
	public abstract class AimingMethod : MonoBehaviour {
		public abstract Vector3ReactiveProperty Direction { get; protected set; }
	}
}