using System;
using UnityEngine;
using UnityEngine.Events;

namespace ErgoSum.Utilities {
    [Serializable]
    public class CollisionEvent : UnityEvent<Collision> { }
}