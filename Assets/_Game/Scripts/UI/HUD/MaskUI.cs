using System;
using _Game.Scripts.Balance;
using DG.Tweening;
using GeneralTools.UI;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.HUD
{
    public class MaskUI : BaseUIBehaviour
    {
        [SerializeField] private Image _mask;
        [SerializeField] private Image _blackMask;
        private Tween _tween;

        public event Action MaskEnd;

        public void SetMask(GameBalance.MaskConfig mask)
        {
            _mask.fillAmount = 1;
            _mask.sprite = mask.Image.GetSprite();
            _blackMask.sprite = mask.Image.GetSprite();
            _tween = DOTween.To(x => _mask.fillAmount = x, 1, 0, mask.Duration).OnComplete(() => MaskEnd?.Invoke());
        }

        public void KillTween()
        {
            _tween?.Kill();
        }
    }
}