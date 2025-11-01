using UniRx;
using UnityEngine;

namespace Scripts
{
    public class PlayerModel : IPlayerModel
    {
        private readonly ReactiveProperty<float> _maxHealth = new(100f);
        private readonly ReactiveProperty<float> _currentHealth;
        private readonly ReactiveProperty<bool> _isAlive = new(true);
        
        public IReadOnlyReactiveProperty<float> CurrentHealth => _currentHealth;
        public IReadOnlyReactiveProperty<float> MaxHealth => _maxHealth;
        public IReadOnlyReactiveProperty<bool> IsAlive => _isAlive;

        public PlayerModel()
        {
            _currentHealth = new ReactiveProperty<float>(_maxHealth.Value);
        }

        public void TakeDamage(float damage)
        {
            if (!_isAlive.Value) return;
            
            _currentHealth.Value = Mathf.Max(0, _currentHealth.Value - damage);
            
            if (_currentHealth.Value <= 0)
            {
                _isAlive.Value = false;
                Debug.Log("Player died!");
            }
        }

        public void Heal(float amount)
        {
            if (!_isAlive.Value) return;
            
            _currentHealth.Value = Mathf.Min(_maxHealth.Value, _currentHealth.Value + amount);
        }
    }
}