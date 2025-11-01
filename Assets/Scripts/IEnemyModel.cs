using UniRx;
using UnityEngine;

namespace Scripts
{
    public interface IEnemyModel
    {
        IReadOnlyReactiveProperty<float> CurrentHealth { get; }
        IReadOnlyReactiveProperty<Vector2> TargetPosition { get; }
        IReadOnlyReactiveProperty<bool> IsAlive { get; }
        IReadOnlyReactiveProperty<float> AttackCooldownTimer { get; }

        EnemyData Data { get; }

        void TakeDamage(float damage);
        void SetTargetPosition(Vector2 position);
        void UpdateAttackCooldown(float deltaTime);
        bool CanAttack();
        void Attack();
        void Die();
        void Reset();
    }
}

