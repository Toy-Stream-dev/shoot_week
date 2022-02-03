using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Model.Items;
using GeneralTools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Scripts.View
{
    public class Muzzle : BaseBehaviour
    {
        [SerializeField] private List<MuzzlePreset> _muzzles;

        public void Play(ItemType type)
        {
            var muzzle = _muzzles.FirstOrDefault(muzzle => muzzle.Type == type);
            muzzle?.Play();
        }
    }

    [Serializable]
    public class MuzzlePreset
    {
        public ItemType Type;
        public List<ParticleSystem> Effects;

        [Button]
        public void Play()
        {
            foreach (var effect in Effects)
            {
                effect.Play();
            }
        }
    }
}