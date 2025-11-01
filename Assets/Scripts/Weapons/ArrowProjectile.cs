using UnityEngine;

namespace Scripts.Weapons
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class ArrowProjectile : MonoBehaviour
    {
        private const float ViewportPadding = 0.002f;

        [SerializeField] private Rigidbody2D _rigidbody;

        private Camera _camera;
        private int _maxRicochets;
        private int _ricochetCount;
        private float _lifetime;
        private float _spawnTime;
        private float _viewportDepth;

        private void Awake()
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody2D>();
            }
        }

        public void Initialize(Vector2 direction, float speed, int maxRicochets, Camera camera, float lifetime)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                direction = Vector2.right;
            }

            _camera = camera == null ? Camera.main : camera;
            _maxRicochets = maxRicochets;
            _ricochetCount = 0;
            _lifetime = lifetime;
            _spawnTime = Time.time;

            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody2D>();
            }

            _rigidbody.velocity = direction.normalized * speed;
            AlignWithVelocity();

            if (_camera != null)
            {
                var viewport = _camera.WorldToViewportPoint(transform.position);
                _viewportDepth = viewport.z;
            }
        }

        private void FixedUpdate()
        {
            if (_lifetime > 0f && Time.time >= _spawnTime + _lifetime)
            {
                Destroy(gameObject);
                return;
            }

            if (_camera == null || _rigidbody == null)
            {
                return;
            }

            HandleRicochet();
        }

        private void HandleRicochet()
        {
            var viewport = _camera.WorldToViewportPoint(transform.position);
            var velocity = _rigidbody.velocity;
            var bounced = false;

            if (viewport.x < 0f && velocity.x < 0f)
            {
                viewport.x = ViewportPadding;
                velocity.x = -velocity.x;
                bounced = true;
            }
            else if (viewport.x > 1f && velocity.x > 0f)
            {
                viewport.x = 1f - ViewportPadding;
                velocity.x = -velocity.x;
                bounced = true;
            }

            if (viewport.y < 0f && velocity.y < 0f)
            {
                viewport.y = ViewportPadding;
                velocity.y = -velocity.y;
                bounced = true;
            }
            else if (viewport.y > 1f && velocity.y > 0f)
            {
                viewport.y = 1f - ViewportPadding;
                velocity.y = -velocity.y;
                bounced = true;
            }

            if (!bounced)
            {
                return;
            }

            viewport.z = viewport.z <= 0f ? (_viewportDepth == 0f ? 1f : _viewportDepth) : viewport.z;
            var clampedViewport = new Vector3(Mathf.Clamp01(viewport.x), Mathf.Clamp01(viewport.y), viewport.z);
            var worldPos = _camera.ViewportToWorldPoint(clampedViewport);
            worldPos.z = transform.position.z;

            transform.position = worldPos;
            _rigidbody.velocity = velocity;
            _ricochetCount++;
            AlignWithVelocity();

            if (_maxRicochets >= 0 && _ricochetCount > _maxRicochets)
            {
                Destroy(gameObject);
            }
        }

        private void AlignWithVelocity()
        {
            if (_rigidbody == null)
            {
                return;
            }

            var velocity = _rigidbody.velocity;
            if (velocity.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            var angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}

