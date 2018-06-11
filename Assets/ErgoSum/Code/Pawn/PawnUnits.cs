using UnityEngine;
using UniRx;

namespace ErgoSum {
    public struct PawnMoveUnit {
        public Vector3 Direction;
    }

    public struct PawnAimUnit {
        public Vector3 Direction;
    }

    public struct PawnJumpUnit {
        public bool Down;
        public bool Release;
    }
}