using System.Linq;
using _Game.Scripts.Balance;
using _Game.Scripts.Model.Unit;
using GeneralTools.Tools;
using GeneralTools.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.View
{
    public class ProgressBarView : BaseUIBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private TextMeshProUGUI _hp;
        [SerializeField] private Image _image;
        [SerializeField] private Sprite _playerSprite;
        [SerializeField] private Color _playerColor;
        [SerializeField] private Sprite _enemySprite;
        [SerializeField] private Color _enemyColor;

        private GameBalance.EnemyType _enemyType;
        private Transform _camera;

        public void Init(UnitType type, GameBalance.EnemyType enemyType)
        {
            _enemyType = enemyType;
            switch (type)
            {
                case UnitType.Player:
                    _image.sprite = _playerSprite;
                    _image.color = _playerColor;
                    break;
                case UnitType.EnemyBase:
                    _image.sprite = _enemySprite;
                    _image.color = _enemyColor;
                    break;
            }

            //var bossConfig = GameBalance.Instance.EnemiesConfig.FirstOrDefault(config => config.Type == enemyType);
            if (enemyType != GameBalance.EnemyType.None)
            {
                _hp.text = "Boss";
            }
            if (_camera == null) _camera = GameCamera.UnityCam.transform;
            this.Activate();
        }
        
        public void UpdateUI(double value, float progress)
        {
            _slider.value = progress;
            switch (_enemyType)
            {
                case GameBalance.EnemyType.None:
                case GameBalance.EnemyType.Shirt:
                case GameBalance.EnemyType.Tommy:
                case GameBalance.EnemyType.Inmate:
                    _hp.text = $"{value}";
                    break;
            }
        }
    }
}