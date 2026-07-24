using System;
using System.Threading;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;

namespace SpaceInvaders.Scenes.Game
{
    public interface IPowerupBehaviour
    {
        PowerupTypes PowerupType { get; }
        event Action<IPowerupBehaviour> Ended; // only ever raised for timed powerups (Duration > 0)

        void Initialize(ShipStats stats, PowerupConfigSO config);
        void Refresh(); // restart the embedded timer without re-applying the bonus
        void CancelTimer(); // teardown: stop the timer without reverting the bonus or raising Ended
    }

    public abstract class PowerupBaseBehaviour : IPowerupBehaviour
    {
        protected ShipStats Stats { get; private set; }
        protected PowerupConfigSO Config { get; private set; }
        private CancellationTokenSource _cts;

        public abstract PowerupTypes PowerupType { get; }
        public event Action<IPowerupBehaviour> Ended;

        public void Initialize(ShipStats stats, PowerupConfigSO config)
        {
            Stats = stats;
            Config = config;
            OnApply();

            if (config.Duration > 0f)
            {
                StartTimer();
            }
        }

        public void Refresh()
        {
            _cts?.CancelAndDispose();
            StartTimer();
        }

        public void CancelTimer()
        {
            _cts?.Cancel();
        }

        private void StartTimer()
        {
            _cts = new CancellationTokenSource();
            RunTimer(_cts.Token).Forget();
        }

        private async UniTaskVoid RunTimer(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(Config.Duration), cancellationToken: token);

            if (token.IsCancellationRequested)
            {
                return;
            }

            OnRemove();
            Ended?.Invoke(this);
        }

        protected abstract void OnApply();
        protected abstract void OnRemove();
    }

    public class InvincibilityPowerup : PowerupBaseBehaviour
    {
        public override PowerupTypes PowerupType => PowerupTypes.Invincibility;

        protected override void OnApply()
        {
            Stats.SetInvincible(true);
        }

        protected override void OnRemove()
        {
            Stats.SetInvincible(false);
        }
    }

    public class HealPowerup : PowerupBaseBehaviour
    {
        public override PowerupTypes PowerupType => PowerupTypes.Heal;

        protected override void OnApply()
        {
            Stats.Heal(((HealPowerupConfigSO)Config).HealAmount);
        }

        protected override void OnRemove() { } // instant — Duration <= 0 means the timer/Ended path never runs
    }

    public class DamageBoostPowerup : PowerupBaseBehaviour
    {
        public override PowerupTypes PowerupType => PowerupTypes.DamageBoost;

        protected override void OnApply()
        {
            Stats.DamageStat.AddBonus(((DamageBoostPowerupConfigSO)Config).DamageMultiplierBonus);
        }

        protected override void OnRemove()
        {
            Stats.DamageStat.RemoveBonus(((DamageBoostPowerupConfigSO)Config).DamageMultiplierBonus);
        }
    }

    public class RapidFirePowerup : PowerupBaseBehaviour
    {
        public override PowerupTypes PowerupType => PowerupTypes.RapidFire;

        protected override void OnApply()
        {
            Stats.FireRateStat.AddBonus(((RapidFirePowerupConfigSO)Config).FireRateMultiplierBonus);
        }

        protected override void OnRemove()
        {
            Stats.FireRateStat.RemoveBonus(((RapidFirePowerupConfigSO)Config).FireRateMultiplierBonus);
        }
    }

    public class SpreadShotPowerup : PowerupBaseBehaviour
    {
        public override PowerupTypes PowerupType => PowerupTypes.SpreadShot;

        protected override void OnApply()
        {
            var config = (SpreadShotPowerupConfigSO)Config;
            Stats.UpdateShotSpread(config.ExtraShotCount, config.SpreadAngleDegrees);
        }

        protected override void OnRemove()
        {
            var config = (SpreadShotPowerupConfigSO)Config;
            Stats.UpdateShotSpread(-config.ExtraShotCount, config.SpreadAngleDegrees);
        }
    }
}
