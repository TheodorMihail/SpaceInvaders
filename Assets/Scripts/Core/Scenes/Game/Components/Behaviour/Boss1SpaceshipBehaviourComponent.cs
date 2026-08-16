using System.Collections.Generic;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Cycles through its attacks in turn, so the player gets a readable rhythm to learn rather than
    /// a single pattern to sit still against. Order comes from the prefab hierarchy.
    /// </summary>
    public class Boss1SpaceshipBehaviourComponent : EnemySpaceshipBehaviourComponent
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
