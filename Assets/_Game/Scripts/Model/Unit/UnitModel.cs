using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Balance;
using _Game.Scripts.Enums;
using _Game.Scripts.Model.Drop;
using _Game.Scripts.Model.Items;
using _Game.Scripts.Model.Numbers;
using _Game.Scripts.Model.Unit.Components;
using _Game.Scripts.UI;
using _Game.Scripts.UI.Windows.MainGame;
using _Game.Scripts.View;
using _Game.Scripts.View.Animations;
using _Idle.Scripts.UI.HUD;
using DG.Tweening;
using GeneralTools.Model;
using GeneralTools.Tools;
using GeneralTools.UI;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Random = GeneralTools.Tools.Random;

namespace _Game.Scripts.Model.Unit
{
    public class UnitModel : ModelWithView<UnitView>, IAnimationEventsListener
    {
        public enum UnitState
        {
            Idle,
            Moving,
            Death
        }

        private readonly List<GameParam> _params = new List<GameParam>();

        private int _id;
        public UnitType Type { get; private set; }
        public GameBalance.EnemyType EnemyType { get; private set; }
        public UnitState State { get; private set; }
        public ItemData Weapon { get; private set; }
        
        public UnitModel Target { get; private set; }
        public float TargetDist { get; private set; }
        
        public Vector3 Position { get; private set; }
        public Quaternion Rotation;

        private GameParam _attackRange;
        private GameParam _supportRange;
        private GameParam _speed;
        
        private float _hindWalkPercent;
        
        private GameModel _game;
        private PopupValueContainer _popupValueContainer;
        private BattleSettings _battle;
        private AttackComponent _attackComponent;
        private GameBalance _balance;
        private List<LocalBoost> _localBoosts = new List<LocalBoost>();

        private float _speedDelta;
        private Vector3 _lastPosition;
        private Vector3 _lastDisFor;
        
        private Vector2 _directionClamped;
        private Vector3 _currentSpeed;
        private Vector3 _rotation;
        private Door _currentDoor;

        private GameBalance.EnemyConfig _enemyConfig;
        private GameBalance.PlayerParamsConfig _playerConfig;
        private double _hp;
        private bool _blockMovement;

        private GameProgress _maskTimer;
        private LocalBoost _currentMaskBoost;
        private LocalBoost _patrolBoost;
        private GameBalance.MaskType _currentMaskType;
        private MainGameWindow _mainGameWindow;

        private Indicator _indicator;
        private IndicatorWindow _indicatorWindow;

        private static readonly int ANIMATION_MOVESPEED = Animator.StringToHash("MoveSpeed");
        private static readonly int ANIMATION_WEAPON = Animator.StringToHash("Weapon");
        private static readonly int ANIMATION_DIRECTION = Animator.StringToHash("Direction");


        public List<ItemData> DropedItems = new List<ItemData>();
        public GameBalance.EnemyConfig EnemyConfig => _enemyConfig;
        public GameBalance.PlayerParamsConfig PlayerConfig => _playerConfig;
        public event Action<UnitModel> OnDie;
        public event Action<float> OnHpChange;

        public Vector3 DeathPoint;

        public UnitModel Init(UnitType type, int id)
        {
            _game = Models.Get<GameModel>();
            _battle = BattleSettings.Instance;
            _balance = GameBalance.Instance;
            _popupValueContainer = GameUI.Get<PopupValueContainer>();
            _indicatorWindow = GameUI.Get<IndicatorWindow>();
            _mainGameWindow = GameUI.Get<MainGameWindow>();
            
            Type = type;
            _id = id;
            
            if (!View.StoreVersion)
            {
                CreateParameters();
                CreateComponents();
            }

            Position = View.transform.localPosition;
            Rotation = View.transform.localRotation;

            _maskTimer = new GameProgress(GameParamType.Timer, 10, false);
            _maskTimer.Pause();
            _maskTimer.CompletedEvent += OnMaskEnded;

            if (View.Patrolling)
            {
                _patrolBoost = new LocalBoost(_speed, 0.5f, -1, true);
                _patrolBoost.OnEnd += OnBoostEnd;
                _localBoosts.Add(_patrolBoost);
            }

            return this;
        }

