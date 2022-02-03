using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Balance;
using _Game.Scripts.Enums;
using _Game.Scripts.Model;
using _Game.Scripts.Model.Items;
using _Game.Scripts.Model.Unit;
using _Game.Scripts.View.Animations;
using _Game.Scripts.View.ScrollCamera;
using _Game.Scripts.View.Unit;
using DG.Tweening;
using GeneralTools;
using GeneralTools.Pooling;
using GeneralTools.Tools;
using Plugins.GeneralTools.Scripts.View;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

namespace _Game.Scripts.View
{
    public class UnitView : ViewWithModel<UnitModel>
    {
        public Collider _mainCollider;
        [SerializeField] public bool StoreVersion;
        [SerializeField] public UnitType Type;
        [ShowIf("Type", UnitType.Player)]
        [SerializeField] public Transform CameraFollow;
        [SerializeField] public bool Boss;
        [ShowIf("Type", UnitType.EnemyBase)]
        [SerializeField] public GameBalance.EnemyType EnemyType;
        [ShowIf("Type", UnitType.EnemyBase)]
        [SerializeField] public bool _patrolling;
        [ShowIf("_patrolling", true)]
        [SerializeField] public List<Transform> _patrollPoints;
        [ShowIf("Type", UnitType.EnemyBase)]
        [SerializeField] public ItemClass WeaponClass;
        [ShowIf("Type", UnitType.EnemyBase)]
        [SerializeField] public ItemType WeaponType;
        [ShowIf("Type", UnitType.EnemyBase)] [SerializeField]
        private NavMeshAgent _agent;
        [SerializeField] private Rigidbody _mainbody;
        [SerializeField] private ParticleSystem _cycle;
        [ShowIf("Type", UnitType.Player)][SerializeField] private ParticleSystem _healthEffect;
        [SerializeField] private Transform _rightHand;
        [SerializeField] private Transform _bulletPoint;
        [SerializeField] private Vector3 _healthBarOffset;
        [SerializeField] private List<Rigidbody> _allBodies;
        [SerializeField] private WeaponIK _weaponIK;
        [SerializeField] private Rig _rig;
        [SerializeField] private CapsuleCollider _capsuleCollider;
        [SerializeField] private ScrollCameraView _scrollCameraView;
        [ShowIf("Type", UnitType.EnemyBase)]
        [SerializeField] private List<GameObject> _hairs = new List<GameObject>();
        [ShowIf("Type", UnitType.Player)]
        [SerializeField] private List<MaskPreset> _maskPresets = new List<MaskPreset>();
        [SerializeField] private Material _whiteMaterial;
        [SerializeField] private List<MaterialsContainer> _materialsContainers = new List<MaterialsContainer>();
        private List<Collider> _allColliders = new List<Collider>();

        public AnimationEventsSender AnimationEventsSender;
        
        private static readonly int ANIMATION_BASE = Animator.StringToHash("Base");
        private static readonly int ANIMATION_ATTACK = Animator.StringToHash("Attack");
        private static readonly int ANIMATION_KICKDOOR = Animator.StringToHash("KickDoor");
        private static readonly int ANIMATION_THROWGRENADE = Animator.StringToHash("ThrowGrenade");
        private static readonly int ANIMATION_RELOAD = Animator.StringToHash("Reload");

        private Animator _animator;
        private BattleSettings _battle;
        private GameSounds _sounds;
        private List<CollisionListener> _collisionListeners;
        private float _blinkDuration;
        private Sequence _physicsSequence;
        private Color _deathColor;

        private ProgressBarView _progress;

        public ParticleSystem HealthEffect => _healthEffect;  
        public bool Patrolling => _patrolling;
        public Transform BulletPoint => _bulletPoint;
        public Animator Animator => _animator;
        public WeaponView Weapon { get; private set; }
        public WeaponIK WeaponIK => _weaponIK;
        public Transform RightHand => _rightHand;
        public Rigidbody Rigidbody { get; private set; }

        public bool CheckVisible()
        {
            return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes (GameCamera.UnityCam), _mainCollider.bounds);
        }

        public void OnValidate()
        {
            foreach (var variablMaterialsContainer in _materialsContainers)
            {
                if (variablMaterialsContainer.MeshRenderer != null)
                {
                    variablMaterialsContainer.Materials = variablMaterialsContainer.MeshRenderer.sharedMaterials;   
                }
            }
        }

