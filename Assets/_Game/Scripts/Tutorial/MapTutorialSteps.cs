
using GeneralTools.Tools;
using GeneralTools.UI;
using UnityEngine;

namespace _Game.Scripts.Tutorial
{
    public class MapTutorialSteps : TutorialStepsBase
    {
        protected override string TokenPrefix => "tutorial_2";
        
        // public MapTutorialSteps()
        // {
        //     RegisterTutorialSteps(Step0, Step1);
        // }
        //
        // private (TutorialNextStepCondition, object) Step0()
        // {
        //     var button = Game.HUD.MapButton;
        //     Game.Hud.MoveToOverlay(GamePlayElement.MapButton);
        //     GameUI.Get<TutorialWindow>().TutorialPanel.Activate();
        //     BaseButton.AllowedTutorialButton = button;
        //     TutorialFinger.ShowOnHud(button);
        //     
        //     return (TutorialNextStepCondition.PressedButton, button);
        // }
        //
        // private (TutorialNextStepCondition, object) Step1()
        // {
        //      GameUI.Get<TutorialWindow>().TutorialPanel.Deactivate();
        //      var button = GameUI.Get<MapWindow>().GetNextLevelButton();
        //      Game.Hud.ReturnFromOverlay(GamePlayElement.MapButton);
        //      BaseButton.AllowedTutorialButton = button;
        //      TutorialFinger.ShowOnHud(button,new Vector2(-50, 40));
        //      return (TutorialNextStepCondition.PressedButton, button);
        // }
        //
        // public override void FinishTutorial()
        // {
        //     Game.Flags.Set(GameFlag.FirstMapAccumulated);
        //     
        //     base.FinishTutorial();
        //     TutorialWindow.HideAll();
        //
        //     GameSave.Save();
        // }
    }
}