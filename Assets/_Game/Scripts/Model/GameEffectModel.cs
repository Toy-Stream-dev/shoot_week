using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Enums;
using _Game.Scripts.Model.Vfx;
using GeneralTools.Model;
using GeneralTools.UI;
using UnityEngine;

namespace _Game.Scripts.Model
{
    public class GameEffectModel : BaseModel
    {
        private readonly List<BaseGameEffect> _effects = new List<BaseGameEffect>();
        private float _time;

        public override void Update(float deltaTime)
        {
            for (int i = 0; i < _effects.Count; i++)
            {
                _effects[i].UpdateMe(deltaTime);
            }
            
            base.Update(deltaTime);
        }

        public void Play(GameEffectType type, BaseModel model, Vector3 pos)
        {
            var effect = _effects.FirstOrDefault(e => e.Type == type && e.OwnerModel == model);
            if (effect == null) effect = GetEffect(type);
            effect.Play(pos);
            
            _effects.Add(effect);
        }
        
        public void Play(GameEffectType type, Vector3 pos)
        {
            var effect = _effects.FirstOrDefault(e => e.Type == type);
            if (effect == null) effect = GetEffect(type);
            effect.Play(pos);
            
            _effects.Add(effect);
        }
        
        private BaseGameEffect GetEffect(GameEffectType type, Transform transform = null)
        {
            switch (type)
            {
            }

            return null;
        }

        public void Remove(BaseGameEffect effect)
        {
            _effects.Remove(effect);
        }
    }
}