using System;
using System.Collections.Generic;
using _Game.Scripts.Model.Items;
using GeneralTools.Tools;
using UnityEngine;

namespace _Game.Scripts.View
{
    [CreateAssetMenu(fileName = "BattleSettings", menuName = "_Game/BattleSettings", order = 1)]
    public class BattleSettings : SingletonScriptableObject<BattleSettings>
    {
        public float HitOffset;
    
        public List<WeaponConfig> Weapons;
        public List<ProjectileConfig> Projectiles;

        [Serializable]
        public class WeaponConfig
        {
            public ItemType Type;
            public Mesh Mesh;
            public Vector3 Scale;
            public Vector3 Position;
            public Vector3 Rotation;

            public Vector3 RightHandPosition;
            public Vector3 RightHandRotation;
            public Vector3 LeftHandPosition;
            public Vector3 LeftHandRotation;

            public Vector3 ProjectilePosition;
        }
        
        [Serializable]
        public class ProjectileConfig
        {
            public ItemType Type;
            public Vector3 Scale;
            public Vector3 Rotation;
            public float Duration;
            public float Speed;
            public float PushDelay;
        }
    }
}