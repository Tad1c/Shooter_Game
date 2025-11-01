using System;
using UnityEngine;

namespace Scripts.Weapons
{
    /// <summary>
    /// Describes the runtime context that a weapon instance requires in order to operate.
    /// </summary>
    [Serializable]
    public class WeaponRuntimeContext
    {
        public Transform Owner { get; }
        public Transform FirePoint { get; }
        public Camera Camera { get; }

        public WeaponRuntimeContext(Transform owner, Transform firePoint, Camera camera)
        {
            Owner = owner;
            FirePoint = firePoint == null ? owner : firePoint;
            Camera = camera;
        }
    }

    public interface IWeapon
    {
        float Cooldown { get; }

        void Initialize(WeaponRuntimeContext context);

        void Fire(Vector2 direction);
    }
}

