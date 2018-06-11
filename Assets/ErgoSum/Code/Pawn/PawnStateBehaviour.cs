using System;
using System.Collections.Generic;
using UnityEngine;
using ErgoSum.Utilities;

namespace ErgoSum {
	public abstract class PawnStateBehaviour : LogicalStateBehaviour {
		public Pawn Pawn {
			get { return _pawn; }
			set {
				if (_pawn != null) {
					OnPawnDetach(_pawn);
				}
				_pawn = value;
				OnPawnAttach(_pawn);
			}
		}
		private Pawn _pawn;
		private List<IDisposable> _streams = new List<IDisposable>();
		protected void AddStreams(IEnumerable<IDisposable> streams) {
			foreach (IDisposable stream in streams) { _streams.Add(stream); }
		}
		protected void AddStreams(params IDisposable[] streams) {
			foreach (IDisposable stream in streams) { _streams.Add(stream); }
		}
		protected override void OnStateExit() { _streams.ForEach(s => s.Dispose()); }
		protected virtual void OnPawnAttach(Pawn pawn) { }
		protected virtual void OnPawnDetach(Pawn pawn) { }
		public virtual void OnDrawGizmos() { }
	}
}