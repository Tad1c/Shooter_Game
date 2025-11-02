using UnityEngine;
using TMPro;

namespace Scripts {
	public class HUDView : MonoBehaviour {
		[Header("Kill Counter")]
		[SerializeField] private TextMeshProUGUI _killCountText;

		public void UpdateKillCount(int killCount) {
			if (_killCountText != null) {
				_killCountText.SetText($"Kills: {killCount}");
			}
		}
	}
}

