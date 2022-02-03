using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Enums;
using _Game.Scripts.Model.Drop;
using _Game.Scripts.Model.Items;
using GeneralTools.Tools;
using GoogleParse;
using GoogleParse.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Scripts.Balance
{
	[CreateAssetMenu(fileName = "GameBalance", menuName = "_Game/GameBalance", order = 0)]
	[TableSheetName("Constants")]
	public class GameBalance : SingletonScriptableObject<GameBalance>, IParsable
	{
		[Header("AdSettings")] 
		public int MaskWindowSoftValue;
		public float OfferRespawnDelay;

		[Header("GeneralSettings")]
		public int MaxLevelID;
		public int TutorialLevelID;
		public int ShowMaskLevel;
		public int StartSoft;

		[Header("GeneralSettings")]
		public float FingerDelay;
		
		[Header("StageSettings")]
		public List<StageConfig> StageConfigs;

		[Header("ShakeSettings")]
		public float ShakeIntensity;
		public float ShakeDuration;

		[Header("CameraSettings")]
		public float MinFOV;
		public float ChangeFOVDuration;

		[Header("BulletSettings")]
		public float PushForce;

		public PlayerParamsConfig PlayerConfig;
		public List<PlayerLevelConfig> PlayerLevelsConfig;
		public List<ItemConfig> Items;
		public List<EnemyConfig> EnemiesConfig;
		public List<ItemConfig> EnemyItems;
		public List<MaskConfig> Masks;

		[Header("UnitSettings")]
		public float DisableUnitPhysicsDelay;
		public float IncreaseSpeedForMelee;
		public float BlinkDuration;
		public Color DeathColor;
		
		[Header("DropSettings")]
		public List<DropConfig> DropConfigs;
		public int MoneyDropCount;
		public float LootBoxOpenDelay;
		public float CollectSpeed;
		public float CollectDelay;

		[Header("ProgressionSettings")]
		public List<Progression> Progressions;
#if UNITY_EDITOR
		[UnityEditor.MenuItem("_Game/Balance")]
		private static void OpenBalance()
		{
			UnityEditor.Selection.activeObject = Instance;
		}
#endif
		public void OnParsed()
		{
		}

		[Serializable]
		public class PlayerParamsConfig
		{
			public float DrawArrowRange;
			public int StartPlayerLevel;
			public int Hp;
			public float Speed;
			public float ChangeRigDelay;
			[Range(0, 1)]
			public float HindWalkSpeedPercent;
		}

		[Serializable]
		public class EnemyConfig
		{
			public EnemyType Type;
			public bool Boss;
			[ShowIf("Boss")] public string Name;
			public int Hp;
			public int Experience;
			public int SoftPerPack;
			public float Speed;
			public float AttackRange;
			public float StoppingRange;
			public float SupportRange;
			public float PatrolRotationDelay;
			public float PatrolEndPointDelay;
			public float NavMeshStopDistance;
			[Range(0,100)] public int DropHealthChance;
		}
		
		[Serializable]
		public class DropConfig : IRandomizedByWeight
		{
			public ItemClass ItemClass;
			public ItemType Type;
			public int Level;
			public float Weight;
			public float RandomWeight => Weight;
			
			public float DropRange;
			public float JumpForce;
			public float JumpTime;
		}
		
		[Serializable]
		public class StageConfig
		{
			public int Id;
			public Sprite Image;
			public string MapText;
			public List<int> Regions = new List<int>();

			public bool HasRegion(int currentRegion)
			{
				foreach (var region in Regions)
				{
					if (region == currentRegion) return true;
				}
				
				//Debug.LogError($"Can`t find region {currentRegion} in StageConfig with ID{Id}");
				return false;
			}

			public (int, int) GetStage(int currentRegion)
			{
				if (HasRegion(currentRegion))
				{
					var index = Regions.FindIndex(item => item == currentRegion) + 1;
					var maxIndex = Regions.Count;
					return (index, maxIndex);
				}
				else
				{
					//Debug.LogError($"Can`t find region {currentRegion} in StageConfig with ID{Id}");
					return (-1, -1);
				}
			}
		}

		[Serializable]
		public enum EnemyType
		{
			None,
			Inmate,
			Shirt,
			Tommy,
			BossInmate,
			BossPimp,
		}

		[Serializable]
		public class ItemConfig
		{
			public ItemClass Class;
			[ShowIf("Class", ItemClass.Weapon)]
			public float ReloadAnimationDuration;
			public ItemType Type;
			public GameSoundType SoundType;
			[ShowIf("Type", ItemType.Shotgun)]
			public int BulletPerShoot;
			[ShowIf("Type", ItemType.Shotgun)]
			public float MinScatter;
			[ShowIf("Type", ItemType.Shotgun)]
			public float MaxScatter;
			public List<itemParamConfig> Params;
		}

		[Serializable]
		public class itemParamConfig
		{
			public GameParamType ParamType;
			public float Value;
		}

		[Serializable]
		public class PlayerLevelConfig
		{
			public int Level;
			public int Amount;
		}

		[Serializable]
		public class MaskConfig
		{
			public int Id;
			public MaskType Type;
			public string Text;
			public int Price;
			public string Image;
			public float Duration;
			public bool Boost;
			[ShowIf("Boost")]
			public float BoostK;

			public MaskConfig(MaskConfig maskConfig)
			{
				Id = maskConfig.Id;
				Type = maskConfig.Type;
				Text = maskConfig.Text;
				Price = maskConfig.Price;
				Image = maskConfig.Image;
				Duration = maskConfig.Duration;
				Boost = maskConfig.Boost;
				BoostK = maskConfig.BoostK;
			}
		}

		public enum MaskType
		{
			None,
			BoostSpeed,
			BoostShotSpeed,
			DisableReloading,
			DoubleDamage,
			BoostAttackRange,
			Immortal,
		}
	}
}