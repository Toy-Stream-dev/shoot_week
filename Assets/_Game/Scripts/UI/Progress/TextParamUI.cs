using _Game.Scripts.Model.Numbers;
using GeneralTools.UI;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.UI.Progress
{
    public class TextParamUI : GameParamUI
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private TextAutoSize _textAutoSize;

        private GameParam _gameParam;

        public GameParam GameParam => _gameParam;
        
        public void SetParam(GameParam gameParam)
        {
            if (gameParam == null)
            {
                Debug.Log($"Cannot set null game param");
                return;
            }

            ParamType = gameParam.Type;
            
            _gameParam = gameParam;
            _gameParam.LevelChanged += Redraw;
            _gameParam.UpdatedEvent += Redraw;

            Redraw();
        }

        private void Redraw()
        {
            switch (ParamType)
            {
               default:
                    SetText(_gameParam.IntValue.ToString());
                    break;
            }
        }

        public void SetText(string text)
        {
            if (_textAutoSize != null)
            {
                _textAutoSize.Text = text;
            }
            else
            {
                _text.text = text;
            }
        }
        
        public void Destroy()
        {
            _gameParam.LevelChanged -= Redraw;
            _gameParam.UpdatedEvent -= Redraw;
        }
    }
}