        private void CreateParameters()
        {
            switch (Type)
            {
                case UnitType.Player:
                    _playerConfig = GameBalance.Instance.PlayerConfig;
                    _speed = CreateParam(GameParamType.Speed, _playerConfig.Speed);

                    _hindWalkPercent = _playerConfig.HindWalkSpeedPercent;
                    var hp = CreateParam(GameParamType.MaxHp, _playerConfig.Hp);

                    _hp = hp.Value;
                    break;
                
                case UnitType.EnemyBase:
                    EnemyType = View.EnemyType;
                    _enemyConfig = GameBalance.Instance.EnemiesConfig.FirstOrDefault(e => e.Type == EnemyType);
                    if (_enemyConfig != null)
                    {
                        var enemyHpProgression =
                            _balance.Progressions.FirstOrDefault(progress => progress.Type == GameParamType.EnemyHp);
                        float hpCount = 0;
                        if (enemyHpProgression != null)
                        {
                            hpCount = (float)enemyHpProgression.GetValue(_game.CurrentData.Region, _enemyConfig.Hp);
                        }
                        var enemyHp = CreateParam(GameParamType.MaxHp, hpCount);
                        if (View.WeaponClass == ItemClass.MeleeWeapon)
                        {
                            _speed = CreateParam(GameParamType.Speed, _enemyConfig.Speed * _balance.IncreaseSpeedForMelee);
                        }
                        else
                        {
                            _speed = CreateParam(GameParamType.Speed, _enemyConfig.Speed);
                        }
                        _attackRange = CreateParam(GameParamType.AttackRange, _enemyConfig.AttackRange);
                        _supportRange = CreateParam(GameParamType.SupportRange, _enemyConfig.SupportRange);
                        
                        _hp = enemyHp.Value;
                    }
                    break;
            }
        }
        
        private void CreateComponents()
        {
            _attackComponent = AddComponent(new AttackComponent(this));
            
            switch (Type)
            {
                case UnitType.Player:
                    AddComponent(new PlayerMovementComponent(this));
                    break;
                
                case UnitType.EnemyBase:
                    AddComponent(new MovementComponent(this));
                    View.HideEnemyCycle();
                    break;
            }
        }

        private GameParam CreateParam(GameParamType type, float value = 0)
        {
            var param = GetParam(type, value);
            return param;
        }
        
        public GameParam GetParam(GameParamType type, float value = 0, bool createIfNotExists = true)
        {
            var param = _params.Find(p => p.Type == type);
            if (param != null || !createIfNotExists) return param;
            param = new GameParam(type, value);
            _params.Add(param);
            return param;
        }
        
        public new UnitModel Start()
        {
            View.AnimationEventsSender.AssignListener(this);
            View.Weapon.AnimationEventsSender.AssignListener(this); 
            View.StartMe();

            if (Type != UnitType.Player)
            {
                var itemModel = Models.Get<ItemsModel>();
                var itemData = itemModel.AddEnemyWeapon(View.WeaponClass, View.WeaponType);
                EquipWeapon(itemData);
                Target = _game.Player;
            }
            else if (!View.StoreVersion)
            {
                View.UpdateProgressBar(_hp, (float)(_hp / GetParam(GameParamType.MaxHp).Value));
            }

            return this;
        }


        public void InitIndicator()
        {
            _indicator = _indicatorWindow.AddIndicator(View.transform);
            _indicator.Hide();
        }

        public override void Update(float deltaTime)
        {
            _maskTimer.Change(deltaTime);
            
            if(View.StoreVersion) return;
            if (State == UnitState.Death) return;

            CalculateTargetDistance();
            View.UpdateProgressBarPos();
            AnimateRunning(deltaTime);
            if (Type != UnitType.Player && _indicator != null)
            {
                if (_indicator.Hiden && TargetDist <= Target.PlayerConfig.DrawArrowRange && !View.CheckVisible())
                {
                    _indicator.Show();
                }else if (!_indicator.Hiden && View.CheckVisible())
                {
                    _indicator.Hide();
                }
            }
            
            base.Update(deltaTime);
        }

