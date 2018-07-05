using UnityEngine;
using UniRx;

namespace ErgoSum {
    public struct PawnMoveUnit {
        public Vector3 Direction;
        public bool DashStart;
        public bool DashEnd;
    }

    public struct PawnAimUnit {
        public Vector3 Eulers;
        public Vector3 Direction;
        public Vector3 Source;
        public bool FireStart;
        public bool FireEnd;
    }

    public struct PawnJumpUnit {
        public bool Down;
        public bool Release;
    }

    public struct PawnCrouchUnit {
        public bool Start;
        public bool End;
    }
}