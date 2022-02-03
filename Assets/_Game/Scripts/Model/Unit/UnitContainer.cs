using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Model.Base;
using _Game.Scripts.View;
using GeneralTools.Model;
using UnityEngine;

namespace _Game.Scripts.Model.Unit
{
    public class UnitContainer : ModelsContainer<UnitModel>
    {
        private MapModel _map;
        private int _aliveUnit;

        public override BaseModel Start()
        {
            _map = Models.Get<MapModel>();
            
            return base.Start();
        }

        public UnitModel SpawnPlayer()
        {
            _aliveUnit = 0;
            var unit = Create(false)
                .SpawnView(MainGame.UnitsContainer, true, view => view.Type == UnitType.Player && !view.StoreVersion)
                .Init(UnitType.Player, 0)
                .Start();

            _aliveUnit++;
            unit.OnDie += OnUnitDie;
            
            return unit;
        }

        public UnitModel SpawnStorePlayer()
        {
            var unit = Create(false)
                .SpawnView(MainGame.UnitsContainer, true, view => view.Type == UnitType.Player && view.StoreVersion)
                .Init(UnitType.Player, 0)
                .Start();

            return unit;
        }

        public void SpawnView(UnitModel unitModel)
        {
            _aliveUnit = 0;
            unitModel.SpawnView(MainGame.UnitsContainer, true, view => view.Type == UnitType.Player && !view.StoreVersion)
                .Start();

            _aliveUnit++;
            unitModel.OnDie += OnUnitDie;
        }

        public void InitEnemies()
        {
            _aliveUnit = 0;
            All.Clear();

            var enemies = _map.View.GetComponentsInChildren<UnitView>();
            for (int i = 0; i < enemies.Length; i++)
            {
                var enemy = Create(false)
                    .SetView(enemies[i])
                    .Init(UnitType.EnemyBase, i+ 1)
                    .Start();
                _aliveUnit++;
                enemy.OnDie += OnUnitDie;
            }
        }

        public UnitModel GetNearestEnemy(Vector3 player)
        {
            float min = -1;
            UnitModel target = null;
            foreach (var enemy in All)
            {
                if(enemy.State == UnitModel.UnitState.Death) continue;
                var dist = (player - enemy.View.transform.position).sqrMagnitude;
                if (min < dist && min > 0) continue;
                min = dist;
                target = enemy;
            }

            foreach (var model in All)
            {
                if(model == target) continue;
                model.View.HideEnemyCycle();
            }
            
            return target;
        }

        public List<UnitModel> GetAllAlive()
        {
            return new List<UnitModel>(All.Where(unit => unit.State != UnitModel.UnitState.Death));
        }

        public void GoToAttackAll()
        {
            foreach (var enemy in All)
            {
                if(enemy.State == UnitModel.UnitState.Death) continue;
                enemy.GoToAttackPlayer(false);
            }
        }

        private void OnUnitDie(UnitModel unitModel)
        {
            unitModel.OnDie -= OnUnitDie;
            _aliveUnit--;
            if (_aliveUnit != 0) return;
            if (unitModel.Type == UnitType.Player)
            {
                Models.Get<GameModel>().CompleteLevel(false);
            }
            else
            {
                Models.Get<GameModel>().SetLevelClear();
            }
        }

        public void ClearAll()
        {
            if (All.First().Type == UnitType.Player)
            {
                var player = All.First();
                player.OnDie -= OnUnitDie;
                player.Destroy();
            }
            else
            {
                foreach (var unitModel in All)
                {
                    unitModel.OnDie -= OnUnitDie;
                    unitModel.Destroy();
                }
                All.Clear();   
            }
        }
    }
}