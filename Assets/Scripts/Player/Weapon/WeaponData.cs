using UnityEngine;

namespace Scripts {
	[CreateAssetMenu(fileName = "WeaponData", menuName = "Game/Weapon Data")]
	public class WeaponData : ScriptableObject {
		[Header("Basic Properties")]
		[SerializeField] private string _weaponName;
		[SerializeField] private float _fireRate = 1f;
		[SerializeField] private float _projectileSpeed = 10f;
		[SerializeField] private float _damage = 10f;
		
		[Header("Projectile")]
		[SerializeField] private GameObject _projectilePrefab;
		
		public string WeaponName => _weaponName;
		public float Cooldown => 1f / _fireRate;
		public float ProjectileSpeed => _projectileSpeed;
		public float Damage => _damage;
		public GameObject ProjectilePrefab => _projectilePrefab;
	}
}
