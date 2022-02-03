using System;
using _Game.Scripts.Model.Numbers;
using DG.Tweening;
using GeneralTools.Model;
using UnityEngine;

namespace _Game.Scripts.Model
{
    public class LocalBoost : BaseModel
    {
        private GameParam _param;
        private float _lastValue;
        private float _lastStartValue;
        private Tween _tween;

        public event Action<LocalBoost> OnEnd;
        
        public LocalBoost(GameParam param, float k, float time, bool boostStartValue = false)
        {
            _param = param;
            _lastValue = _param.Value;
            _lastStartValue = _param.StartValue;
            if (boostStartValue)
            {
                _param.SetStartValue(_lastStartValue * k);   
            }
            _param.SetValue(_lastValue * k);
            if (time > 0)
            {
                _tween = DOTween.Sequence().AppendInterval(time).OnComplete(StopBoost);   
            }
        }

        public void StopBoost()
        {
            _tween.Kill();
            _param.SetValue(_lastValue);
            _param.SetStartValue(_lastStartValue);   
            OnEnd?.Invoke(this);
        }
    }
}