using System;
using System.Collections.Generic;
using _Game.Scripts.Model;
using _Game.Scripts.Model.Unit;
using DG.Tweening;
using GeneralTools;
using GeneralTools.Model;
using UnityEngine;

namespace _Game.Scripts.View
{
    public class DiscoEffect : BaseBehaviour
    {
        [SerializeField] private List<ParticleSystem> _particleSystems = new List<ParticleSystem>();

        private void Start()
        {
            foreach (var particleSystem in _particleSystems)
            {
                particleSystem.Play();
            }
        }
    }
}