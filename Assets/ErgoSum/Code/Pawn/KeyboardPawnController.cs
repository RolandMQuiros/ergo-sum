using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum {
	public class KeyboardPawnController : PawnController {
		public override IObservable<PawnMoveUnit> Movement { get { return _movementStream; } }
		public override IObservable<PawnAimUnit> Aim { get { return _aimStream; } }
		public override IObservable<PawnJumpUnit> Jump { get { return _jumpStream; } }
		
		#region Editor Fields
		[Header("References")]
		[SerializeField]private Transform _camera;
		[SerializeField]private Transform _cameraPivot;
		[SerializeField]private Rigidbody _body;
		[SerializeField]private AimingMethod _aim;
		[Header("Input")]
		[SerializeField]private string _horizontalAxis;
		[SerializeField]private string _verticalAxis;
		[SerializeField]private string _viewHorizontalAxis;
		[SerializeField]private string _viewVerticalAxis;
		[SerializeField]private string _primaryFire;
		[SerializeField]private string _jumpButton;
		[SerializeField]private string _dashButton;
		[SerializeField]private Vector2 _viewSensitivity = Vector2.one;
		[SerializeField]private float _verticalLimit = 60f;
		#endregion

		private IObservable<PawnMoveUnit> _movementStream;
		private IObservable<PawnAimUnit> _aimStream;
		private IObservable<PawnJumpUnit> _jumpStream;
		private Vector2 _aimEulers;

		#region MonoBehaviour
		private void Awake() {
			Cursor.lockState = CursorLockMode.Locked;
			_camera = _camera ?? Camera.main.transform;
			
			var moveInputStream = this.FixedUpdateAsObservable()
				.Select((x, y) => new Vector2(Input.GetAxis(_horizontalAxis), Input.GetAxis(_verticalAxis)));

			_movementStream = moveInputStream
				// Only capture when movement is being applied
				.Where(movement => movement != Vector2.zero)
				.Select(movement => new PawnMoveUnit() {
					Direction = Vector3.ProjectOnPlane(_camera.forward, _body.transform.up).normalized * movement.y +
						Vector3.ProjectOnPlane(_camera.right, _body.transform.up).normalized * movement.x,
					DashStart = Input.GetButtonDown(_dashButton),
					DashEnd = Input.GetButtonUp(_dashButton)
				});
			_aimStream = this.UpdateAsObservable()
				.Select((x, y) => new PawnAimUnit() {
					Eulers = new Vector2(Input.GetAxis(_viewHorizontalAxis), -Input.GetAxis(_viewVerticalAxis)),
					FireStart = Input.GetButtonDown(_primaryFire),
					FireEnd = Input.GetButtonUp(_primaryFire)
				})
				.Where(unit => unit.Eulers != Vector3.zero || (unit.FireStart ^ unit.FireEnd))
				.Select(unit => {
					Vector2 aimAxes = unit.Eulers * _viewSensitivity * Time.deltaTime;
					_aimEulers += aimAxes;
					_aimEulers.y = Mathf.Clamp(_aimEulers.y, -_verticalLimit, _verticalLimit);
					unit.Eulers = _aimEulers;
					unit.Direction = _aim.Direction;
					unit.Source = _aim.Source;
					return unit;
				});

			_jumpStream = this.UpdateAsObservable()
				.Select(x => new PawnJumpUnit() {
					Down = Input.GetButtonDown(_jumpButton),
					Release = Input.GetButtonUp(_jumpButton)
				})
				.Where(unit => unit.Down || unit.Release);
			
			_aimStream.Subscribe(unit => {
				_cameraPivot.rotation = Quaternion.Euler(unit.Eulers.y,  unit.Eulers.x, 0f);
			});
		}
		#endregion
	}
}