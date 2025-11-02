using UnityEngine;
using Zenject;

namespace Scripts.Weapons
{
    public class CrossbowWeapon : WeaponBehaviour
    {
        [SerializeField] private CrossbowWeaponData _fallbackData;

        private CrossbowWeaponData Config => GetData<CrossbowWeaponData>() ?? _fallbackData;

        [Inject(Optional = true)]
        public void Construct(CrossbowWeaponData data)
        {
            if (data != null)
            {
                SetData(data);
            }
        }

        private void Awake()
        {
            if (_fallbackData != null && GetDefaultData() == null)
            {
                SetDefaultData(_fallbackData);
            }
        }

        public override void Fire(Vector2 direction)
        {
            if (Context == null)
            {
                Debug.LogWarning($"{name} tried to fire without being initialized.");
                return;
            }

            var config = Config;
            if (config == null)
            {
                Debug.LogWarning($"{name} has no weapon data assigned.");
                return;
            }

            if (config.ArrowPrefab == null)
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
            var container = Context.Container;

            ArrowProjectile arrowInstance;
            if (container != null)
            {
                arrowInstance = container.InstantiatePrefabForComponent<ArrowProjectile>(config.ArrowPrefab.gameObject,
                    firePoint.position, Quaternion.identity, null);
            }
            else
            {
                arrowInstance = Instantiate(config.ArrowPrefab, firePoint.position, Quaternion.identity);
            }

            arrowInstance.Initialize(normalizedDirection, config.ProjectileSpeed, config.MaxRicochets, Context.Camera,
                config.ProjectileLifetime);
        }
    }
}

