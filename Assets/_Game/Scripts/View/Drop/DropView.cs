using System;
using _Game.Scripts.Balance;
using _Game.Scripts.Model.Drop;
using _Game.Scripts.Model.Items;
using _Game.Scripts.Model.Unit;
using Plugins.GeneralTools.Scripts.View;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Scripts.View.Drop
{
    public class DropView : ViewWithModel<DropModel>
    {
        [SerializeField] private bool _pickable;
        [SerializeField] private ItemClass _class;
        [ShowIf("_class", ItemClass.Mask)][SerializeField] private GameBalance.MaskType _maskType;
        [HideIf("_class", ItemClass.Mask)][SerializeField] private ItemType _itemType;
        [SerializeField] private Animation _animation;
        //[ShowIf("_pickable")][SerializeField] private ParticleSystem _pickUpEffect;

        public GameBalance.MaskType MaskType => _maskType;
        public ItemClass Class => _class;
        public ItemType ItemType => _itemType;

        public void OnJump()
        {
            switch (Class)
            {
                
            }
        }

        public void OnLie()
        {
            switch (Class)
            {
                case ItemClass.Value:
                    _animation.Play("Shake");
                    break;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == 10)
            {
                var unit = other.gameObject.GetComponent<UnitView>();
                if (unit != null && unit.Type == UnitType.Player && unit.Model.State != UnitModel.UnitState.Death)
                {
                    Model.Execute();
                }
            }
        }
    }
}