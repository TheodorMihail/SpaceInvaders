using System.Collections.Generic;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Shared boss behaviour: invulnerable while entering, then cycles through its attacks in turn.
    /// Attack order comes from the prefab hierarchy.
    /// </summary>
    public abstract class BossSpaceshipBehaviourComponent : EnemySpaceshipBehaviourComponent
    {
        private int _shotCounter;

        protected override bool IsInvulnerableWhileEntering => true;

        public override void OnDespawned()
        {
            base.OnDespawned();
            _shotCounter = 0;
        }

        protected override BaseShipAttackComponent SelectAttack()
        {
            IReadOnlyList<BaseShipAttackComponent> attacks = _weapon.Attacks;

            if (attacks.Count == 0)
            {
                return null;
            }

            return attacks[_shotCounter % attacks.Count];
        }

        protected override void OnAttackFired(BaseShipAttackComponent attack)
        {
            _shotCounter++;
        }
    }
}
