﻿using System;
using DG.Tweening;
using GeneralTools.Tools;
using GeneralTools.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.Message
{
    public class MessageUI : BaseUIBehaviour
    {
        public Action<MessageUI> OnEventCompleted;
        
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Image _back;
        
        private float _progress;
        private float _duration = 2f;

        private Tweener _visibleTween;
        private Tweener _moveTween;

        public void Play(string text, RectTransform rect)
        {
            _text.text = text;

            var newRect = rect.rect;
            rectTransform.localScale = rect.localScale;
            rectTransform.offsetMin = rect.offsetMin;
            rectTransform.offsetMax = rect.offsetMax;
            rectTransform.sizeDelta = rect.sizeDelta;
            rectTransform.rect.Set(newRect.x, newRect.y, newRect.width, newRect.height);
            rectTransform.anchoredPosition = new Vector2(0f, -100f);
            
            _back.color = new Color(0f, 0f, 0f, 0.8f);
            _text.color = new Color(1f, 1f, 1f, 1f);
            
            _moveTween?.Kill();
            _moveTween = rectTransform.DOAnchorPosY(0f, 0.5f);
            
            _progress = 0f;
            _visibleTween?.Kill();
            _visibleTween = DOTween.To(() => _progress, v => _progress = v, 1, _duration)
                                   .OnComplete(OnShowed);

            this.Activate();
        }
        
        private void OnShowed()
        {
            _visibleTween = DOTween.To(() => _progress, v => _progress = v, 1, 0.7f)
                .SetEase(Ease.OutCubic)
                .OnUpdate(OnUpdate)
                .OnComplete(OnCompleted);
        }

        private void OnUpdate()
        {
            _back.color = new Color(0f, 0f, 0f, _back.color.a - 0.10f);
            _text.color = new Color(1f, 1f, 1f, _back.color.a - 0.10f);
        }

        private void OnCompleted()
        {
            _visibleTween.Kill();
            _moveTween.Kill();
            
            OnEventCompleted?.Invoke(this);
        }

        public void Move(float newPos)
        {
            _moveTween?.Kill();
            _moveTween = rectTransform.DOAnchorPosY(newPos, 0.2f);
        }
    }
}