        public void CalculateTargetDistance()
        {
            TargetDist = Target == null ? -1 : Vector3.Distance(View.transform.position, Target.View.transform.position);
        }

        public new UnitModel SpawnView(Transform root, bool activate = true, Predicate<UnitView> predicate = default)
        {
            base.SpawnView(root, activate, predicate);
            if (View.StoreVersion)
            {
                
            }
            return this;
        }
        
        public new UnitModel SetView(UnitView view)
        {
            base.SetView(view);
            View.Init();
            return this;
        }

        public void SetPosition(Vector3 position)
        {
            _blockMovement = false;
            if (!View.StoreVersion)
            {
                _attackComponent.EndReload();
            }
            View.transform.position = position;
        }

        public void Move(Vector2 direction)
        {
            if (direction == Vector2.zero)
            {
                View.SetVelocity(Vector3.zero);
                return;
            }
            if(_blockMovement) return;
            if (direction.magnitude > 2f)
            {
                _directionClamped = direction.normalized * 2f;
            }
            else
            {
                _directionClamped = direction;
            }
            _currentSpeed.x = _directionClamped.x;
            _currentSpeed.x = -Mathf.Clamp(_currentSpeed.x, -2f, 2f);
            _currentSpeed.z = _directionClamped.y;
            _currentSpeed.z = -Mathf.Clamp(_currentSpeed.z, -2f, 2f);
            _currentSpeed.y = View.Rigidbody.velocity.y;
            if (!HasTarget())
            {
                Rotate();   
            }
            
            if (Math.Sign(_currentSpeed.x) == Math.Sign(View.transform.forward.x) &&
                Math.Sign(_currentSpeed.z) == Math.Sign(View.transform.forward.z))
            {
                View.Animator.SetFloat(ANIMATION_DIRECTION, 1);
                _speed.SetValue(_speed.StartValue);
            }
            else
            {
                View.Animator.SetFloat(ANIMATION_DIRECTION, -1);
                _speed.SetValue(_speed.StartValue * _hindWalkPercent);
            }
            var forwardedVelocity = _currentSpeed * _speed.Value;
            forwardedVelocity.y = View.Rigidbody.velocity.y;
            View.Rigidbody.velocity = forwardedVelocity;
        }
        
        private void Rotate()
        {
            if (_currentSpeed.magnitude > 0.01f)
            {
                _rotation = Quaternion.LookRotation(_currentSpeed).eulerAngles;
                _rotation.x = 0f;
                _rotation.z = 0f;
                View.transform.rotation = Quaternion.Euler(_rotation);
            }
        }
        
        private void AnimateRunning(float deltaTime)
        {
            var position = View.transform.position;
            _speedDelta = (position - _lastPosition).magnitude;
            switch (Type)
            {
                case UnitType.Player:
                    if (View.Rigidbody.velocity == Vector3.zero)
                    {
                        View.Animator.SetFloat(ANIMATION_DIRECTION, 0);
                        View.Animator.SetFloat(ANIMATION_MOVESPEED, 0);
                    }
                    else
                    {
                        View.Animator.SetFloat(ANIMATION_MOVESPEED, View.Rigidbody.velocity.magnitude);
                    }
                    break;
                case UnitType.EnemyBase:
                    if (_speedDelta == 0)
                    {
                        View.Animator.SetFloat(ANIMATION_DIRECTION, 0);
                        View.Animator.SetFloat(ANIMATION_MOVESPEED, 0);
                    }
                    else
                    {
                        View.Animator.SetFloat(ANIMATION_MOVESPEED, _speed.Value);
                        View.Animator.SetFloat(ANIMATION_DIRECTION, 1);
                    }
                    break;
            }
            _lastPosition = position;
        }

        public void Rotate(Quaternion rotation)
        {
            Rotation = rotation;
            View.SetRotation(Rotation);
        }

