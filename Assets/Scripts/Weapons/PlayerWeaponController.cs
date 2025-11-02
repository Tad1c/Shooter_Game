using UnityEngine;
using Zenject;

namespace Scripts.Weapons
{
    public class PlayerWeaponController : MonoBehaviour
    {
        [Header("Weapon Setup")]
        [SerializeField] private WeaponBehaviour _equippedWeapon;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private WeaponData _weaponData;

        [Header("Aiming")]
        [SerializeField] private Camera _camera;
        [SerializeField] private bool _useMouseAim = true;
        [SerializeField, Range(0f, 1f)] private float _aimDeadZone = 0.05f;

        private float _nextFireTime;
        private Vector2 _externalAimDirection = Vector2.zero;
        private Vector2 _mouseAimDirection = Vector2.right;
        private DiContainer _container;
        private WeaponData _injectedWeaponData;

        public WeaponBehaviour EquippedWeapon => _equippedWeapon;

        [Inject]
        public void Construct(DiContainer container, [InjectOptional] WeaponData weaponData = null)
        {
            _container = container;
            _injectedWeaponData = weaponData;
        }

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        private void Start()
        {
            InitializeEquippedWeapon();
        }

        private void Update()
        {
            if (_useMouseAim)
            {
                UpdateMouseAimDirection();
            }

            if (Input.GetMouseButtonDown(0))
            {
                TryFire();
            }
        }

        public void SetAimDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude <= _aimDeadZone * _aimDeadZone)
            {
                _externalAimDirection = Vector2.zero;
                return;
            }

            _externalAimDirection = direction.normalized;
        }

        public bool TryFire()
        {
            if (_equippedWeapon == null)
            {
                Debug.LogWarning($"{name} attempted to fire without an equipped weapon.");
                return false;
            }

            if (Time.time < _nextFireTime)
            {
                return false;
            }

            var direction = GetAimDirection();
            if (direction.sqrMagnitude <= _aimDeadZone * _aimDeadZone)
            {
                return false;
            }

            _equippedWeapon.Fire(direction.normalized);
            _nextFireTime = Time.time + Mathf.Max(0f, _equippedWeapon.Cooldown);
            return true;
        }

        public void EquipWeapon(WeaponBehaviour newWeapon, WeaponData weaponData = null)
        {
            if (newWeapon == null)
            {
                Debug.LogWarning("Tried to equip a null weapon.");
                return;
            }

            if (newWeapon.transform.parent != transform)
            {
                newWeapon.transform.SetParent(transform, false);
            }

            _equippedWeapon = newWeapon;
            if (weaponData != null)
            {
                _weaponData = weaponData;
            }

            InitializeEquippedWeapon();
        }

        public WeaponBehaviour EquipWeaponFromPrefab(WeaponBehaviour weaponPrefab, WeaponData weaponData = null)
        {
            if (weaponPrefab == null)
            {
                Debug.LogWarning("Weapon prefab is null; cannot equip.");
                return null;
            }

            var instance = Instantiate(weaponPrefab, transform);
            EquipWeapon(instance, weaponData);
            return instance;
        }

        private void InitializeEquippedWeapon()
        {
            if (_equippedWeapon == null)
            {
                return;
            }

            _container?.Inject(_equippedWeapon);

            var data = GetActiveWeaponData();
            if (data != null)
            {
                _equippedWeapon.SetData(data);
            }

            var context = new WeaponRuntimeContext(transform, _firePoint, _camera, _container);
            _equippedWeapon.Initialize(context);
            _nextFireTime = Time.time;
        }

        private Vector2 GetAimDirection()
        {
            if (_externalAimDirection.sqrMagnitude > _aimDeadZone * _aimDeadZone)
            {
                return _externalAimDirection;
            }

            if (_mouseAimDirection.sqrMagnitude > _aimDeadZone * _aimDeadZone)
            {
                return _mouseAimDirection;
            }

            return transform.right;
        }

        private void UpdateMouseAimDirection()
        {
            if (_camera == null)
            {
                return;
            }

            var reference = _firePoint == null ? transform : _firePoint;
            var mouseWorld = _camera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = reference.position.z;

            var direction = mouseWorld - reference.position;
            direction.z = 0f;

            var planarDirection = new Vector2(direction.x, direction.y);
            if (planarDirection.sqrMagnitude > _aimDeadZone * _aimDeadZone)
            {
                _mouseAimDirection = planarDirection.normalized;
            }
        }

        private WeaponData GetActiveWeaponData()
        {
            if (_injectedWeaponData != null)
            {
                return _injectedWeaponData;
            }

            return _weaponData;
        }
    }
}

