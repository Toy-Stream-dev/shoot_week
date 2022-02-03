using System.Collections.Generic;
using System.Linq;
using GeneralTools.Pooling;
using Plugins.GeneralTools.Scripts.UI;
using UnityEngine;
namespace _Game.Scripts.UI
{
    public class IndicatorWindow : BaseWindow
    {
        
        [SerializeField] private RectTransform _indicatorsContainer;
        [SerializeField] private int _indicatorSize = 100;
        [SerializeField] private float _saveAreaDownOffset;

        [SerializeField] private List<Indicator> _indicators = new List<Indicator>();
        private float _border;
        private float _halfScreenHeight;
        private float _halfScreenWidth;

        
        public override void Init()
        {
            _halfScreenHeight = Screen.height / 2f;
            _halfScreenWidth = Screen.width / 2f;

            if (_indicatorsContainer == null)
            {
                Debug.LogError("Need canvas for Indicators");
                return;
            }

            _border = _indicatorSize / 2f;
            
            Pool.Spawn<Indicator>(50);
            
            base.Init();
        }

        public Indicator AddIndicator(Transform target)
        {
            var newIndicator = Pool.Pop<Indicator>(_indicatorsContainer.transform);
            newIndicator.transform.localScale = Vector3.one;
            newIndicator.rectTransform.sizeDelta = new Vector2(_indicatorSize, _indicatorSize);
            newIndicator.Target = target;
            _indicators.Add(newIndicator);

            return newIndicator;
        }

        public void RemoveIndicator(Indicator indicator)
        {
            _indicators.Remove(indicator);
            indicator.Target = null;
            indicator.PushToPool();
        }

        private void LateUpdate()
        {
            foreach (var indicator in _indicators)
            {
                if(indicator.Hiden) continue;
                UpdateIndicator(indicator);
            }
        }

        private void UpdateIndicator(Indicator indicator)
        {
			var screenPoint = Camera.main.WorldToScreenPoint(indicator.Target.localPosition);
            Vector3 newPosition;
			float angle;

            var heading = indicator.Target.position - Camera.main.transform.position;
			var isBehindCamera = Vector3.Dot(Camera.main.transform.forward, heading) < 0;
            if (screenPoint.x > Screen.width - _border || screenPoint.x < _border ||
                screenPoint.y > Screen.height - _border || screenPoint.y < _border || isBehindCamera)
            {
                //indicator.IsOnScreen = false;
                angle = Mathf.Atan2(screenPoint.y - _halfScreenHeight, screenPoint.x - _halfScreenWidth);
                float x, y;
                if (screenPoint.x - _halfScreenWidth > 0)
                {
                    // right
                    x = _halfScreenWidth - _border;
                    y = x * Mathf.Tan(angle);
                }
                else
                {
                    // left
                    x = -_halfScreenWidth + _border;
                    y = x * Mathf.Tan(angle);
                }

                if (y > _halfScreenHeight - _border)
                {
                    // up
                    y = _halfScreenHeight - _border;
                    x = y / Mathf.Tan(angle);
                }

                if (y < -_halfScreenHeight + _border)
                {
                    // down
                     y = -_halfScreenHeight + _border + (_saveAreaDownOffset / 2);
                     x = y / Mathf.Tan(angle);
                }

                if (isBehindCamera)
                {
                    x = -x;
                    y = -y;
                }

                newPosition = new Vector3(x, y, 0);
            }
            else
            {
                //indicator.IsOnScreen = true;
                var x = screenPoint.x - _halfScreenWidth;
                var y = screenPoint.y - _halfScreenHeight;
                newPosition = new Vector3(x, y, 0);
            }

            indicator.transform.localPosition =
                newPosition;
            //rotate
            // if ((indicator.IsOnScreen && indicator.RotateOnScreen) ||
            //     (!indicator.IsOnScreen && indicator.RotateOffScreen))
            // {
            //     angle = isBehindCamera ? Mathf.Atan2(-(screenPoint.y - _halfScreenHeight), -(screenPoint.x - _halfScreenWidth))
            //         : Mathf.Atan2(screenPoint.y - _halfScreenHeight, screenPoint.x - _halfScreenWidth);
            // }
            // else
            // {
            //     angle = 90 * Mathf.Deg2Rad;
            // }
            angle = isBehindCamera ? Mathf.Atan2(-(screenPoint.y - _halfScreenHeight), -(screenPoint.x - _halfScreenWidth))
                : Mathf.Atan2(screenPoint.y - _halfScreenHeight, screenPoint.x - _halfScreenWidth);
            indicator.transform.localEulerAngles = new Vector3(0, 0, angle * Mathf.Rad2Deg - 90); 
        }
    }

}
        
        