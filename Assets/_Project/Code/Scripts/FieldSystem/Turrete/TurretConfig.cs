using UnityEngine;

namespace _Project.Code.Scripts.BattleField
{
    [CreateAssetMenu(fileName = "TurretConfig", menuName = "GameConfig/TurretConfig")]
    public class TurretConfig : ScriptableObject
    {
        [Header("Health")]
        public float MaxHp = 60f;

        [Header("Combat")]
        public float AttackRadius = 5f;
        public float FireRate = 1f;
        public float Damage = 10f;
        public float BulletSpeed = 8f;

        [Header("Radius Display")]
        public float RadiusFadeDuration = 1f;
        public float RadiusMaxAlpha = 0.25f;
    }
}
