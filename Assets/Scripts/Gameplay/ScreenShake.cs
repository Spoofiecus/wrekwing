using System;
using System.Collections;
using UnityEngine;

namespace WreckWing.Gameplay
{
    public class ScreenShake : MonoBehaviour
    {
        public static ScreenShake Instance { get; private set; }

        [Header("Shake Settings")]
        [SerializeField] private float defaultIntensity = 0.5f;
        [SerializeField] private float defaultDuration = 0.3f;
        [SerializeField] private float dampingSpeed = 1f;

        public event Action OnShakeStart;
        public event Action OnShakeEnd;

        private Coroutine activeShake;
        private Transform cameraTransform;
        private Vector3 originalLocalPosition;
        private bool isShaking;

        public bool IsShaking => isShaking;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            cameraTransform = Camera.main?.transform;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Shake(float duration, float intensity)
        {
            if (duration <= 0f) return;
            if (activeShake != null) StopCoroutine(activeShake);
            activeShake = StartCoroutine(ShakeCoroutine(duration, intensity));
        }

        public void Shake()
        {
            Shake(defaultDuration, defaultIntensity);
        }

        public void StopShake()
        {
            if (activeShake != null)
            {
                StopCoroutine(activeShake);
                activeShake = null;
            }
            if (isShaking)
            {
                ResetShakeState();
                OnShakeEnd?.Invoke();
            }
        }

        private IEnumerator ShakeCoroutine(float duration, float intensity)
        {
            isShaking = true;
            OnShakeStart?.Invoke();

            if (cameraTransform != null)
                originalLocalPosition = cameraTransform.localPosition;

            float elapsed = 0f;
            float currentIntensity = intensity;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                currentIntensity = Mathf.Lerp(intensity, 0f, elapsed / duration);

                if (cameraTransform != null)
                {
                    Vector3 offset = UnityEngine.Random.insideUnitSphere * currentIntensity;
                    offset.z = 0f;
                    cameraTransform.localPosition = originalLocalPosition + offset;
                }
                yield return null;
            }

            if (cameraTransform != null)
                cameraTransform.localPosition = originalLocalPosition;

            isShaking = false;
            activeShake = null;
            OnShakeEnd?.Invoke();
        }

        private void ResetShakeState()
        {
            if (cameraTransform != null)
                cameraTransform.localPosition = originalLocalPosition;
            isShaking = false;
        }
    }
}