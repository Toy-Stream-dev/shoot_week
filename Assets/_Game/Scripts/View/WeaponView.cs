using _Game.Scripts.Enums;
using _Game.Scripts.Model.Items;
using _Game.Scripts.View.Animations;
using GeneralTools;
using GeneralTools.Tools;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Scripts.View
{
    public class WeaponView : BaseBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _weaponPoint;
        [SerializeField] private Transform _gun;
        [SerializeField] private Transform _fakeGun;
        [SerializeField] private MeshFilter _fakeMeshFilter;
        [SerializeField] public Transform Point;
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private Transform _rightHand;
        [SerializeField] private Transform _leftHand;
        [SerializeField] private AnimationEventsSender _animationEventsSender;
        [SerializeField] private Muzzle _muzzle;
        private GameSounds _sounds;

        private static readonly int ANIMATION_SHOT = Animator.StringToHash("Shot");
        private static readonly int ANIMATION_WEAPON = Animator.StringToHash("Weapon");
        
        public ItemData Item { get; private set; }
        public AnimationEventsSender AnimationEventsSender => _animationEventsSender;
        // public Transform FakeGun => _fakeGun;
        // public Transform Gun => _gun;

        public void Shoot()
        {
            // Item.GetParam(GameParamType.Ammunition).Change(-1);
            _animator.Play(ANIMATION_SHOT);
            _muzzle.Play(Item.Type);
            _sounds.PlaySound(Item.SoundType);
        }

        // public bool HasAmmo()
        // {
        //     return Item.GetParam(GameParamType.MaxAmmunition).Value > 0;
        // }

        public void SetFakeGun(bool value)
        {
            _fakeGun.SetActive(value);
        }

        public void SetGun(bool value)
        {
            _gun.SetActive(value);
        }
        
        public void Init(ItemData item, BattleSettings.WeaponConfig config)
        {
            _sounds = GameSounds.Instance;
            Item = item;
            _animator.SetFloat(ANIMATION_WEAPON, Item.GetParam(GameParamType.AnimationWeaponID).Value);
            _weaponPoint.localPosition = config.Position;
            _weaponPoint.localRotation = Quaternion.Euler(config.Rotation);
            _weaponPoint.localScale = config.Scale;

            _fakeGun.localScale = config.Scale;
            
            _meshFilter.sharedMesh = config.Mesh;
            _fakeMeshFilter.sharedMesh = config.Mesh;
            _fakeGun.Deactivate();
            
            _rightHand.localPosition = config.RightHandPosition;
            _rightHand.localRotation = Quaternion.Euler(config.RightHandRotation);
            _leftHand.localPosition = config.LeftHandPosition;
            _leftHand.localRotation = Quaternion.Euler(config.LeftHandRotation);

            Point.transform.localPosition = config.ProjectilePosition;
            //this.Deactivate();
        }
    }
}