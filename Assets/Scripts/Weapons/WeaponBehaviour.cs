using UnityEngine;

namespace Scripts.Weapons
{
    public abstract class WeaponBehaviour : MonoBehaviour, IWeapon
    {
        [SerializeField, Min(0f)] private float _cooldown = 0.4f;

        protected WeaponRuntimeContext Context { get; private set; }

        public virtual float Cooldown => _cooldown;

        public virtual void Initialize(WeaponRuntimeContext context)
        {
            Context = context;
        }

        public abstract void Fire(Vector2 direction);
    }
}

