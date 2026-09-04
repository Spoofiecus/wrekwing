using UnityEngine;
using WreckWing.Core;

namespace WreckWing.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlaneController : MonoBehaviour
    {
        [SerializeField] private PlaneData planeData;
        [SerializeField] private ControlScheme controlScheme = ControlScheme.Tilt;
        [SerializeField] private float tiltSensitivity = 2f;
        [SerializeField] private float gravityMultiplier = 1f;
        [SerializeField] private float throttleMultiplier = 1.5f;
        [SerializeField] private Transform modelTransform;

        private Rigidbody _rigidbody;
        private float _currentSpeed;
        private float _currentFuel;
        private bool _isThrottling;
        private bool _isCrashed;
        private float _pitchAngle;
        private float _rollAngle;
        private float _tiltInput;
        private float _pitchInput;

        public System.Action OnCrashed;
        public System.Action<float> OnFuelChanged;

        public PlaneData Data => planeData;
        public float CurrentSpeed => _currentSpeed;
        public float CurrentFuel => _currentFuel;
        public float FuelPercent => _currentFuel / planeData.maxFuel;
        public bool IsCrashed => _isCrashed;
        public bool IsThrottling => _isThrottling;
        public Vector3 Velocity => _rigidbody.velocity;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            _rigidbody.drag = 0.01f;
            _rigidbody.angularDrag = 0.5f;
        }

        private void Start()
        {
            _currentFuel = planeData.maxFuel;
            _currentSpeed = planeData.baseSpeed;
        }

        private void Update()
        {
            if (_isCrashed) return;
            GatherInput();
            UpdateFuel();
            UpdateVisuals();
        }

        private void FixedUpdate()
        {
            if (_isCrashed) return;
            ApplyFlightPhysics();
            ApplyGravity();
            ClampVelocity();
        }

        private void GatherInput()
        {
            _tiltInput = 0f;
            _pitchInput = 0f;
            _isThrottling = false;

            switch (controlScheme)
            {
                case ControlScheme.Tilt:
                    _tiltInput = Mathf.Clamp(Input.acceleration.x * tiltSensitivity, -1f, 1f);
                    _pitchInput = Mathf.Clamp(Input.acceleration.z * planeData.pitchSensitivity, -0.5f, 0.5f);
                    break;
                case ControlScheme.Joystick:
                    _tiltInput = Input.GetAxis("Horizontal");
                    _pitchInput = Input.GetAxis("Vertical");
                    break;
                case ControlScheme.TapZones:
                    if (Input.touchCount > 0)
                        _tiltInput = Input.GetTouch(0).position.x < Screen.width / 2f ? -1f : 1f;
                    break;
            }

            _isThrottling = Input.GetKey(KeyCode.Space) || Input.touchCount >= 2;
        }

        private void ApplyFlightPhysics()
        {
            float targetHorizontalSpeed = planeData.maxHorizontalSpeed * _tiltInput * planeData.maneuverability;
            float speedMultiplier = _isThrottling ? throttleMultiplier : 1f;
            _currentSpeed = planeData.baseSpeed * speedMultiplier;

            Vector3 targetVelocity = new Vector3(targetHorizontalSpeed, -_currentSpeed * gravityMultiplier, 0f);
            _rigidbody.velocity = targetVelocity;

            _pitchAngle = Mathf.Lerp(_pitchAngle, _pitchInput * 30f, Time.fixedDeltaTime * 5f);
            _rollAngle = Mathf.Lerp(_rollAngle, -_tiltInput * 45f, Time.fixedDeltaTime * 5f);
        }

        private void ApplyGravity()
        {
            _rigidbody.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
        }

        private void ClampVelocity()
        {
            float maxSpeed = planeData.baseSpeed * throttleMultiplier * 1.5f;
            if (_rigidbody.velocity.magnitude > maxSpeed)
                _rigidbody.velocity = _rigidbody.velocity.normalized * maxSpeed;
        }

        private void UpdateFuel()
        {
            if (_currentFuel <= 0f) return;
            float consumption = planeData.fuelConsumptionRate * Time.deltaTime;
            if (_isThrottling) consumption *= 2f;
            _currentFuel = Mathf.Max(0f, _currentFuel - consumption);
            OnFuelChanged?.Invoke(FuelPercent);
        }

        private void UpdateVisuals()
        {
            if (modelTransform != null)
            {
                Quaternion targetRotation = Quaternion.Euler(_pitchAngle, 0f, _rollAngle);
                modelTransform.localRotation = Quaternion.Slerp(modelTransform.localRotation, targetRotation, Time.deltaTime * 10f);
            }
        }

        public void OnCrash(Collision collision)
        {
            if (_isCrashed) return;
            _isCrashed = true;
            _rigidbody.isKinematic = true;
            OnCrashed?.Invoke();
        }

        public void SetControlScheme(ControlScheme scheme) => controlScheme = scheme;

        public void SetPlaneData(PlaneData data)
        {
            planeData = data;
            _currentFuel = planeData.maxFuel;
            _currentSpeed = planeData.baseSpeed;
        }

        public float GetAltitude() => transform.position.y;

        public float GetImpactForce() => planeData.CalculateImpactForce(_currentSpeed);

        private void OnCollisionEnter(Collision collision)
        {
            if (!_isCrashed && collision.gameObject.CompareTag("Stadium"))
                OnCrash(collision);
        }
    }

    public enum ControlScheme
    {
        Tilt,
        Swipe,
        Joystick,
        TapZones
    }
}
