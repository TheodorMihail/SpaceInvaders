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

        void Initialize(ShipStats stats, PowerupConfigSO config, float duration);
        void Refresh(float duration); // restart the embedded timer without re-applying the bonus
        void CancelTimer(); // teardown: stop the timer without reverting the bonus or raising Ended
    }

    /// <summary>
    /// Applies its effect on initialization and reverts it after the duration it was activated with.
    /// A duration of 0 or less is instant and never starts a timer.
    /// </summary>
    public abstract class PowerupBaseBehaviour : IPowerupBehaviour
    {
        protected ShipStats Stats { get; private set; }
        protected PowerupConfigSO Config { get; private set; }
        protected float Duration { get; private set; }
        private CancellationTokenSource _cts;

        public abstract PowerupTypes PowerupType { get; }
        public event Action<IPowerupBehaviour> Ended;

        public void Initialize(ShipStats stats, PowerupConfigSO config, float duration)
        {
            Stats = stats;
            Config = config;
            Duration = duration;
            OnApply();

            if (duration > 0f)
            {
                StartTimer();
            }
        }

        public void Refresh(float duration)
        {
            Duration = duration;
            _cts?.CancelAndDispose();
            StartTimer();
        }

        public void CancelTimer()
        {
            _cts?.Cancel();
        }

        protected abstract void OnApply();
        protected abstract void OnRemove();

        private void StartTimer()
        {
            _cts = new CancellationTokenSource();
            RunTimer(_cts.Token).Forget();
        }

        private async UniTaskVoid RunTimer(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(Duration), cancellationToken: token);

            if (token.IsCancellationRequested)
            {
                return;
            }

            OnRemove();
            Ended?.Invoke(this);
        }
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
            var config = (DamageBoostPowerupConfigSO)Config;
            Stats.DamageStat.AddBonus(config.Bonus, config.ValueType);
        }

        protected override void OnRemove()
        {
            var config = (DamageBoostPowerupConfigSO)Config;
            Stats.DamageStat.RemoveBonus(config.Bonus, config.ValueType);
        }
    }

    public class RapidFirePowerup : PowerupBaseBehaviour
    {
        public override PowerupTypes PowerupType => PowerupTypes.RapidFire;

        protected override void OnApply()
        {
            var config = (RapidFirePowerupConfigSO)Config;
            Stats.FireRateStat.AddBonus(config.Bonus, config.ValueType);
        }

        protected override void OnRemove()
        {
            var config = (RapidFirePowerupConfigSO)Config;
            Stats.FireRateStat.RemoveBonus(config.Bonus, config.ValueType);
        }
    }

    /// <summary>The weapon handles the reload and the refill on both edges of the toggle.</summary>
    public class UnlimitedAmmoPowerup : PowerupBaseBehaviour
    {
        public override PowerupTypes PowerupType => PowerupTypes.UnlimitedAmmo;

        protected override void OnApply()
        {
            Stats.SetUnlimitedAmmo(true);
        }

        protected override void OnRemove()
        {
            Stats.SetUnlimitedAmmo(false);
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
