using GeneralTools.Tools;
using GeneralTools.UI;
using UnityEngine;

namespace _Game.Scripts
{
    public class Cheats
    {
        private static bool IsKeyDown(KeyCode keyCode) => Input.GetKeyDown(keyCode);
        
        public static void Update()
        {
#if !UNITY_EDITOR
    
#endif
            Pause();
            
            if (Input.GetKeyDown(KeyCode.F11)) CheckTimeScale(3f);
            if (Input.GetKeyDown(KeyCode.F12)) CheckTimeScale(10f);
            if (Input.GetKeyDown(KeyCode.F1)) GameUI.Root.Deactivate();
            if (Input.GetKeyDown(KeyCode.F2)) GameUI.Root.Activate();
            
            void CheckTimeScale(float value) => Time.timeScale = Time.timeScale > 1f ? 1f : value;
        }

        private static void Pause()
        {
            if (!IsKeyDown(KeyCode.P)) return;
            Time.timeScale = Time.timeScale >= 1 ? 0 : 1;
        }
        
        public static void PauseGame()
        {
            Time.timeScale = Time.timeScale >= 1 ? 0 : 1;
        }
    }
}