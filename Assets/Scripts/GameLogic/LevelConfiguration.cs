using System.Collections.Generic;
using UnityEngine;

namespace Scripts {
	[CreateAssetMenu(fileName = "LevelConfiguration", menuName = "Game/Level Configuration", order = 2)]
	public class LevelConfiguration : ScriptableObject {
		[Header("Level Info")]
		[SerializeField] private string _levelName = "Level 1";
		[SerializeField, TextArea(3, 6)] private string _levelDescription;

		[Header("Wave Configuration")]
		[SerializeField] private List<WaveData> _waves = new();
		[SerializeField] private float _delayBetweenWaves = 5f;

		[Header("Spawn Settings")]
		[SerializeField, Range(1f, 10f)] private float _offScreenSpawnDistance = 2f;
		[SerializeField, Range(1f, 20f)] private float _despawnDistance = 15f;

		public string LevelName => _levelName;
		public string LevelDescription => _levelDescription;
		public List<WaveData> Waves => _waves;
		public float DelayBetweenWaves => _delayBetweenWaves;
		public float OffScreenSpawnDistance => _offScreenSpawnDistance;
		public float DespawnDistance => _despawnDistance;

		public int TotalWaves => _waves.Count;

		public WaveData GetWave(int index) {
			if (index < 0 || index >= _waves.Count) {
				return null;
			}
			
			return _waves[index];
		}
	}
}
