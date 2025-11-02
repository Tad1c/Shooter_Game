using UnityEngine;

namespace Scripts {
	public interface IWeapon {
		bool TryFire(Vector2 firePosition, Vector2 direction);
		void Update(float deltaTime);
		float CurrentCooldown { get; }
		float MaxCooldown { get; }
	}
}
