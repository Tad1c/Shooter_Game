using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts {
	[RequireComponent(typeof(Collider2D))]
	public class EnemyView : MonoBehaviour {
		[SerializeField] private SpriteRenderer _spriteRenderer;
		[SerializeField] private Slider _healthBar;
		[SerializeField] private Transform _visualRoot;
		[SerializeField] private Collider2D _collider2D;

		private float _moveSpeed;
		private Vector2 _targetPosition;
		private bool _isActive;
		private IEnemyModel _model;

		public Vector2 Position => transform.position;
		public IEnemyModel Model => _model;

		public void SetModel(IEnemyModel model) {
			_model = model;
		}

		private void Awake() {
			if (_collider2D == null) {
				_collider2D = GetComponent<Collider2D>();
			}

			if (_collider2D != null) {
				_collider2D.isTrigger = true;
			}

			if (!gameObject.CompareTag("Enemy")) {
				gameObject.tag = "Enemy";
			}
		}

		public void Initialize(EnemyData data) {
			_moveSpeed = data.MoveSpeed;

			if (_spriteRenderer != null) {
				_spriteRenderer.color = data.TintColor;
			}

			if (_visualRoot != null) {
				_visualRoot.localScale = Vector3.one * data.Scale;
			}

			_isActive = true;
		}

		public void SetPosition(Vector2 position) {
			transform.position = position;
		}

		public void SetTargetPosition(Vector2 target) {
			_targetPosition = target;
		}

		public void UpdateMovement() {
			if (!_isActive) {
				return;
			}

			Vector2 currentPosition = transform.position;
			Vector2 direction = (_targetPosition - currentPosition).normalized;

			Vector2 newPosition = Vector2.MoveTowards(currentPosition, _targetPosition,
				_moveSpeed * Time.deltaTime);

			transform.position = newPosition;

			if (_visualRoot != null && direction.x != 0) {
				var scale = _visualRoot.localScale;
				scale.x = Mathf.Abs(scale.x) * (direction.x > 0 ? 1 : -1);
				_visualRoot.localScale = scale;
			}
		}

		public void UpdateHealth(float normalizedHealth) {
			if (_healthBar != null) {
				_healthBar.DOValue(normalizedHealth, 0.2f).SetEase(Ease.OutSine);
			}
		}

		public void PlayDeathAnimation(System.Action onComplete) {
			_isActive = false;

			if (_visualRoot != null) {
				_visualRoot.DOScale(0f, 0.3f)
					.SetEase(Ease.InBack)
					.OnComplete(() => onComplete?.Invoke());
			}
			else {
				onComplete?.Invoke();
			}
		}

		public void Deactivate() {
			_isActive = false;
			gameObject.SetActive(false);
		}
	}
}
