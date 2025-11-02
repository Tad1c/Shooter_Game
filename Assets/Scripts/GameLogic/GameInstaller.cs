using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts {
	public class GameInstaller : MonoInstaller<GameInstaller> {
		[Header("Player")]
		[SerializeField] private JoystickView _joyStick;
		[SerializeField] private PlayerView _playerView;

		[Header("HUD")]
		[SerializeField] private HUDView _hudView;

		[Header("Level Configuration")]
		[SerializeField] private LevelConfiguration _levelConfiguration;

		[Header("Weapon System")]
		[SerializeField] private WeaponData _crossbowData;
		[SerializeField] private WeaponController _weaponController;

		[Header("Camera")]
		[SerializeField] private Camera _mainCamera;

		private readonly CompositeDisposable _disposer = new();

		private void OnDestroy() {
			_disposer.Dispose();
		}

		public override void InstallBindings() {
			Container.BindInterfacesAndSelfTo<CompositeDisposable>().AsSingle();
			
			if (_mainCamera == null) {
				_mainCamera = Camera.main;
			}
			
			Container.BindInstance(_mainCamera).AsSingle();

			Container.BindInstance(_playerView);
			Container.BindInstance(_joyStick);
			Container.BindInterfacesTo<PlayerModel>().AsSingle();
			Container.BindInterfacesTo<PlayerPresenter>().AsSingle().NonLazy();

			if (_hudView != null) {
				Container.BindInstance(_hudView);
				Container.BindInterfacesTo<GameStatsModel>().AsSingle();
				Container.BindInterfacesTo<HUDPresenter>().AsSingle().NonLazy();
			}
			
			if (_weaponController != null && _crossbowData != null) {
				Container.BindInstance(_weaponController);
				Container.BindInstance(_crossbowData);

				var crossbow = new CrossbowWeapon(_crossbowData, Container);
				Container.Bind<IWeapon>().FromInstance(crossbow).AsSingle();
				
				_weaponController.SetWeapon(crossbow);
			}
			
			Container.BindInstance(_levelConfiguration).AsSingle();

			Container.BindFactory<IEnemyModel, EnemyView, IPlayerModel, PlayerView, EnemyPresenter, EnemyPresenter.Factory>();
			Container.BindInterfacesAndSelfTo<EnemySpawnService>().AsSingle();
			Container.BindInterfacesAndSelfTo<WaveManager>().AsSingle().NonLazy();
		}
	}
}
