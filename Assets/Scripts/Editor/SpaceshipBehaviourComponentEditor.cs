using SpaceInvaders.Scenes.Game;
using UnityEditor;
using UnityEngine;

namespace SpaceInvaders.Editor
{
    [CustomEditor(typeof(BaseSpaceshipBehaviourComponent), true)]
    public class SpaceshipBehaviourComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var ship = (BaseSpaceshipBehaviourComponent)target;
            ShipStats stats = ship.Stats;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Ship Stats (Runtime)", EditorStyles.boldLabel);

            if (stats == null)
            {
                EditorGUILayout.HelpBox("Ship has not spawned yet.", MessageType.Info);
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.IntField("Current Health", stats.CurrentHealth);
                EditorGUILayout.IntField("Max Health", stats.CurrentMaxHealth);
                EditorGUILayout.FloatField("Move Speed", stats.CurrentMoveSpeed);
                EditorGUILayout.FloatField("Fire Rate", stats.CurrentFireRate);
                EditorGUILayout.IntField("Projectile Damage", stats.CurrentProjectileDamage);
                EditorGUILayout.FloatField("Projectile Speed", stats.CurrentProjectileSpeed);
                EditorGUILayout.IntField("Cumulative Damage Taken", stats.CumulativeDamageTaken);
                EditorGUILayout.Toggle("Is Invincible", stats.IsInvincible);
                EditorGUILayout.IntField("Extra Shot Count", stats.ExtraShotCount);
                EditorGUILayout.FloatField("Spread Angle", stats.SpreadAngleDegrees);
            }

            if (Application.isPlaying)
            {
                Repaint();
            }
        }
    }
}