        public void SetState(UnitState state)
        {
            if (state == State) return;
            
            State = state;
            switch (state)
            {
                case UnitState.Idle:
                    break;
                
                case UnitState.Moving:
                    break;
                
                case UnitState.Death:
                    View.WeaponIK.HasTarget = false;
                    View.SetRigWeight(0,0, () =>
                    {
                        View.Weapon.SetGun(false);
                        View.Weapon.SetFakeGun(true);
                    },null);
                    View.Fall(-View.transform.forward * _battle.HitOffset);
                    var dropContainer = Models.Get<DropContainer>();
                    if (Type != UnitType.Player)
                    {
                        GetComponent<MovementComponent>().KillTween();
                        // var moneyProgression = GameBalance.Instance.Progressions.FirstOrDefault(progression =>
                        //     progression.Type == GameParamType.SoftPerPack);
                        // var expProgression = GameBalance.Instance.Progressions.FirstOrDefault(progression => 
                        //     progression.Type == GameParamType.EnemyExperience);
                        // if (expProgression != null)
                        // {
                        //     _game.IncomeLoot.Push(LootType.Experience, (int)expProgression.GetValue(_game.CurrentData.Region, _enemyConfig.Experience));
                        // }
                        _game.IncomeLoot.Push(LootType.Experience, _enemyConfig.Experience);
                        var moneyDropCount = GameBalance.Instance.MoneyDropCount;
                        int softPerPack = _enemyConfig.SoftPerPack;
                        // if (moneyProgression != null)
                        // {
                        //     softPerPack = (int)moneyProgression.GetValue(_game.CurrentData.Region);
                        // }
                        // else
                        // {
                        //     Debug.LogError("Can`t find SoftPerPack progression");
                        // }
                        for (var i = 0; i < moneyDropCount; i++)
                        {
                            var drop = dropContainer.SpawnDrop(ItemClass.Value, ItemType.Soft);
                            drop.Count = softPerPack;
                            drop.Jump(View.transform.position);
                        }
                        var rndHealth = Random.Range(0, 100);
                        if (rndHealth <= _enemyConfig.DropHealthChance)
                        {
                            var drop = dropContainer.SpawnDrop(ItemClass.MedPack, ItemType.SmallHealth);
                            drop.Jump(View.transform.position);
                        }
                        _indicator.Hide();
                    }
                    else
                    {
                        DeathPoint = View.transform.position;
                    }
                    OnDie?.Invoke(this);
                    //RemoveAllComponents();
                    break;
            }
        }

