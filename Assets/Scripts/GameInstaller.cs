using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    public class GameInstaller : MonoInstaller<GameInstaller>
    {
        [Header("Player")]
        [SerializeField] private JoystickView _joyStick;
        [SerializeField] private PlayerView _playerView;

        [Header("Level Configuration")]
        [SerializeField] private LevelConfiguration _levelConfiguration;

        [Header("Camera")]
        [SerializeField] private Camera _mainCamera;

        private readonly CompositeDisposable _disposer = new();

        private void OnDestroy()
        {
            _disposer.Dispose();
        }

        public override void InstallBindings()
        {
            // Core
            Container.BindInterfacesAndSelfTo<CompositeDisposable>().AsSingle();

            // Camera
            if (_mainCamera == null)
                _mainCamera = Camera.main;
            Container.BindInstance(_mainCamera).AsSingle();

            // Player
            Container.BindInstance(_playerView);
            Container.BindInstance(_joyStick);
            Container.BindInterfacesTo<PlayerModel>().AsSingle();
            Container.BindInterfacesTo<PlayerPresenter>().AsSingle().NonLazy();

            // Level Configuration
            Container.BindInstance(_levelConfiguration).AsSingle();

            // Enemy System
            Container.BindFactory<IEnemyModel, EnemyView, IPlayerModel, PlayerView, EnemyPresenter, EnemyPresenter.Factory>();
            Container.BindInterfacesAndSelfTo<EnemySpawnService>().AsSingle();
            Container.BindInterfacesAndSelfTo<WaveManager>().AsSingle().NonLazy();
        }
    }
}