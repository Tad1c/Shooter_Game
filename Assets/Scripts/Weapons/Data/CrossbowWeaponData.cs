using UnityEngine;

namespace Scripts.Weapons
{
    [CreateAssetMenu(menuName = "Weapons/Crossbow", fileName = "CrossbowWeaponData")]
    public class CrossbowWeaponData : WeaponData
    {
        [Header("Projectile")]
        [SerializeField] private ArrowProjectile _arrowPrefab;

        [SerializeField, Min(0f)] private float _projectileSpeed = 14f;
        [SerializeField, Min(0f)] private float _projectileLifetime = 6f;
        [SerializeField, Min(0)] private int _maxRicochets = 5;

        public ArrowProjectile ArrowPrefab => _arrowPrefab;
        public float ProjectileSpeed => _projectileSpeed;
        public float ProjectileLifetime => _projectileLifetime;
        public int MaxRicochets => _maxRicochets;
    }
}

