using UniRx;
using Zenject;

namespace Scripts {
	public class HUDPresenter : IInitializable {
		private readonly HUDView _view;
		private readonly IGameStatsModel _statsModel;
		private readonly CompositeDisposable _disposer;

		public HUDPresenter(HUDView view, IGameStatsModel statsModel, CompositeDisposable disposer) {
			_view = view;
			_statsModel = statsModel;
			_disposer = disposer;
		}

		public void Initialize() {
			_statsModel.TotalEnemiesKilled
				.Subscribe(_view.UpdateKillCount)
				.AddTo(_disposer);
		}
	}
}