        public void ExecuteEvent(AnimationEventType eventType)
        {
            switch (eventType)
            {
                case AnimationEventType.Shoot:
                    if (Type == UnitType.Player)
                    {
                        GameCamera.Instance.ShakeCamera(GameBalance.Instance.ShakeIntensity, GameBalance.Instance.ShakeDuration);   
                    }

                    if (Weapon.Class != ItemClass.Weapon)
                    {
                        return;
                    }
                    Weapon.Ammunition.Change(-1);
                    if (Weapon.Type == ItemType.Shotgun)
                    {
                        var lookRotation = Quaternion.LookRotation(Target.View.BulletPoint.transform.position - View.Weapon.Point.transform.position, Vector3.up);
                        for (var i = 0; i < Weapon.BulletPerShot; i++)
                        {
                            var rotation = lookRotation;
                            var newRotation = new Quaternion(
                                rotation.x + Random.Range(Weapon.MinScatter, Weapon.MaxScatter),
                                rotation.y + Random.Range(Weapon.MinScatter, Weapon.MaxScatter),
                                rotation.z + Random.Range(Weapon.MinScatter, Weapon.MaxScatter), rotation.w);
                            var projectile = _game.SpawnProjectile(Weapon.Type, View.Weapon.Point.position, newRotation, Weapon.GetParam(GameParamType.Dmg).Value, this);
                            projectile.OnDestroy += OnDestroyProjectile;
                        }
                        if (Weapon.GetParam(GameParamType.Ammunition).Value <= 0)
                        {
                            var newWeapon = CheckAvailableWeapon();
                            if (Type == UnitType.Player && newWeapon != null)
                            {
                                EquipWeapon(newWeapon);
                                _mainGameWindow.OnEquipWeapon(newWeapon);
                            }
                            else
                            {
                                StartReload();   
                            }
                        }   
                    }
                    else
                    {
                        var rotation = Quaternion.LookRotation(Target.View.BulletPoint.transform.position - View.Weapon.Point.transform.position, Vector3.up);
                        var projectile = _game.SpawnProjectile(Weapon.Type, View.Weapon.Point.position, rotation, Weapon.GetParam(GameParamType.Dmg).Value, this);
                        projectile.OnDestroy += OnDestroyProjectile;
                        if (Weapon.GetParam(GameParamType.Ammunition).Value <= 0)
                        {
                            var newWeapon = CheckAvailableWeapon();
                            if (Type == UnitType.Player && newWeapon != null)
                            {
                                EquipWeapon(newWeapon);
                                _mainGameWindow.OnEquipWeapon(newWeapon);
                            }
                            else
                            {
                                StartReload();   
                            }
                        }   
                    }
                    break;
                case AnimationEventType.DoorKick:
                    _currentDoor.Open();
                    _currentDoor = null;
                    //_blockMovement = false;
                    break;
                case AnimationEventType.ThrowGrenade:
                    var grenade = new GrenadeModel(Weapon);
                    grenade.SetPosition(View.RightHand.position);
                    grenade.Throw(Target.View.transform.position, 1, 0.8f);
                    DropedItems.Add(Weapon);
                    _game.RemoveWeapon(Weapon);
                    var weapon = _game.ChosenWeapons.First(item => item.Class == ItemClass.Weapon);
                    EquipWeapon(weapon);
                    _mainGameWindow.OnEquipWeapon(weapon);
                    break;
                case AnimationEventType.Reload:
                    if (Type == UnitType.Player)
                    {
                        _mainGameWindow.GetGameItem(Weapon).SetReload(false);   
                    }
                    ReloadWeapon();
                    _attackComponent.EndReload();
                    break;
                case AnimationEventType.AnimationEnd:
                    View.SetRigWeight(1, 0.1f, null, () =>
                    {
                        View.Weapon.SetGun(true);
                        View.Weapon.SetFakeGun(false);
                    });
                    break;
                case AnimationEventType.MeleeAttack:
                    if (HasTarget())
                    {
                        Target.Hit(Weapon.GetParam(GameParamType.Dmg).Value);
                    }
                    break;
            }
        }

        private ItemData CheckAvailableWeapon()
        {
            foreach (var weapon in _game.ChosenWeapons)
            {
                if (weapon.Class == ItemClass.Weapon && weapon.Ammunition.Value > 0)
                {
                    return weapon;
                }
            }

            return null;
        }
        
        private void StartReload()
        {
            if (Type == UnitType.Player)
            {
                _mainGameWindow.GetGameItem(Weapon).SetReload(true);   
            }
            _attackComponent.StartReload();
            View.PlayAnimation(UnitAnimationType.Reload);
        }
        
        private void ReloadWeapon()
        {
            Weapon.ResetAmmunition();
        }

        public void LookAt(Vector3 target, float time)
        {
            var lookPos = target - View.transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            View.transform.DORotate((rotation.eulerAngles), time);
        }

        public void KickDoor(Door door, Vector3 target)
        {
            _blockMovement = true;
            _currentDoor = door;
            LookAt(target, 0.2f);
            View.PlayAnimation(UnitAnimationType.KickDoor);
        }

        private void OnDestroyProjectile(ProjectileView projectile)
        {
            _game.RemoveProjectile(projectile);
            projectile.OnDestroy -= OnDestroyProjectile;
        }

        public void SetTarget()
        {
            if (Type == UnitType.Player)
            {
                Target = _game.GetEnemy();
                if (Target != null)
                {
                    Target.View.ShowEnemyCycle();
                    _attackComponent.StartAttack();
                }
                else
                {
                    _attackComponent.StopAttack();
                }   
            }
            else
            {
                if (TargetDist < _attackRange.Value)
                {
                    if(State == UnitState.Moving) return;
                    GoToAttackPlayer(true, true);
                }
                // else if(TargetDist > _stoppingRange.Value && State == UnitState.Moving)
                // {
                //     StopAttackPlayer();
                //     _attackComponent.StopAttack();
                // } 
            }
        }

