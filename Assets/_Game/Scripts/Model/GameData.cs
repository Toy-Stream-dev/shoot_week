using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Balance;
using _Game.Scripts.Enums;
using _Game.Scripts.Model.Numbers;
using _Game.Scripts.Saves;
using UnityEngine;

namespace _Game.Scripts.Model
{
    [Serializable]
    public class GameData
    {
        [SerializeField] private int _region;
        [SerializeField] private List<GameParam> _params = new List<GameParam>();
        [SerializeField] private List<GameProgress> _progresses = new List<GameProgress>();

        private GameBalance _balance;
        private Progression _expToLevelUp;

        private GameParam _level;
        private GameProgress _exp;
        
        public int Region => _region;
        
        public DateTime WentBackgroundTime { get; private set; }

        public GameData(int region)
        {
            _region = region;
            _balance = GameBalance.Instance;

            CreateParam(GameParamType.Soft, GameBalance.Instance.StartSoft, false);
            CreateParam(GameParamType.Hard, 0, false);
            CreateParam(GameParamType.MapId, 1, false);
            
            _level = CreateParam(GameParamType.PlayerLevel, GameBalance.Instance.PlayerConfig.StartPlayerLevel, false);

            _expToLevelUp =
                GameBalance.Instance.Progressions.FirstOrDefault(progression => progression.Type == GameParamType.ExpToLevelUp);
            if (_expToLevelUp == null)
            {
                Debug.LogError( $"Can`t find Progression with Type {GameParamType.ExpToLevelUp}");
            }
            else
            {
                _exp = CreateProgress(GameParamType.PlayerLevel, (float)_expToLevelUp.GetValue(_level.Level), false);
                _exp.CompletedEvent += OnLevelComplete;
                //FillPlayerLevelProgress();
            }
        }

        public void IncreaseExperience(int exp)
        {
            int remainder = 0;
            if (_exp.CurrentValue + exp > _exp.TargetValue)
            {
                remainder = (int)_exp.CurrentValue + exp - (int)_exp.TargetValue;
            }
            _exp.Change(exp);
            if (remainder > 0)
            {
                IncreaseExperience(remainder);   
            }
            //FillPlayerLevelProgress();
        }

        private void OnLevelComplete()
        {
            _level.IncLevel();
            _exp.SetTargetValue((float)_expToLevelUp.GetValue(_level.Level));
            _exp.Reset();
            //FillPlayerLevelProgress();
        }

        private void FillPlayerLevelProgress()
        {
            // var config = GameBalance.Instance.PlayerLevelsConfig.FirstOrDefault(l => l.Level == _level.Value);
            // _exp.SetTargetValue(config?.Amount ?? 0);
        }

        public void IncRegion()
        {
            _region++;
        }
       
        private GameParam CreateParam(GameParamType type, float baseValue = 0f, bool updateParamValue = true)
        {
            var param = new GameParam(type, baseValue);
            _params.Add(param);
            return param;
        }
		
        public GameParam GetParam(GameParamType type, bool createIfNotExists = true)
        {
            var param = _params.Find(p => p.Type == type);

            if (param == null && createIfNotExists)
            {
                param = new GameParam(type);
                _params.Add(param);
            }

            return param;
        }

        public bool HasParam(GameParamType type) => GetParam(type, false) != null;

        public GameProgress CreateProgress(GameParamType type, BigNumber target, bool looped = true)
        {
            var progress = new GameProgress(type, target, looped);
            _progresses.Add(progress);
            return progress;
        }

        public GameProgress GetProgress(GameParamType type) => _progresses.Find(p => p.Type == type);

        public IEnumerable<GameParamType> GetCurrentParams()
        {
            return _params.Select(p => p.Type);
        }
        
        public void CopyFrom(GameData source)
        {
            _params.CopyFrom(source._params);
            _progresses.CopyFrom(source._progresses);
        }
    }
}