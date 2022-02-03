using GeneralTools.UI;
using Plugins.GeneralTools.Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI
{
    public class LootItemUI : BaseUIBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _textMeshProUGUI;
        [SerializeField] private Animation _animation;
        [SerializeField] private float _animationDuration;

        public int AsyncDelay => (int)(_animationDuration * 1000);

        public void SetSprite(string key)
        {
            _image.sprite = key.GetSprite();
        }

        public void SetText(string text)
        {
            _textMeshProUGUI.text = text;
        }

        public void PlayAnimation()
        {
            _animation.Play();
        }
    }
}