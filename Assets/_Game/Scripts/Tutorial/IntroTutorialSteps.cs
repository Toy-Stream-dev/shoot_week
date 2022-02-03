using _Game.Scripts.Model;
using _Game.Scripts.Saves;
using GeneralTools.UI;

namespace _Game.Scripts.Tutorial
{
    public class IntroTutorialSteps : TutorialStepsBase
    {
        protected override string TokenPrefix => "tutorial_0";
	
        public IntroTutorialSteps()
        {
            RegisterTutorialSteps(Step0);
        }
        
        private (TutorialNextStepCondition, object) Step0()
        {
            Game.InputOff();
            TutorialWindow.ShowShotTutor();
            TutorialWindow.SetTouchBlockerState(true);
        
            var button = TutorialWindow.Continue;
            BaseButton.AllowedTutorialButton = button;
            
            return (TutorialNextStepCondition.PressedButton, button);
        }
        
        public override void FinishTutorial()
        {
            base.FinishTutorial();
            Game.Flags.Set(GameFlag.ShotTutorial);
            TutorialWindow.HideAll();
            Game.InputOn();
        
            GameSave.Save();
        }
    }
}