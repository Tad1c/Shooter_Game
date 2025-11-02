using UnityEngine;

namespace Scripts.Weapons
{
    public abstract class WeaponData : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _cooldown = 0.4f;

        public float Cooldown => _cooldown;
    }
}

