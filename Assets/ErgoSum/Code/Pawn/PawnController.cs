using System;
using UnityEngine;
using UniRx;

namespace ErgoSum {
    public abstract class PawnController : MonoBehaviour {
        public abstract IObservable<PawnMoveUnit> Movement { get; }
        public abstract IObservable<PawnAimUnit> Aim { get; }
        public abstract IObservable<PawnJumpUnit> Jump { get; }
    }
}