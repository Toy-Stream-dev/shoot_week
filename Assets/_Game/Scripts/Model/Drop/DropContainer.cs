using _Game.Scripts.Model.Base;
using _Game.Scripts.Model.Items;
using _Game.Scripts.View.Drop;
using GeneralTools.Model;
using GeneralTools.Pooling;

namespace _Game.Scripts.Model.Drop
{
    public class DropContainer : ModelsContainer<DropModel>
    {
        public override BaseModel Init()
        {
            Pool.Spawn<DropView>(40, drop => drop.ItemType == ItemType.Soft);
            Pool.Spawn<DropView>(10, drop => drop.ItemType == ItemType.SmallHealth);
            
            return base.Init();
        }

        public DropModel SpawnDrop(ItemClass itemClass, ItemType itemType = default)
        {
            var dropModel = Create()
                .SpawnView(MainGame.DropContainer, true, drop => drop.ItemType == itemType)
                .Start();

            return dropModel;
        }

        public void CollectAll()
        {
            foreach (var drop in All)
            {
                drop.Collect();
            }
        }

        public void Destroy(DropModel dropModel)
        {
            dropModel.Destroy();
            All.Remove(dropModel);
        }

        public void KillTweenAll()
        {
            foreach (var drop in All)
            {
                drop.KillTween();
            }
        }
        
        public void DestroyAll()
        {
            foreach (var drop in All)
            {
                drop.Destroy();
            }
            All.Clear();
        }
    }
}