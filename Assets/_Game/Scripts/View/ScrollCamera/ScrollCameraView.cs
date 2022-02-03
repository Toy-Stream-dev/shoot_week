using _Idle.Scripts.UI;
using DG.Tweening;
using GeneralTools.UI;
using GeneralTools.View;
using UnityEngine;

namespace _Game.Scripts.View.ScrollCamera
{
    public class ScrollCameraView : BaseView
    {
        [SerializeField] private Transform _cameraParent;
        [SerializeField] private Transform _camera;
        [SerializeField] private Transform _player;
        private Tween _tween;
        private bool _isActive;
        
        public bool IsInteractable { get; set; }

        public override void StartMe()
        {
            GameUI.Get<InputScroll>().OnValueChanged += OnDrag;
            _isActive = true;
            base.StartMe();
        }
        

        public override void UpdateMe(float deltaTime)
        {
            if(IsInteractable || !_isActive) return;
            //Quaternion rotationX = Quaternion.AngleAxis(GameBalance.Instance.FishRotateSpeed, Vector3.forward);
            //_car.rotation *= rotationX;

            base.UpdateMe(deltaTime);
        }

        public void SetActive(bool value)
        {
            _isActive = value;
            if (value)
            {
                GameUI.Get<InputScroll>().OnValueChanged -= OnDrag;
            }
        }

        private void OnDrag(Vector2 delta)
        {
            _cameraParent.Rotate(Vector3.up * 2, delta.x / 2, Space.Self);   
        }
    }
}