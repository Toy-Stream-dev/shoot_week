using System;
using _Game.Scripts.Balance;
using _Game.Scripts.Model.Items;
using Plugins.GeneralTools.Scripts.View;
using UnityEngine;

namespace _Game.Scripts.View.Items
{
    public class GrenadeView : ViewWithModel<GrenadeModel>
    {
        [SerializeField] private ItemType _itemType;
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Rigidbody _rigidbody;

        public Transform Body => _meshRenderer.transform;
        public Rigidbody Rigidbody => _rigidbody;
            // [SerializeField] private CapsuleCollider _capsuleCollider;

        public ItemType ItemType => _itemType;

        public void PlayExplosion(GameSoundType soundType)
        {
            _meshRenderer.enabled = false;
            _particleSystem.Play();
            GameSounds.Instance.PlaySound(soundType);
        }

        // public void SetExplosionRadius(float radius)
        // {
        //     _capsuleCollider.radius = radius;
        //     _capsuleCollider.enabled = true;
        // }

        private void OnTriggerEnter(Collider other)
        {
            Model.OnTriggerEnter(other);
        }

        public override void Clear()
        {
            _meshRenderer.enabled = true;
            //_rigidbody.isKinematic = true;
            // _capsuleCollider.radius = 0;
            // _capsuleCollider.enabled = false;
            _particleSystem.transform.localScale = Vector3.one;
            
            base.Clear();
        }
    }
}