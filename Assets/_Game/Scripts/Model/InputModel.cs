using _Game.Scripts.Model.Unit;
using _Game.Scripts.Model.Unit.Components;
using GeneralTools.Model;
using GeneralTools.UI;
using UnityEngine;

namespace _Game.Scripts.Model
{
	public enum InputState
	{
		None,
		Play,
		Pause,
	}
	
	public class InputModel : BaseModel
	{
		private Vector2 _startInputJoystick;
		private Vector2 _inputCurrent;
		private Vector2 _inputDelta;
		
		private bool _pressed;
		
		private PlayerMovementComponent _playerMovementComponent;
		private Joystick _joystick;
		private Vector3 _input;
		private InputState _inputState;
		private UnitModel _player;
		private GameModel _gameModel;
		
		public override BaseModel Start()
		{
			_gameModel = Models.Get<GameModel>();
			_joystick = GameUI.Get<Joystick>();

			return base.Start();
		}

		public void SetPlayer(UnitModel player)
		{
			_player = player;
			_playerMovementComponent = _player.GetComponent<PlayerMovementComponent>();
			SetState(InputState.Pause);
		}

		public override void Update(float deltaTime)
		{
			if(_inputState == InputState.Pause) return;
			CheckInput();
			if (_pressed)
			{
				Pressed(deltaTime);
			}
			else
			{
				_inputCurrent = Vector2.zero;
			}

			if (_gameModel.Player != null)
			{
				_playerMovementComponent.Move(_inputCurrent, deltaTime);
			}
			
			base.Update(deltaTime);
		}

		public void SetState(InputState state)
		{
			if(_inputState == state) return;
			_inputState = state;
			switch (state)
			{
				case InputState.Pause:
					_playerMovementComponent.Move(Vector2.zero, 0);
					break;
			}
		}
		
		private void CheckInput()
		{			
			if (Input.GetMouseButtonDown(0))
			{
				PointerDown();
			}
			if (Input.GetMouseButtonUp(0))
			{
				PointerUp();
			}
		}
		
		private void PointerDown()
		{
			_pressed = true;
			_startInputJoystick = Input.mousePosition;
		}
		
		private void Pressed(float deltaTime)
		{
			_inputCurrent = Input.mousePosition;
			_inputCurrent = _startInputJoystick - _inputCurrent;
			_inputCurrent *= deltaTime * 3f;
		}
		
		private void PointerUp()
		{
			_pressed = false;
		}
	}
}