using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoSum.Utilities {
	public abstract class LogicalStateBehaviour : StateMachineBehaviour {
		protected virtual void OnStateUpdate() { }
		protected virtual void OnStateEnter() { }
		protected virtual void OnStateExit() { }
	}

}