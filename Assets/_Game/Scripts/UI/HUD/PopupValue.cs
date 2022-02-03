using DG.Tweening;
using GeneralTools.Pooling;
using GeneralTools.Tools;
using GeneralTools.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = GeneralTools.Tools.Random;

namespace _Idle.Scripts.UI.HUD
{
    public enum PopupValueType
    {
        None,
        Soft,
        Damage,
        Heal,
        BlockDamage,
    }
    
    public class PopupValue : BaseUIBehaviour
    {
        [SerializeField] private Vector2 _defaultPos;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Vector3 _startScale;
        [SerializeField][Range(0, 1)] private float _randomizeScale;
        [SerializeField] private float _rotation;
        [SerializeField] private float _time;
        [SerializeField] private Color _healColor;
        //[SerializeField] private Animation _animation;
        //private Tween _tween;
        private Sequence _sequence;
        private Vector2 _targetPosition;
        private Vector2 _sizeDelta => rectTransform.sizeDelta;

        public void PlaceAtDefaultPos()
        {
            rectTransform.anchoredPosition = _defaultPos;
        }

        public void PlaceAtPos(Vector2 pos)
        {
            rectTransform.anchoredPosition = pos;
        }

        private void OnAnimPlayed()
        {
            _sequence?.Kill();
            
            this.PushToPool();
        }

        public void Show(PopupValueType popupValueType, Vector2 pos, double value)
        {
            float rndScale = 0;
            switch (popupValueType)
            {
                case PopupValueType.Damage:
                    _text.color = Color.white;
                    _text.text = $"{-value}";
                    rndScale = Random.Range(0, _randomizeScale);
                    rectTransform.localScale = new Vector3(
                        _startScale.x + rndScale,
                        _startScale.y + rndScale,
                        _startScale.z + rndScale);
                    break;
                case PopupValueType.Heal:
                    _text.color = _healColor;
                    _text.text = $"+{value}";
                    rndScale = Random.Range(0, _randomizeScale);
                    rectTransform.localScale = new Vector3(
                        _startScale.x + rndScale + 0.1f,
                        _startScale.y + rndScale + 0.1f,
                        _startScale.z + rndScale + 0.1f);
                    break;
                case PopupValueType.BlockDamage:
                    _text.color = Color.gray;
                    _text.text = $"{-value}";
                    rndScale = Random.Range(0, _randomizeScale);
                    rectTransform.localScale = new Vector3(
                        _startScale.x + rndScale,
                        _startScale.y + rndScale,
                        _startScale.z + rndScale);
                    break;
            }
            
            pos = new Vector2(pos.x + Random.Range(-75, 75), pos.y + 200);
            rectTransform.Rotate(0,0,Random.Range(-_rotation, _rotation));
            
            PlaceAtPos(pos);
            //_targetPosition = new Vector2(pos.x, pos.y + 50);
            var seq = DOTween.Sequence();
            _sequence = seq;
            //_sequence.Append(rectTransform.DOAnchorPos(_targetPosition, 0.25f));
            _sequence.Append(rectTransform.DORotate(Vector3.zero, _time).SetEase(Ease.InBack));
            //_sequence.Join(rectTransform.DOSizeDelta(new Vector2(_sizeDelta.x * 1.25f,_sizeDelta.y * 1.25f), 0.25f));
            //_sequence.Append(rectTransform.DOAnchorPos(pos, 0.25f));
            _sequence.Join(rectTransform.DOScale(rectTransform.localScale * 0.7f, _time).SetEase(Ease.InBack));
            _sequence.OnComplete(Hide);
            _sequence.Play();

            this.Activate();
        }

        public void Hide()
        {
            _sequence?.Kill();
            this.PushToPool();
        }
    }
}