        public bool HasTarget()
        {
            if (Weapon == null || Target == null) return false;
            return TargetDist <= Weapon.GetParam(GameParamType.AttackRange).Value;
        }

        public void GoToAttackPlayer(bool getSupport, bool boostAttackRange = false)
        {
            if (_patrolBoost != null && View.Patrolling)
            {
                _patrolBoost.StopBoost();
                _patrolBoost = null;
            }
            GetComponent<MovementComponent>().MoveToPlayer();
            _attackComponent.StartAttack();
            SetState(UnitState.Moving);
            if (boostAttackRange)
            {
                var localBoost = new LocalBoost(_attackRange, 5, -1);
                _localBoosts.Add(localBoost);
                localBoost.OnEnd += OnBoostEnd;
            }
            if (getSupport)
            {
                GetSupport();  
            }
        }

        private void GetSupport()
        {
            var enemies = _game.EnemyContainer.GetAllAlive();
            enemies.Remove(this);
            foreach (var enemy in enemies)
            {
                if(enemy.State == UnitState.Death) return;
                //var dist = Vector3.Distance(enemy.View.transform.position, View.transform.position);
                var dist = (enemy.View.transform.position - View.transform.position).sqrMagnitude;
                if (dist <= _supportRange.Value)
                {
                    enemy.GoToAttackPlayer(false, true);
                }
            }   
        }

        private void OnBoostEnd(LocalBoost boost)
        {
            boost.OnEnd -= OnBoostEnd;
            _localBoosts.Remove(boost);
        }

        private void StopAttackPlayer()
        {
            GetComponent<MovementComponent>().StopMove();
            SetState(UnitState.Idle);
        }

        public void SetEmptyHand()
        {
            View.SetRigWeight(0, 0.1f, () =>
            {
                View.Weapon.SetFakeGun(false);
                View.Weapon.SetGun(false);
            }, null);
            View.Animator.SetFloat(ANIMATION_WEAPON, 0);
            View.SetLayerWeight(1,-1);
        }

        public void EquipWeapon(List<ItemData> items)
        {
            if (items.Count == 0) return;
            Weapon = items[0];
            _mainGameWindow.OnEquipWeapon(Weapon);
            View.Animator.SetFloat(ANIMATION_WEAPON, Weapon.GetParam(GameParamType.AnimationWeaponID).Value);
            Debug.Log($"take weapon:{Weapon.Type}");
            View.ShowWeapon();

            if(!View.StoreVersion)
            {
                _attackComponent.InitWeaponConfig();   
            }
        }

        public void EquipWeapon(ItemData item)
        {
            if (item == null)
            {
                Debug.LogError($"Weapon is null on {View.name}");
                return;
            }

            if (Weapon == item && !View.StoreVersion)
            {
                if(_attackComponent.State == AttackComponent.AttackState.Reload) return;
                if (Weapon.Class == ItemClass.Weapon)
                {
                    if (Weapon.Ammunition.Value < Weapon.MaxAmmunition.Value)
                    {
                        StartReload();
                    }
                }
                return;
            }

            if (Type == UnitType.Player && _attackComponent is { State: AttackComponent.AttackState.Reload })
            {
                _mainGameWindow.GetGameItem(Weapon)?.SetReload(false);
                _attackComponent.EndReload();
                View.StopAnimation(UnitAnimationType.Reload);
            }
            Weapon = item;
            switch (Weapon.Class)
            {
                case ItemClass.Weapon:
                    View.SetRigWeight(1, 0, () => View.Weapon.SetFakeGun(false), () => View.Weapon.SetGun(true));
                    break;
                case ItemClass.MeleeWeapon:
                    View.SetRigWeight(0, 0.1f, () =>
                    {
                        View.Weapon.SetFakeGun(true);
                        View.Weapon.SetGun(false);
                    }, () =>
                    {
                        View.Weapon.SetFakeGun(true);
                    });
                    break;
                case ItemClass.Grenade:
                    View.SetRigWeight(0, 0, () =>
                    {
                        View.Weapon.SetFakeGun(true);
                        View.Weapon.SetGun(false);
                    }, () =>
                    {
                        View.Weapon.SetFakeGun(true);
                    });
                    break;
                case ItemClass.MedPack:
                    break;
            }
            View.Animator.SetFloat(ANIMATION_WEAPON, Weapon.GetParam(GameParamType.AnimationWeaponID).Value);
            View.ShowWeapon();
            if (!View.StoreVersion)
            {
                _attackComponent.InitWeaponConfig();
                if (Weapon.Class == ItemClass.Weapon && Weapon.Ammunition.Value <= 0)
                {
                    StartReload();
                }
            }
            
        }

