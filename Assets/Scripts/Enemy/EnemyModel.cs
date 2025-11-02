using UniRx;
using UnityEngine;

namespace Scripts {
	public class EnemyModel : IEnemyModel {
		private readonly ReactiveProperty<float> _currentHealth;
		private readonly ReactiveProperty<Vector2> _targetPosition = new(Vector2.zero);
		private readonly ReactiveProperty<bool> _isAlive = new(true);
		private readonly ReactiveProperty<float> _attackCooldownTimer = new(0f);
		private readonly EnemyData _data;

		public IReadOnlyReactiveProperty<float> CurrentHealth => _currentHealth;
		public IReadOnlyReactiveProperty<Vector2> TargetPosition => _targetPosition;
		public IReadOnlyReactiveProperty<bool> IsAlive => _isAlive;
		public IReadOnlyReactiveProperty<float> AttackCooldownTimer => _attackCooldownTimer;
		public EnemyData Data => _data;

		public EnemyModel(EnemyData data) {
			_data = data;
			_currentHealth = new ReactiveProperty<float>(data.MaxHealth);
		}

		public void TakeDamage(float damage) {
			if (!_isAlive.Value) {
				return;
			}

			_currentHealth.Value = Mathf.Max(0, _currentHealth.Value - damage);

			if (_currentHealth.Value <= 0) {
				Die();
			}
		}

		public void SetTargetPosition(Vector2 position) {
			_targetPosition.Value = position;
		}

		public void UpdateAttackCooldown(float deltaTime) {
			if (_attackCooldownTimer.Value > 0) {
				_attackCooldownTimer.Value = Mathf.Max(0, _attackCooldownTimer.Value - deltaTime);
			}
		}

		public bool CanAttack() {
			return _isAlive.Value && _attackCooldownTimer.Value <= 0;
		}

		public void Attack() {
			_attackCooldownTimer.Value = _data.AttackCooldown;
		}

		public void Die() {
			_isAlive.Value = false;
		}

		public void Reset() {
			_currentHealth.Value = _data.MaxHealth;
			_isAlive.Value = true;
			_attackCooldownTimer.Value = 0f;
		}
	}
}
