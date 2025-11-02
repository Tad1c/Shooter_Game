using UnityEngine;

namespace Scripts {
	[RequireComponent(typeof(Rigidbody2D))]
	public class ArrowProjectile : MonoBehaviour, IProjectile {
		[Header("Ricochet Settings")]
		[SerializeField] private int _maxRicochets = 5;
		[SerializeField] private float _lifeTime = 10f;
		[SerializeField] private LayerMask _collisionMask;

		[Header("Visual Settings")]
		[SerializeField] private SpriteRenderer _spriteRenderer;
		[SerializeField] private TrailRenderer _trailRenderer;

		private Rigidbody2D _rigidbody;
		private Camera _mainCamera;
		private int _ricochetCount;
		private float _currentLifeTime;
		private float _speed;
		private bool _isActive;

		public float Damage { get; set; }

		private void Awake() {
			if (_rigidbody == null) {
				_rigidbody = GetComponent<Rigidbody2D>();
			}
			
			_rigidbody.gravityScale = 0;
			_mainCamera = Camera.main;
		}

		public void Launch(Vector2 startPosition, Vector2 direction, float speed) {
			transform.position = startPosition;
			_speed = speed;
			_rigidbody.velocity = direction.normalized * speed;
			_isActive = true;
			_ricochetCount = 0;
			_currentLifeTime = 0f;

			UpdateRotation();
		}

		private void Update() {
			if (!_isActive) {
				return;
			}

			_currentLifeTime += Time.deltaTime;

			if (_currentLifeTime >= _lifeTime || _ricochetCount >= _maxRicochets) {
				Destroy();
				return;
			}

			UpdateRotation();
		}

		private void FixedUpdate() {
			if (!_isActive || _mainCamera == null) {
				return;
			}

			CheckCameraBoundaries();
		}

		private void CheckCameraBoundaries() {
			Vector2 position = transform.position;
			Vector2 velocity = _rigidbody.velocity;

			float cameraHeight = _mainCamera.orthographicSize * 2;
			float cameraWidth = cameraHeight * _mainCamera.aspect;

			Vector2 cameraPos = _mainCamera.transform.position;
			float left = cameraPos.x - cameraWidth / 2;
			float right = cameraPos.x + cameraWidth / 2;
			float bottom = cameraPos.y - cameraHeight / 2;
			float top = cameraPos.y + cameraHeight / 2;

			bool reflected = false;
			Vector2 newVelocity = velocity;

			if (position.x <= left && velocity.x < 0) {
				newVelocity.x = Mathf.Abs(velocity.x);
				position.x = left + 0.01f;
				reflected = true;
			}
			else if (position.x >= right && velocity.x > 0) {
				newVelocity.x = -Mathf.Abs(velocity.x);
				position.x = right - 0.01f;
				reflected = true;
			}

			if (position.y <= bottom && velocity.y < 0) {
				newVelocity.y = Mathf.Abs(velocity.y);
				position.y = bottom + 0.01f;
				reflected = true;
			}
			else if (position.y >= top && velocity.y > 0) {
				newVelocity.y = -Mathf.Abs(velocity.y);
				position.y = top - 0.01f;
				reflected = true;
			}

			if (reflected) {
				_ricochetCount++;
				transform.position = position;
				_rigidbody.velocity = newVelocity.normalized * _speed;
			}
		}

		private void UpdateRotation() {
			if (_rigidbody.velocity.magnitude > 0.1f) {
				float angle = Mathf.Atan2(_rigidbody.velocity.y, _rigidbody.velocity.x) * Mathf.Rad2Deg;
				transform.rotation = Quaternion.Euler(0, 0, angle);
			}
		}

		private void OnTriggerEnter2D(Collider2D other) {
			if (!_isActive) {
				return;
			}

			var enemy = other.GetComponent<EnemyView>();
			if (enemy != null && enemy.Model != null) {
				enemy.Model.TakeDamage(Damage);
			}
		}

		public void Destroy() {
			_isActive = false;
			_rigidbody.velocity = Vector2.zero;
			Destroy(gameObject);
		}
	}
}