        public void EquipMask(GameBalance.MaskConfig mask)
        {
            if (_currentMaskType != GameBalance.MaskType.None)
            {
                OnMaskEnded();
            }
            switch (mask.Type)
            {
                case GameBalance.MaskType.BoostSpeed:
                    _maskTimer.SetTargetValue(mask.Duration);
                    _maskTimer.Reset();
                    _currentMaskBoost = new LocalBoost(_speed, mask.BoostK, -1, true);
                    _localBoosts.Add(_currentMaskBoost);
                    break;
                case GameBalance.MaskType.DoubleDamage:
                    _maskTimer.SetTargetValue(mask.Duration);
                    _maskTimer.Reset();
                    foreach (var weapon in _game.ChosenWeapons)
                    {
                        if (weapon.HasParam(GameParamType.Dmg))
                        {
                            var dmg = weapon.GetParam(GameParamType.Dmg);
                            dmg.SetValue(dmg.StartValue * 2);
                        }
                    }
                    break;
                case GameBalance.MaskType.DisableReloading:
                    _maskTimer.SetTargetValue(mask.Duration);
                    _maskTimer.Reset();
                    foreach (var weapon in _game.ChosenWeapons)
                    {
                        if (weapon.Class == ItemClass.Weapon)
                        {
                            weapon.Ammunition.BlockChanges = true;
                        }
                    }
                    break;
                case GameBalance.MaskType.BoostShotSpeed:
                    _maskTimer.SetTargetValue(mask.Duration);
                    _maskTimer.Reset();
                    foreach (var weapon in _game.ChosenWeapons)
                    {
                        if (weapon.HasParam(GameParamType.AttackSpeed))
                        {
                            var attackSpeed = weapon.GetParam(GameParamType.AttackSpeed);
                            attackSpeed.SetValue(attackSpeed.StartValue * mask.BoostK);
                        }
                    }
                    _attackComponent.InitWeaponConfig();
                    break;
                case GameBalance.MaskType.BoostAttackRange:
                    _maskTimer.SetTargetValue(mask.Duration);
                    _maskTimer.Reset();
                    foreach (var weapon in _game.ChosenWeapons)
                    {
                        if (weapon.HasParam(GameParamType.AttackRange))
                        {
                            var attackRange = weapon.GetParam(GameParamType.AttackRange);
                            attackRange.SetValue(attackRange.StartValue * mask.BoostK);
                        }
                    }
                    View.ShowWeapon();
                    break;
                case GameBalance.MaskType.Immortal:
                    _maskTimer.SetTargetValue(mask.Duration);
                    _maskTimer.Reset();
                    break;
            }

            _currentMaskType = mask.Type;
            _mainGameWindow.SetMask(mask);
            
            View.ShowMask(mask.Type);
        }

