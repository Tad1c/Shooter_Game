using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts {
	public class EnemyPresenter : IInitializable, IDisposable {
		private readonly IEnemyModel _model;
		private readonly EnemyView _view;
		private readonly IPlayerModel _playerModel;
		private readonly PlayerView _playerView;
		private readonly CompositeDisposable _disposer = new();

		public IEnemyModel Model => _model;
		public EnemyView View => _view;

		public EnemyPresenter(
			IEnemyModel model,
			EnemyView view,
			IPlayerModel playerModel,
			PlayerView playerView) {
			_model = model;
			_view = view;
			_playerModel = playerModel;
			_playerView = playerView;
		}

		public void Initialize() {
			_view.SetModel(_model);
			
			_model.TargetPosition
				.Subscribe(_view.SetTargetPosition)
				.AddTo(_disposer);

			_model.CurrentHealth
				.Select(health => health / _model.Data.MaxHealth)
				.Subscribe(_view.UpdateHealth)
				.AddTo(_disposer);

			_model.IsAlive
				.Where(isAlive => !isAlive)
				.Subscribe(_ => OnDeath())
				.AddTo(_disposer);

			Observable.EveryUpdate()
				.Where(_ => _model.IsAlive.Value && _playerModel.IsAlive.Value)
				.Subscribe(_ => {
					_view.UpdateMovement();
					_model.UpdateAttackCooldown(Time.deltaTime);
					CheckAttack();
				})
				.AddTo(_disposer);
		}

		private void CheckAttack() {
			float distanceToPlayer = Vector2.Distance(_view.Position, _playerView.Position);

			if (distanceToPlayer <= _model.Data.AttackRange && _model.CanAttack()) {
				_model.Attack();
				_playerModel.TakeDamage(_model.Data.Damage);

				Debug.Log($"{_model.Data.EnemyName} attacked player for {_model.Data.Damage} damage!");
			}
		}

		private void OnDeath() {
			_view.PlayDeathAnimation(() => _view.Deactivate());
		}

		public void Dispose() {
			_disposer?.Dispose();
		}

		public class Factory : PlaceholderFactory<IEnemyModel, EnemyView, IPlayerModel, PlayerView, EnemyPresenter> {
		}
	}
}

