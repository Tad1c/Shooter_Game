using UniRx;
using Zenject;
using Scripts.Weapons;

namespace Scripts
{
    public class PlayerPresenter : IInitializable
    {
        private readonly JoystickView _joystickView;
        private readonly PlayerView _playerView;
        private readonly IPlayerModel _playerModel;
        private readonly CompositeDisposable _disposer;
        private readonly PlayerWeaponController _playerWeaponController;

        public PlayerPresenter(JoystickView joystickView, PlayerView playerView, IPlayerModel playerModel,
            CompositeDisposable disposer, PlayerWeaponController playerWeaponController)
        {
            _joystickView = joystickView;
            _playerView = playerView;
            _playerModel = playerModel;
            _disposer = disposer;
            _playerWeaponController = playerWeaponController;
        }

        public void Initialize()
        {
            _joystickView.OnInput
                .Subscribe(direction =>
                {
                    _playerView.Move(direction);
                    _playerWeaponController?.SetAimDirection(direction);
                })
                .AddTo(_disposer);
            
            // Update health bar with normalized health (0-1)
            _playerModel.CurrentHealth
                .Select(health => health / _playerModel.MaxHealth.Value)
                .Subscribe(_playerView.UpdateHealth)
                .AddTo(_disposer);
            
            // Handle death
            _playerModel.IsAlive
                .Where(isAlive => !isAlive)
                .Subscribe(_ => OnPlayerDeath())
                .AddTo(_disposer);
        }

        private void OnPlayerDeath()
        {
            // Could add death animation, game over screen, etc.
            //Debug.Log("Player death handled in presenter");
        }
    }
}