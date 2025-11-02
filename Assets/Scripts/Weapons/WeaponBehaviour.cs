using UnityEngine;

namespace Scripts.Weapons
{
    public abstract class WeaponBehaviour : MonoBehaviour, IWeapon
    {
        [SerializeField] private WeaponData _defaultData;

        protected WeaponRuntimeContext Context { get; private set; }
        protected WeaponData Data => _overrideData != null ? _overrideData : _defaultData;

        private WeaponData _overrideData;

        public virtual float Cooldown => Data != null ? Data.Cooldown : 0f;

        public void SetData(WeaponData data)
        {
            _overrideData = data;
        }

        protected WeaponData GetDefaultData()
        {
            return _defaultData;
        }

        protected void SetDefaultData(WeaponData data)
        {
            _defaultData = data;
        }

        public virtual void Initialize(WeaponRuntimeContext context)
        {
            Context = context;
        }

        protected TData GetData<TData>() where TData : WeaponData
        {
            return Data as TData;
        }

        public abstract void Fire(Vector2 direction);
    }
}

