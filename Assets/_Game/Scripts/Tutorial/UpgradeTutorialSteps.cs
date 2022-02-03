
using _Game.Scripts.Enums;
using _Game.Scripts.Model;
using _Game.Scripts.Saves;
using _Game.Scripts.Tutorial.UI;
using _Game.Scripts.UI.HUD;
using DG.Tweening;
using GeneralTools.Model;
using GeneralTools.Tools;
using GeneralTools.UI;
using Plugins.GeneralTools.Scripts.UI;
using UnityEngine;

namespace _Game.Scripts.Tutorial
{
    public class UpgradeTutorialSteps : TutorialStepsBase
    {
        protected override string TokenPrefix => "tutorial_1";
        private Transform _tempTransform;
        private Transform _upgradeParam;
        private Vector3 _worldPosition;
        private Vector3 _fingerPos;

        public UpgradeTutorialSteps()
        {
            RegisterTutorialSteps(Step0, Step1, Step2, Step3, Step4);
        }
        
        private (TutorialNextStepCondition, object) Step0()
        {
            var button = GameUI.Get<HUD>().GetGameplayElement(GamePlayElement.Inventory).GetComponent<BaseButton>();
            BaseButton.AllowedTutorialButton = button;
            TutorialFinger.ShowOnHud(button);
            _fingerPos = button.transform.position;
            //TutorialFinger.PlayAnimation(FingerAnimation.Click);
            
            return (TutorialNextStepCondition.PressedButton, button);
        }
        
        private (TutorialNextStepCondition, object) Step1()
        {
            var button = GameUI.Get<HUD>().GetGameplayElement(GamePlayElement.InventoryWeapon).GetComponent<BaseButton>();
            BaseButton.AllowedTutorialButton = button;
            //var pos = TutorialFinger.CurrentFinger.transform.position;
            DOTween.Sequence().AppendInterval(0.5f).OnComplete(() =>
            {
                TutorialFinger.ShowOnHud(button, _fingerPos);
                _fingerPos = button.transform.position;
            });
            return (TutorialNextStepCondition.PressedButton, button);
        }
        
        private (TutorialNextStepCondition, object) Step2()
        {
            var button = GameUI.Get<HUD>().GetGameplayElement(GamePlayElement.ActionButton).GetComponent<BaseButton>();
            BaseButton.AllowedTutorialButton = button;
            //var pos = TutorialFinger.CurrentFinger.transform.position;
            TutorialFinger.ShowOnHud(button, _fingerPos);
            _fingerPos = button.transform.position;
            _fingerPos = TutorialFinger.CurrentFinger.transform.position;
            return (TutorialNextStepCondition.PressedButton, button);
        }
        
        private (TutorialNextStepCondition, object) Step3()
        {
            var button = GameUI.Get<HUD>().GetGameplayElement(GamePlayElement.MapButton).GetComponent<BaseButton>();
            BaseButton.AllowedTutorialButton = button;
            //var pos = TutorialFinger.CurrentFinger.transform.position;
            TutorialFinger.ShowOnHud(button, _fingerPos);
            _fingerPos = button.transform.position;
            return (TutorialNextStepCondition.PressedButton, button);
        }
        
        private (TutorialNextStepCondition, object) Step4()
        {
            var button = GameUI.Get<HUD>().GetGameplayElement(GamePlayElement.PlayButton).GetComponent<BaseButton>();
            BaseButton.AllowedTutorialButton = button;
            //var pos = TutorialFinger.CurrentFinger.transform.position;
            TutorialFinger.ShowOnHud(button, _fingerPos);
            return (TutorialNextStepCondition.PressedButton, button);
        }
        
        public override void FinishTutorial()
        {
            TutorialFinger.RemoveFinger();
            Game.Flags.Set(GameFlag.InventoryTutorial);

            base.FinishTutorial();
            TutorialWindow.HideAll();
        
            GameSave.Save();
        }
    }
}