using UnityEngine;

namespace Scripts
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Game/Enemy Data", order = 0)]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _enemyName;
        [SerializeField] private EnemyView _enemyPrefab;

        [Header("Stats")]
        [SerializeField, Range(1f, 1000f)] private float _maxHealth = 100f;
        [SerializeField, Range(0.1f, 10f)] private float _moveSpeed = 2f;
        [SerializeField, Range(1f, 100f)] private float _damage = 10f;

        [Header("Combat")]
        [SerializeField, Range(0.1f, 5f)] private float _attackRange = 1f;
        [SerializeField, Range(0.1f, 5f)] private float _attackCooldown = 1f;

        [Header("Visual")]
        [SerializeField] private Color _tintColor = Color.white;
        [SerializeField, Range(0.5f, 3f)] private float _scale = 1f;

        public string EnemyName => _enemyName;
        public EnemyView EnemyPrefab => _enemyPrefab;
        public float MaxHealth => _maxHealth;
        public float MoveSpeed => _moveSpeed;
        public float Damage => _damage;
        public float AttackRange => _attackRange;
        public float AttackCooldown => _attackCooldown;
        public Color TintColor => _tintColor;
        public float Scale => _scale;
    }
}

