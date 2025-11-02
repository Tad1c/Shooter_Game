using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts {
	public class GameController : MonoBehaviour {
		private IWaveManager _waveManager;
		private readonly CompositeDisposable _disposer = new();

		[Inject]
		public void Construct(IWaveManager waveManager) {
			_waveManager = waveManager;
		}

		private void Start() {
			_waveManager.OnWaveStarted
				.Subscribe(wave => Debug.Log($"Wave Started: {wave.WaveName}"))
				.AddTo(_disposer);

			_waveManager.OnWaveCompleted
				.Subscribe(wave => Debug.Log($"Wave Completed: {wave.WaveName}"))
				.AddTo(_disposer);

			_waveManager.OnAllWavesCompleted
				.Subscribe(_ => Debug.Log("All Waves Completed! Level finished!"))
				.AddTo(_disposer);

			_waveManager.StartLevel();
		}

		private void OnDestroy() {
			_disposer?.Dispose();
		}
	}
}
