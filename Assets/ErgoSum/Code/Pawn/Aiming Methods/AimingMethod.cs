using UnityEngine;
using UniRx;

namespace ErgoSum {
	public abstract class AimingMethod : MonoBehaviour {
		public abstract IObservable<PawnAimUnit> Aim { get; }
	}
}