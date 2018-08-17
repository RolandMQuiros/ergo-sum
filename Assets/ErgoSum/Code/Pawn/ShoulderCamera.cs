using UnityEngine;

namespace ErgoSum {
	public class ShoulderCamera : MonoBehaviour {
		[Tooltip("The parent transform that controls this camera's rotation and offset")]
		[SerializeField]private Transform _pivot;
		[Tooltip("Objects that obstruct camera view and trigger a zoom")]
		[SerializeField]private LayerMask _obstruction;
		[Tooltip("The small sphere surrounding the camera preventing it from looking behind geometry faces")]
		[SerializeField]private float _radius;
		[Tooltip("The camera's resting position when not obstructed by terrain")]
		[SerializeField]private Vector3 _restPosition;
		private void OnEnable() {
			_pivot = _pivot ?? transform.parent;
			_restPosition = transform.localPosition;
		}
		
		private void Update() {
			RaycastHit hitInfo;
			Vector3 start = _pivot.position;
			Vector3 end = _pivot.TransformPoint(_restPosition);
			Vector3 diff = end - start;

			float distance = diff.magnitude;
			if (Physics.SphereCast(start, _radius, diff.normalized, out hitInfo, distance, _obstruction.value)) {
				distance = Mathf.Clamp(hitInfo.distance, _radius, diff.magnitude);
			}
			transform.position = start + diff.normalized * distance;
		}

		private void OnDrawGizmos() {
			Gizmos.color = Color.magenta;
			Gizmos.DrawSphere(_pivot.position, _radius);
			Gizmos.DrawSphere(_pivot.TransformPoint(_restPosition), _radius);
			Gizmos.DrawSphere(transform.position, _radius);
		}
	}
}