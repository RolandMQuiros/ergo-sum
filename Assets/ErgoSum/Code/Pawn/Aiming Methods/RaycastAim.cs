using UnityEngine;

namespace ErgoSum {
    public class RaycastAim : AimingMethod {
        public override Vector3 Direction { get { return _direction; } }
        public override Vector3 Source { get { return _source.position; } }
        [SerializeField]private Transform _camera;
        [SerializeField]private Transform _source;
        private Vector3 _direction;

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