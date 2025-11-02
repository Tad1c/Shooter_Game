using UnityEngine;
using Zenject;

namespace Scripts.Weapons
{
    public class WeaponRuntimeContext
    {
        public Transform Owner { get; }
        public Transform FirePoint { get; }
        public Camera Camera { get; }
        public DiContainer Container { get; }

        public WeaponRuntimeContext(Transform owner, Transform firePoint, Camera camera, DiContainer container)
        {
            Owner = owner;
            FirePoint = firePoint == null ? owner : firePoint;
            Camera = camera;
            Container = container;
        }
    }

    public interface IWeapon
    {
        float Cooldown { get; }

        void Initialize(WeaponRuntimeContext context);

        void Fire(Vector2 direction);
    }
}

