using System;
using _Game.Scripts.Enums;
using _Game.Scripts.Model;
using _Game.Scripts.View.ScrollCamera;
using GeneralTools.Model;
using GeneralTools.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Idle.Scripts.UI
{
    public class InputScroll : BaseUI, IDragHandler, IEndDragHandler
    {
        public Action<Vector2> OnValueChanged;
        private ScrollCameraView _scrollCamera;
        private GameModel _gameModel;

        public override void StartMe()
        {
            _gameModel = Models.Get<GameModel>();
            //_scrollCamera = _gameModel.Car.GarageVersion.ScrollCameraView;

            base.StartMe();
        }

        private void OnLevelChanged()
        {
            //_scrollCamera = _gameModel.Car.GarageVersion.ScrollCameraView;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            //_scrollCamera.IsInteractable = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            //_scrollCamera.IsInteractable = true;
            OnValueChanged(eventData.delta);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            //_scrollCamera.IsInteractable = false;
        }
    }
}