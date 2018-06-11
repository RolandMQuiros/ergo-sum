using UnityEngine;
using UniRx;

namespace ErgoSum {
    public class RaycastAim : AimingMethod {
        public override Vector3ReactiveProperty Direction { get; protected set; }
        [SerializeField]private Transform _camera;
        [SerializeField]private Transform _source;

        private void Update() {
            float sourceDistance = Vector3.Dot(_camera.position - _source.position, _camera.forward);
            Vector3 source = _camera.position + _camera.forward * sourceDistance;
            RaycastHit hitInfo;
            if (Physics.Raycast(source, _camera.forward, out hitInfo)) {
                Direction.Value = (hitInfo.point - _source.position).normalized;
            } else {
                Direction.Value = _camera.forward;
            }
        }
    }
}