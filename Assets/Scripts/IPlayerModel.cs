using UniRx;

namespace Scripts
{
    public interface IPlayerModel
    {
        IReadOnlyReactiveProperty<float> CurrentHealth { get; }
        IReadOnlyReactiveProperty<float> MaxHealth { get; }
        IReadOnlyReactiveProperty<bool> IsAlive { get; }
        
        void TakeDamage(float damage);
        void Heal(float amount);
    }
}