        public void OnMaskEnded()
        {
            _maskTimer.Pause();
            switch (_currentMaskType)
            {
                case GameBalance.MaskType.BoostSpeed:
                    break;
                case GameBalance.MaskType.DoubleDamage:
                    foreach (var weapon in _game.ChosenWeapons)
                    {
                        if (weapon.HasParam(GameParamType.Dmg))
                        {
                            var dmg = weapon.GetParam(GameParamType.Dmg);
                            dmg.SetValue(dmg.StartValue);
                        }
                    }
                    break;
                case GameBalance.MaskType.DisableReloading:
                    foreach (var weapon in _game.ChosenWeapons)
                    {
                        if (weapon.Class == ItemClass.Weapon)
                        {
                            weapon.Ammunition.BlockChanges = false;
                        }
                    }
                    break;
                case GameBalance.MaskType.BoostShotSpeed:
                    foreach (var weapon in _game.ChosenWeapons)
                    {
                        if (weapon.HasParam(GameParamType.AttackSpeed))
                        {
                            var attackSpeed = weapon.GetParam(GameParamType.AttackSpeed);
                            attackSpeed.SetValue(attackSpeed.StartValue);
                        }
                    }
                    _attackComponent.InitWeaponConfig();
                    break;
                case GameBalance.MaskType.BoostAttackRange:
                    foreach (var weapon in _game.ChosenWeapons)
                    {
                        if (weapon.HasParam(GameParamType.AttackRange))
                        {
                            var attackRange = weapon.GetParam(GameParamType.AttackRange);
                            attackRange.SetValue(attackRange.StartValue);
                        }
                    }
                    View.ShowWeapon();
                    break;
            }
            
            if (_currentMaskBoost != null)
            {
                _localBoosts.Remove(_currentMaskBoost);
                _currentMaskBoost.StopBoost();
                _currentMaskBoost = null;
            }
            _mainGameWindow.OnMaskEnd();
            _currentMaskType = GameBalance.MaskType.None;
            View.ShowMask(_currentMaskType);
        } 

        public void Hit(double dmg)
        {
            if (_currentMaskType != GameBalance.MaskType.Immortal)
            {
                _hp -= dmg;
                _popupValueContainer.Show(PopupValueType.Damage, View.transform.position, dmg);
            }
            else
            {
                _popupValueContainer.Show(PopupValueType.BlockDamage, View.transform.position, dmg);
            }
            if (_hp > 0)
            {
                var progress = (float)(_hp / GetParam(GameParamType.MaxHp).Value);
                OnHpChange?.Invoke(progress);
                View.UpdateProgressBar(_hp, progress);

                if (Type != UnitType.Player)
                {
                    var position = View.transform.position + (-View.transform.forward * _battle.HitOffset);
                    View.SetPosition(position);   
                }
                if (State == UnitState.Idle && Type == UnitType.EnemyBase)
                {
                    GoToAttackPlayer(true, true);
                }
                View.Blink();
            }
            else
            {
                View.HideProgressBar();
                View.SetDieColor();
                SetState(UnitState.Death);
                if (Type == UnitType.EnemyBase)
                {
                    GetSupport();
                }
            }
        }

        public void Health(ItemType healType)
        {
            var health = _balance.Items.FirstOrDefault(item => item.Type == healType);
            if (health == null)
            {
                Debug.LogError($"Can`t find heal {healType}");
                return;
            }

            var healthPoint = health.Params.FirstOrDefault(param => param.ParamType == GameParamType.Health);
            if (healthPoint == null) return;
            _hp += healthPoint.Value;
            if (_hp > GetParam(GameParamType.MaxHp).Value)
            {
                _hp = GetParam(GameParamType.MaxHp).Value;
            }
            View.UpdateProgressBar(_hp, (float)(_hp / GetParam(GameParamType.MaxHp).Value));
            
            _popupValueContainer.Show(PopupValueType.Heal, View.transform.position, healthPoint.Value);
            View.HealthEffect.Play();
        }

        private void ResetParam()
        {
            var hp = GetParam(GameParamType.MaxHp);
            _hp = hp.Value;
            SetState(UnitState.Idle);
            Target = null;
        }

        public void Destroy()
        {
            if (_indicator != null)
            {
                _indicatorWindow.RemoveIndicator(_indicator);   
            }
            if (Type == UnitType.EnemyBase)
            {
                RemoveAllComponents();   
            }
            else
            {
                ResetParam();
            }

            foreach (var boost in _localBoosts)
            {
                boost.OnEnd -= OnBoostEnd;
                boost.StopBoost();
            }
            _localBoosts.Clear();
            View.KillTweens();
            DestroyView(true);
        }
    }

    public enum UnitType
    {
        Player,
        EnemyBase,
    }
}