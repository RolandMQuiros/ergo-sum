﻿using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace ErgoSum {
	public struct MotorMoveUnit {
		public Vector3 Velocity;
		public Vector3 OldPosition;
		public Vector3 NewPosition;
		public IEnumerable<ContactPoint> Contacts;
	}

	[RequireComponent(typeof(Rigidbody))]
	public abstract class PawnMotor : MonoBehaviour {
		public abstract IObservable<MotorMoveUnit> Movement { get; }
		public abstract void Move(Vector3 velocity);
	}
}