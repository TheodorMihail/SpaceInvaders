using System;
using System.Threading;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class VFXBehaviourComponent : MonoBehaviour, IPoolableObject
    {
        [Inject] private readonly ISpawnService _spawnService;

        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private float _lifetime = 2f;

        private CancellationTokenSource _cts;

        public void OnSpawned()
        {
            _particleSystem.Play(true);

            _cts = new CancellationTokenSource();
            DespawnAfterLifetime(_cts.Token).Forget();
        }

        public void OnDespawned()
        {
            _cts?.CancelAndDispose();
            _cts = null;

            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private async UniTaskVoid DespawnAfterLifetime(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_lifetime), cancellationToken: token);

            if (token.IsCancellationRequested)
            {
                return;
            }

            _spawnService.Despawn(this);
        }
    }
}
