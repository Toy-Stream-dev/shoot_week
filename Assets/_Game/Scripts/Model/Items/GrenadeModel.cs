using System;
using _Game.Scripts.Enums;
using _Game.Scripts.View;
using _Game.Scripts.View.Items;
using _Game.Scripts.View.Unit;
using DG.Tweening;
using GeneralTools.Model;
using UnityEngine;
using Random = GeneralTools.Tools.Random;

namespace _Game.Scripts.Model.Items
{
    public enum GrenadeState
    {
        None,
        Fly,
        Explosion,
        Stop,
    }
    
    public class GrenadeModel : ModelWithView<GrenadeView>
    {
        private GrenadeState _state;
        private ItemData _itemData;
        
        public GrenadeModel(ItemData itemData)
        {
            _itemData = itemData;
            SpawnView(MainGame.ProjectilesContainer, true, item => item.ItemType == _itemData.Type);
        }

        private new GrenadeModel SpawnView(Transform root, bool activate = true, Predicate<GrenadeView> predicate = default)
        {
            base.SpawnView(root, activate, predicate);
            return this;
        }

        public void Throw(Vector3 target, float jumpPower, float duration)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(View.transform.DOJump(target, jumpPower, 1, duration).SetEase(Ease.InSine).OnComplete(() =>
            {
                //View.Rigidbody.isKinematic = false;
                DOTween.Sequence().AppendInterval(_itemData.GetParam(GameParamType.ExplosionDelay).Value)
                    .OnComplete(() => SetState(GrenadeState.Explosion));
            }));
            sequence.Join(View.Body.DORotate(new Vector3(Random.Range(0, 360), Random.Range(0, 360),
                Random.Range(0, 360)), duration));
        }

        private void Explosion()
        {
            GameSounds.Instance.PlaySound(GameSoundType.Grenade);
            var enemies = Models.Get<GameModel>().EnemyContainer.GetAllAlive();
            foreach (var enemy in enemies)
            {
                if (Vector3.Distance(enemy.View.transform.position, View.transform.position) <=
                    _itemData.GetParam(GameParamType.ExplosionRadius).Value)
                {
                    enemy.Hit(_itemData.GetParam(GameParamType.Dmg).Value);
                }
            }
            //View.SetExplosionRadius(_itemData.GetParam(GameParamType.ExplosionRadius).Value);
            View.PlayExplosion(_itemData.SoundType);
        }

        public void OnTriggerEnter(Collider other)
        {
            if(_state != GrenadeState.Explosion) return;
            if (other.gameObject.layer == 8)
            {
                var listener = other.gameObject.GetComponent<CollisionListener>();
                if (listener != null)
                {
                    listener.UnitModel.Hit(_itemData.GetParam(GameParamType.Dmg).Value);
                }
            }
        }

        public void SetPosition(Vector3 position)
        {
            View.transform.position = position;
        }

        public void SetState(GrenadeState state)
        {
            if(_state == state) return;
            _state = state;
            
            switch (_state)
            {
                case GrenadeState.Fly:
                    break;
                case GrenadeState.Explosion:
                    Explosion();
                    DOTween.Sequence().AppendInterval(1f).OnComplete(() => SetState(GrenadeState.Stop));
                    break;
                case GrenadeState.Stop:
                    Destroy();
                    break;
            }
        }

        public void Destroy()
        {
            View.Clear();
            DestroyView();
        }
    }
}