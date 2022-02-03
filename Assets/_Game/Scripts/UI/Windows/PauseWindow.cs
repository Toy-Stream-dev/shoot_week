using _Game.Scripts.Model;
using GeneralTools.Model;
using GeneralTools.UI;
using Plugins.GeneralTools.Scripts.UI;
using UnityEngine;

namespace _Game.Scripts.UI.Windows
{
    public class PauseWindow : BaseWindow
    {
        [SerializeField] private BaseButton _exit;
        [SerializeField] private BaseButton _resume;

        public override void Init()
        {
            _resume.SetCallback(OnPressedResume);
            _exit.SetCallback(OnPressedExit);
            base.Init();
        }

        private void OnPressedResume()
        {
            Cheats.PauseGame();
            Close();
        }

        private void OnPressedExit()
        {
            Models.Get<GameModel>().RestartLevel();
            Cheats.PauseGame();
            Close();
        }
    }
}