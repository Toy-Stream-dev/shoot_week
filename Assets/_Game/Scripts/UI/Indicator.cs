using GeneralTools.UI;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI
{
    public class Indicator : BaseUIBehaviour
    {
        public Image Image;

        [HideInInspector]
        public Transform Target;
        [HideInInspector]
        public bool IsOnScreen;
        
        public bool RotateOnScreen;
        public bool RotateOffScreen;

        public bool Hiden;


        public void Show()
        {
            Hiden = false;
            Image.enabled = true;
        }

        public void Hide()
        {
            Hiden = true;
            Image.enabled = false;
        }
    }
}