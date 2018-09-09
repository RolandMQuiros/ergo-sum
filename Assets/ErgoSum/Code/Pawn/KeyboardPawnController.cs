using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace ErgoSum {
	public class KeyboardPawnController : PawnController {
		public override IObservable<PawnMoveUnit> Movement { get { return _movementStream; } }
		public override IObservable<PawnCrouchUnit> Crouch { get { return _crouchStream; } }
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
		[SerializeField]private string _crouchButton;
		[SerializeField]private string _jumpButton;
		[SerializeField]private string _dashButton;
		[SerializeField]private Vector2 _viewSensitivity = Vector2.one;
		[SerializeField]private float _verticalLimit = 60f;
		#endregion

		private IObservable<PawnMoveUnit> _movementStream;
		private IObservable<PawnCrouchUnit> _crouchStream;
		private IObservable<PawnAimUnit> _aimStream;
		private IObservable<PawnJumpUnit> _jumpStream;

		#region MonoBehaviour
		private void Awake() {
			Cursor.lockState = CursorLockMode.Locked;
			_camera = _camera ?? Camera.main.transform;
			
			var moveInputStream = this.UpdateAsObservable()
				.Select((x, y) => new Vector2(Input.GetAxis(_horizontalAxis), Input.GetAxis(_verticalAxis)));

			_movementStream = moveInputStream
				.Pairwise()
				// Only capture when movement is being applied
				.Where(pair => pair.Current != Vector2.zero || pair.Previous - pair.Current != Vector2.zero)
				.Select(pair => {
					float angle = Mathf.Atan2(pair.Current.y, pair.Current.x);
					float maxX = Mathf.Abs(Mathf.Cos(angle));
					float maxY = Mathf.Abs(Mathf.Sin(angle));
					return new Vector2(
						Mathf.Clamp(Mathf.Abs(pair.Current.x), 0f, maxX) * Mathf.Sign(pair.Current.x),
						Mathf.Clamp(Mathf.Abs(pair.Current.y), 0f, maxY) * Mathf.Sign(pair.Current.y)
					);
				})
				.Select(move => new PawnMoveUnit {
					Direction = Vector3.ProjectOnPlane(_camera.forward, _body.transform.up).normalized *move.y +
						Vector3.ProjectOnPlane(_camera.right, _body.transform.up).normalized * move.x,
					DashStart = Input.GetButtonDown(_dashButton),
					DashEnd = Input.GetButtonUp(_dashButton)
				});

			_movementStream.Where(m => m.DashStart).Subscribe(_ => { Debug.Log("Dahsed"); });
			
			_crouchStream = this.UpdateAsObservable()
				.Select(_ => new PawnCrouchUnit {
					Start = Input.GetButtonDown(_crouchButton),
					End = Input.GetButtonUp(_crouchButton)
				})
				.Where(unit => unit.Start ^ unit.End);

			Vector2 aimAxes = Vector2.zero;
			_aimStream = this.UpdateAsObservable()
				.Select(mouseDelta => new {
					Axes = new Vector2(-Input.GetAxis(_viewVerticalAxis), Input.GetAxis(_viewHorizontalAxis)),
					FireStart = Input.GetButtonDown(_primaryFire),
					FireEnd = Input.GetButtonUp(_primaryFire)
				})
				.Where(inUnit => inUnit.Axes != Vector2.zero || inUnit.FireStart ^ inUnit.FireEnd)
				.WithLatestFrom(
					_aim.Aim,
					(inUnit, aimUnit) => {
						aimUnit.Eulers = aimAxes + inUnit.Axes * _viewSensitivity * Time.deltaTime;
						aimUnit.Eulers.x = Mathf.Clamp(aimUnit.Eulers.x, -_verticalLimit, _verticalLimit);
						aimUnit.FireStart = inUnit.FireStart;
						aimUnit.FireEnd = inUnit.FireEnd;
						return aimUnit;
					}
				);
			_aimStream.Subscribe(unit => {
				aimAxes.x = unit.Eulers.x;
				aimAxes.y = unit.Eulers.y;
				_cameraPivot.rotation = Quaternion.Euler(unit.Eulers.x,  unit.Eulers.y, 0f);
			});

			_jumpStream = this.UpdateAsObservable()
				.Select(x => new PawnJumpUnit() {
					Down = Input.GetButtonDown(_jumpButton),
					Release = Input.GetButtonUp(_jumpButton)
				})
				.Where(unit => unit.Down || unit.Release);
		}
		#endregion
	}
}