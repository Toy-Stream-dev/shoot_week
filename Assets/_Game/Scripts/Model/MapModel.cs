using _Game.Scripts.View;
using GeneralTools.Model;
using GeneralTools.Tools;
using UnityEngine;

namespace _Game.Scripts.Model
{
    public class MapModel : ModelWithView<MapView>
    {
        public override BaseModel Init()
        {
            return this;
        }

        public void InitView(int id)
        {
            if (DevFlags.DEV_TOOLS)
            {
                var map = MainGame.Root.GetComponentInChildren<MapView>();
                SetView(map).View.Init(MainGame.Root);
            }
            else
            {
                var map = Resources.Load<MapView>($"Maps/loc-{id}").Copy();
                SetView(map).View.Init(MainGame.Root);
            }
        }
        
        public void OnLoad()
        {
        }
        
        public override void Update(float deltaTime)
        {
            if (View == null) return;
            View.UpdateMe(deltaTime);
            base.Update(deltaTime);
        }

        public void Destroy()
        {
            View.gameObject.Destroy();
        }
    }
}