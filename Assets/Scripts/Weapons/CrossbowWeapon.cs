using UnityEngine;

namespace Scripts.Weapons
{
    public class CrossbowWeapon : WeaponBehaviour
    {
        [Header("Crossbow Settings")]
        [SerializeField] private ArrowProjectile _arrowPrefab;
        [SerializeField, Min(0f)] private float _projectileSpeed = 14f;
        [SerializeField, Min(0f)] private float _projectileLifetime = 6f;
        [SerializeField, Min(0)] private int _maxRicochets = 5;

        public override void Fire(Vector2 direction)
        {
            if (Context == null)
            {
                Debug.LogWarning($"{name} tried to fire without being initialized.");
                return;
            }

            if (_arrowPrefab == null)
            {
                Debug.LogWarning($"{name} has no arrow prefab assigned.");
                return;
            }

            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            var normalizedDirection = direction.normalized;
            var firePoint = Context.FirePoint != null ? Context.FirePoint : transform;

            var arrowInstance = Instantiate(_arrowPrefab, firePoint.position, Quaternion.identity);
            arrowInstance.Initialize(normalizedDirection, _projectileSpeed, _maxRicochets, Context.Camera, _projectileLifetime);
        }
    }
}

