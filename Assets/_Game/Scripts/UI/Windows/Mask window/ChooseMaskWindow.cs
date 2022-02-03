using System.Linq;
using _Game.Scripts.Ad;
using _Game.Scripts.Balance;
using _Game.Scripts.Enums;
using _Game.Scripts.Model;
using DG.Tweening;
using GeneralTools.Model;
using GeneralTools.Tools;
using GeneralTools.UI;
using Plugins.GeneralTools.Scripts.UI;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.UI.Windows.Mask_window
{
    public class ChooseMaskWindow : BaseWindow
    {
        [SerializeField] private TextMeshProUGUI _soft;
        [SerializeField] private TextMeshProUGUI _hard;
        [SerializeField] private BaseButton _addSoftButton;
        [SerializeField] private BaseButton _continueButton;
        [SerializeField] private MaskItem[] _masks;
        [SerializeField] private CanvasGroup _canvasGroup;

        private AdModel _ad;
        private GameModel _game;
        
        public override void Init()
        {
            _addSoftButton.SetCallback(OnPressedSoft);
            _continueButton.SetCallback(Close);
            
            _ad = Models.Get<AdModel>();
            _game = Models.Get<GameModel>();
            
            _addSoftButton.SetText(GameBalance.Instance.MaskWindowSoftValue.ToString());
            
            base.Init();
        }

        public void RedrawAllMask()
        {
            foreach (var mask in _masks)
            {
                mask.Redraw();
            }
        }

        private void OnPressedSoft()
        {
            _ad.AdEvent += OnAdFinished;
            _ad.ShowAd(AdType.AddSoftMaskWindow);
            _addSoftButton.Deactivate();
        }

        private void OnAdFinished(bool success)
        {
            _ad.AdEvent -= OnAdFinished;
            if (!success) return;

            Models.Get<GameModel>().CurrentData.GetParam(GameParamType.Soft).Change(GameBalance.Instance.MaskWindowSoftValue);
            RedrawAllMask();
            _soft.text = _game.CurrentData.GetParam(GameParamType.Soft).Value.ToString();
            _hard.text = _game.CurrentData.GetParam(GameParamType.Hard).Value.ToString();
            //Close();
        }
        
        public override BaseUI Open()
        {
            var masks = GameBalance.Instance.Masks.Copy();
            for (int i = 0; i < 3; i++)
            {
                var mask = masks.RandomValue();
                _masks[i].Show(mask);
                masks.Remove(mask);
                // var mask = masks.FirstOrDefault(mask => mask.Type == GameBalance.MaskType.BoostAttackRange);
                // _masks[i].Show(mask);
            }

            _addSoftButton.Activate();
            _soft.text = _game.CurrentData.GetParam(GameParamType.Soft).Value.ToString();
            _hard.text = _game.CurrentData.GetParam(GameParamType.Hard).Value.ToString();

            //DOTween.Sequence().AppendInterval(0.2f).OnComplete(() => _canvasGroup.blocksRaycasts = true);
            _canvasGroup.blocksRaycasts = true;
            RedrawAllMask();
            
            return base.Open();
        }

        public override void Close()
        {
            _game.InputOn();
            
            base.Close();
        }
    }
}