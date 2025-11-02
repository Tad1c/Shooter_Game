using UnityEngine;

namespace Scripts {
	public interface IProjectile {
		void Launch(Vector2 startPosition, Vector2 direction, float speed);
		void Destroy();
	}
}
