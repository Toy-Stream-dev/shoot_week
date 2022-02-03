using System.Collections.Generic;
using _Game.Scripts;
using _Game.Scripts.Enums;
using _Game.Scripts.Model.Numbers;
using GeneralTools.Pooling;
using GeneralTools.Tools;
using Plugins.GeneralTools.Scripts.UI;
using UnityEngine;

namespace _Idle.Scripts.UI.HUD
{
    public class PopupValueContainer : BaseWindow
    {
        [SerializeField] private RectTransform _canvasRect;
        private Vector2 _sizeDelta => _canvasRect.sizeDelta;
        // private GameProgress _timer;
        // private GameProgress _zeroTimer;
        // private int _countOfCombo;
        // private int _combo = 1;
        // private int _maxCombo = 10;
        //private readonly List<PopupRate> _rates = new List<PopupRate>();

        // public int Combo => _combo;

        public override void Init()
        {
            Pool.Spawn<PopupValue>(60);
            // _timer = new GameProgress(GameParamType.Timer, 0.5f);
            // _timer.CompletedEvent += ComboComplete;
            // _timer.Pause();
            // _zeroTimer = new GameProgress(GameParamType.Timer, 15);
            // _zeroTimer.CompletedEvent += ClearCombo;
            // _zeroTimer.Pause();
            base.Init();

            this.Activate();
        }

        // public override void UpdateMe(float deltaTime)
        // {
        //     _timer.Change(deltaTime);
        //     _zeroTimer.Change(deltaTime);
        //     base.UpdateMe(deltaTime);
        // }
        //
        // private void ComboComplete()
        // {
        //     _timer.Pause();
        // }

        public void Show(PopupValueType popupValueType, Vector3 pos, double value)
        {
            Vector2 newPos = GameCamera.UnityCam.WorldToViewportPoint(pos);
            var newRate = Pool.Pop<PopupValue>(transform);
            newRate.Show(popupValueType, CalculateToScreen(newPos), value);
        }

        public Vector2 CalculateToScreen(Vector2 pos)
        {
            return new Vector2(
                ((pos.x * _sizeDelta.x)-(_sizeDelta.x * 0.5f)),
                ((pos.y * _sizeDelta.y)-(_sizeDelta.y * 0.5f)));
        }
    }
}