        public override void Init()
        {
            if (_patrollPoints.Count == 0)
            {
                _patrolling = false;
            }
            _blinkDuration = GameBalance.Instance.BlinkDuration;
            _animator = GetComponentInChildren<Animator>();
            _allColliders = GetComponentsInChildren<Collider>().ToList();
            _battle = BattleSettings.Instance;
            _sounds = GameSounds.Instance;
            Rigidbody = GetComponent<Rigidbody>();
            Weapon = GetComponentInChildren<WeaponView>();
            PlayAnimation(UnitAnimationType.Base);
            _collisionListeners = new List<CollisionListener>();
            _collisionListeners = GetComponentsInChildren<CollisionListener>().ToList();
            if (_hairs.Count > 0)
            {
                _hairs.RandomValue().Activate();
            }
            
            _deathColor = GameBalance.Instance.DeathColor;

            base.Init();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public override void StartMe()
        {
            foreach (var collisionListener in _collisionListeners)
            {
                collisionListener.UnitModel = Model;
            }

            if (StoreVersion)
            {
                _scrollCameraView.StartMe();
            }
            //if(Type == UnitType.Player) ShowProgressBar();

            base.StartMe();
        }

        public void SetPosition(Vector3 position)
        {
            if (Type == UnitType.EnemyBase)
            {
                _agent.Warp(position);
            }
            else
            {
                transform.localPosition = position;   
            }
            //UpdateProgressBarPosition();
        }
        
        public void SetVelocity(Vector3 velocity)
        {
            if (velocity.x > 1)
            {
                velocity.x = 1;
            }
            
            if (velocity.y > 1)
            {
                velocity.y = 1;
            }
            Rigidbody.velocity = velocity;
        }

        public void SetLayerWeight(int id, int weight)
        {
            _animator.SetLayerWeight(id, weight);
        }

        public void SetRotation(Quaternion rotation)
        {
            transform.localRotation = rotation;
        }

        public void Fall(Vector3 impulse)
        {
            foreach (var body in _allBodies)
            {
                body.isKinematic = false;
            }

            if (Type == UnitType.EnemyBase)
            {
                _agent.enabled = false;
            }
            foreach (var bodyCollider in _allColliders)
            {
                bodyCollider.gameObject.layer = 13 << 0;
            }

            //_capsuleCollider.enabled = false;
            _animator.enabled = false;
            _mainbody.AddForce(impulse.normalized * GameBalance.Instance.PushForce, ForceMode.Impulse);
            if (Type != UnitType.Player)
            {
                _physicsSequence = DOTween.Sequence().AppendInterval(GameBalance.Instance.DisableUnitPhysicsDelay).OnComplete(() =>
                {
                    foreach (var body in _allBodies)
                    {
                        body.isKinematic = true;
                    }

                    foreach (var bodyCollider in _allColliders)
                    {
                        bodyCollider.enabled = false;
                    }
                });   
            }
        }

        public void PlayAnimation(UnitAnimationType animationType)
        {
            switch(animationType)
            {
                case UnitAnimationType.Base:
                    _animator.Play(ANIMATION_BASE);
                    break;

                case UnitAnimationType.Attack:
                    switch (Model.Weapon.Type)
                    {
                        case ItemType.FragGrenade:
                            //SetRigWeight(0, 0.1f, () => Weapon.Gun.Deactivate(),null);
                            _animator.Play(ANIMATION_THROWGRENADE);
                            break;
                        case ItemType.BaseballBat:
                        case ItemType.Baton:
                        case ItemType.Cleaver:
                        case ItemType.Crowbar:
                        case ItemType.Machete:
                        case ItemType.Katana:
                            _animator.Play(ANIMATION_ATTACK);
                            _sounds.PlaySound(Model.Weapon.SoundType);
                            break;
                        case ItemType.Gun:
                        case ItemType.AutomaticRifle:
                        case ItemType.Uzi:
                        case ItemType.Shotgun:
                        case ItemType.Magnum:
                            Weapon.Shoot();
                            break;
                    }
                    break;
                
                case UnitAnimationType.Death:
                    break;
                case UnitAnimationType.KickDoor:
                    SetRigWeight(0, GameBalance.Instance.PlayerConfig.ChangeRigDelay, () =>
                    {
                        Weapon.SetGun(false);
                        Weapon.SetFakeGun(true);
                    },null);
                    _animator.Play(ANIMATION_KICKDOOR);
                    break;
                case UnitAnimationType.Reload:
                    SetRigWeight(0, 0.1f, () =>
                    {
                        Weapon.SetFakeGun(true);
                        Weapon.SetGun(false);
                    },null);
                    _animator.Play(ANIMATION_RELOAD);
                    break;
            }
        }

        public void StopAnimation(UnitAnimationType animationType)
        {
            switch (animationType)
            {
                case UnitAnimationType.Reload:
                    // SetRigWeight(0, 0.1f, () =>
                    // {
                    //     Weapon.SetFakeGun(true);
                    //     Weapon.SetGun(false);
                    // },null);
                    _animator.Play("Hand");
                    break;
            }
        }

        public void SetRigWeight(int weight, float time, Action actionStart, Action actionEnd)
        {
            actionStart?.Invoke();
            if (time <= 0)
            {
                _rig.weight = weight;
                actionEnd?.Invoke();
                return;
            }
            if (weight > 0)
            {
                DOTween.To(x => _rig.weight = x, 0, 1, time)
                    .OnComplete(() =>
                    {
                        actionEnd?.Invoke();
                    });
            }
            else
            {
                DOTween.To(x => _rig.weight = x, 1, 0, time)
                    .OnComplete(() =>
                    {
                        actionEnd?.Invoke();
                    });
            }
        }

        public void ShowWeapon()
        {
            var settings = _battle.Weapons.FirstOrDefault(i => i.Type == Model.Weapon.Type);
            if (settings != null) Weapon.Init(Model.Weapon, settings);
            Debug.Log($"show weapon:{Model.Weapon.Type}");
            if (Type == UnitType.Player && !StoreVersion)
            {
                var attackRangeParam = Weapon.Item.GetParam(GameParamType.AttackRange);
                ShowPlayerCycle(attackRangeParam.Value);
                GameCamera.Instance.ChangeFOV(attackRangeParam.StartValue);
            }
        }

        public void ShowMask(GameBalance.MaskType maskType)
        {
            var maskPreset = _maskPresets.FirstOrDefault(mask => mask.Type == maskType);
            if (maskPreset == null)
            {
                Debug.LogError($"Can`t find mask {maskType}");
                return;
            }

            foreach (var varMaskPreset in _maskPresets)
            {
                if (varMaskPreset.Type != maskPreset.Type)
                {
                    foreach (var mask in varMaskPreset.Masks)
                    {
                        mask.Deactivate();
                    }
                }
                else
                {
                    foreach (var mask in varMaskPreset.Masks)
                    {
                        mask.Activate();
                    }
                }
            }
        }

        private void ShowPlayerCycle(float range)
        {
            _cycle.transform.localScale = new Vector3(range, range, range) * 2;
            _cycle.Play();
        }

        public void ShowEnemyCycle()
        {
            if (!_cycle.IsActivate()) _cycle.Activate();
        }
        
        public void HideEnemyCycle()
        {
            if (_cycle.IsActivate()) _cycle.Deactivate();
        }
        
        public void Blink()
        {
            //_fillSlider.DOColor(Color.white, 0.1f).OnComplete(() => _fillSlider.color = _startColor);
            foreach (var materialsContainer in _materialsContainers)
            {
                Material[] whiteMaterials = new Material[materialsContainer.Materials.Length];
                for (var i = 0; i < materialsContainer.Materials.Length; i++)
                {
                    whiteMaterials[i] = _whiteMaterial;
                }

                materialsContainer.MeshRenderer.materials = whiteMaterials;
            }
            DOTween.Sequence().AppendInterval(_blinkDuration).OnComplete(SetOriginalMaterials);
        }
        
        public void SetDieColor()
        {
            foreach (var materialsContainer in _materialsContainers)
            {
                var grayMaterials = new Material[materialsContainer.Materials.Length];
                for (var i = 0; i < materialsContainer.Materials.Length; i++)
                {
                    grayMaterials[i] = new Material(materialsContainer.Materials[i])
                    {
                        color = materialsContainer.Materials[i].color - _deathColor,
                    };
                }
                
                materialsContainer.MeshRenderer.materials = grayMaterials;
            }
        }
        
        private void SetOriginalMaterials()
        {
            foreach (var materialsContainer in _materialsContainers)
            {
                materialsContainer.MeshRenderer.materials = materialsContainer.Materials;
            }
        }

        private void ShowProgressBar()
        {
            _progress = Pool.Pop<ProgressBarView>(MainGame.WorldSpaceCanvas, false);
            _progress.Init(Type, EnemyType);
            _progress.transform.position = transform.position + _healthBarOffset;
        }

        public void UpdateProgressBarPos()
        {
            if(_progress == null) return;
            var newPos = transform.position;
            newPos.y = _progress.transform.position.y;
            _progress.transform.position = newPos;
        }

        public void UpdateProgressBar(double value, float progress)
        {
            if (_progress == null) ShowProgressBar();
            _progress.UpdateUI(value, progress);
        }

        // private void UpdateProgressBarPosition()
        // {
        //     if (_progress == null) return; 
        //     _progress.transform.position = transform.position + _uiOffset;
        // }

        public void HideProgressBar()
        {
            if (_progress == null) return;
            _progress.PushToPool();
            _progress = null;
        }

        public override void Delete()
        {
            Destroy(gameObject);
        }

        public void KillTweens()
        {
            _physicsSequence.Kill();
        }
    }

    [Serializable]
    public class MaskPreset
    {
        public GameBalance.MaskType Type;
        public GameObject[] Masks;
    }
    
    public enum UnitAnimationType
    {
        Base,
        Attack,
        Death,
        KickDoor,
        ThrowGrenade,
        Reload,
    }

    public enum UnitBehaviour
    {
        Stay,
        Patrolling
    }
    
    [Serializable]
    public class MaterialsContainer
    {
        public Material[] Materials;
        public SkinnedMeshRenderer MeshRenderer;
    }
}