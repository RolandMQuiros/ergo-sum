using UnityEngine;

namespace ErgoSum {
	public abstract class AimingMethod : MonoBehaviour {
		public abstract Vector3 Direction { get; }
		public abstract Vector3 Source { get; }
	}
}