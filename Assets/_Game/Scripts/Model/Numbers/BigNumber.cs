using System;
using GeneralTools.Tools.ExtensionMethods;
using UnityEngine;

namespace _Game.Scripts.Model.Numbers
{
    [Serializable]
    public class BigNumber
    {
        public event Action UpdatedEvent;

        [SerializeField] private float _sourceValue;
        [SerializeField] private float _value;
        [SerializeField] private float _boostK = 1f;
        [SerializeField] private float _startValue;
        [SerializeField] private bool _startValueSet;
       
        private float _delta;

        public bool BlockChanges { get; set; }
        public float StartValue => _startValue;
        public float Value => _value;
        public float Delta => _delta;
        public float SourceValue => _sourceValue;
        public int IntValue => (int)Math.Round(Value);
        public int BoostK => (int)_boostK;

        public BigNumber()
        {
        }

        public BigNumber(float value)
        {
            Init(value);
        }

        protected void Init(float value)
        {
            if (!_startValueSet)
            {
                _startValue = value;
                if (_startValue > 0)
                {
                    _startValueSet = true;
                }
            }
            SetSourceValue(value);
            _value = _sourceValue * _boostK;
        }

        public void SetStartValue(float value)
        {
            _startValue = value;
        }

        public virtual void SetValue(float value)
        {
            if(BlockChanges) return;
            if (!_startValueSet)
            {
                _startValueSet = true;
                _startValue = value;
            }
            SetSourceValue(value);
            _value = _sourceValue * _boostK;
            UpdatedEvent?.Invoke();
        }

        public virtual double GetValue(float value)
        {
            return value * _boostK;
        }

        public void Change(BigNumber delta)
        {
            Change(delta.Value);
        }

        public virtual void Change(float delta)
        {
            if(BlockChanges) return;
            SetSourceValue(_sourceValue + delta);
            _delta = delta;
            _value += delta;
            UpdatedEvent?.Invoke();
        }

        private void SetSourceValue(float value)
        {
            _sourceValue = value;
        }

        public void CopyFrom(BigNumber source)
        {
            if (!_sourceValue.EqualTo(source._sourceValue))
            {
                SetValue(source._sourceValue);
            }
        }

        public override string ToString()
        {
            return Value.ToString();
        }
        
        public static implicit operator BigNumber(float d) => new BigNumber(d);
    }
}
