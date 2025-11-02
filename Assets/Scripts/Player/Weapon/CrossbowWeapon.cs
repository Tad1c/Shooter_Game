using UnityEngine;
using Zenject;

namespace Scripts {
	public class CrossbowWeapon : IWeapon {
		private readonly WeaponData _weaponData;
		private readonly DiContainer _container;
		private float _currentCooldown;

		public float CurrentCooldown => _currentCooldown;
		public float MaxCooldown => _weaponData.Cooldown;

		public CrossbowWeapon(WeaponData weaponData, DiContainer container) {
			_weaponData = weaponData;
			_container = container;
			_currentCooldown = 0f;
		}

		public bool TryFire(Vector2 firePosition, Vector2 direction) {
			if (_currentCooldown > 0) {
				return false;
			}
			if (direction.magnitude < 0.1f) {
				return false;
			}

			Fire(firePosition, direction);
			_currentCooldown = _weaponData.Cooldown;
			return true;
		}

		private void Fire(Vector2 firePosition, Vector2 direction) {
			if (_weaponData.ProjectilePrefab == null) {
				Debug.LogError("Projectile prefab is not assigned!");
				return;
			}

			GameObject projectileObj = _container.InstantiatePrefab(
				_weaponData.ProjectilePrefab,
				firePosition,
				Quaternion.identity,
				null
				);

			var projectile = projectileObj.GetComponent<IProjectile>();
			if (projectile != null) {
				projectile.Launch(firePosition, direction, _weaponData.ProjectileSpeed);

				if (projectile is ArrowProjectile arrow) {
					arrow.Damage = _weaponData.Damage;
				}
			}
			else {
				Debug.LogError("Projectile doesn't implement IProjectile interface!");
				Object.Destroy(projectileObj);
			}
		}

		public void Update(float deltaTime) {
			if (_currentCooldown > 0) {
				_currentCooldown -= deltaTime;
				if (_currentCooldown < 0) {
					_currentCooldown = 0;
				}
			}
		}
	}
}
