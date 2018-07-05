using UnityEngine;
using UniRx;

namespace ErgoSum {
    public class RaycastAim : AimingMethod {
        public override IObservable<PawnAimUnit> Aim { get { return _aim; } }
        [SerializeField]private Transform _camera;
        [SerializeField]private Transform _source;
        private Vector3 _direction;
        private IObservable<PawnAimUnit> _aim;

        private void Update() {
            float sourceDistance = Vector3.Dot(_camera.position - _source.position, _camera.forward);
            Vector3 source = _camera.position + _camera.forward * sourceDistance;
            RaycastHit hitInfo;
            if (Physics.Raycast(source, _camera.forward, out hitInfo)) {
                _direction = (hitInfo.point - _source.position).normalized;
            } else {
                _direction = _camera.forward;
            }
        }
    }
}