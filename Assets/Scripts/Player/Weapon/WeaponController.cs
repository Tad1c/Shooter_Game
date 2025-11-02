using UnityEngine;
using UniRx;

namespace Scripts {
	public class WeaponController : MonoBehaviour {
		[Header("Weapon Settings")]
		[SerializeField] private Transform _firePoint;
		[SerializeField] private float _autoFireRadius = 5f;

		private IWeapon _currentWeapon;
		private Transform _playerTransform;

		public void SetWeapon(IWeapon weapon) {
			_currentWeapon = weapon;
		}

		private void Awake() {
			_playerTransform = transform;

			if (_firePoint == null) {
				_firePoint = transform;
			}
		}

		private void Update() {
			if (_currentWeapon == null) {
				return;
			}

			_currentWeapon.Update(Time.deltaTime);

			AutoFire();
		}

		private void AutoFire() {
			var enemies = FindObjectsOfType<EnemyView>();
			EnemyView nearestEnemy = null;
			float nearestDistance = float.MaxValue;

			foreach (var enemy in enemies) {
				float distance = Vector2.Distance(_playerTransform.position, enemy.transform.position);
				if (distance < nearestDistance && distance <= _autoFireRadius) {
					nearestDistance = distance;
					nearestEnemy = enemy;
				}
			}

			if (nearestEnemy != null) {
				Vector2 direction = (nearestEnemy.transform.position - _firePoint.position).normalized;
				_currentWeapon.TryFire(_firePoint.position, direction);
			}
		}

		private void OnDrawGizmosSelected() {
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(transform.position, _autoFireRadius);
		}
	}
}
