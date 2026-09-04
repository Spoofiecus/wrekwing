using System;
using System.Collections;
using UnityEngine;

namespace WreckWing.Gameplay
{
    /// <summary>
    /// Controls slow-motion camera effects for dramatic gameplay moments.
    /// </summary>
    public class SlowMotionEffect : MonoBehaviour
    {
        public static SlowMotionEffect Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float transitionSpeed = 5f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public event Action OnSlowMotionStart;
        public event Action OnSlowMotionEnd;

        private Coroutine activeCoroutine;
        private float normalTimeScale = 1f;
        private bool isInSlowMotion;

        public bool IsInSlowMotion => isInSlowMotion;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// Triggers a slow-motion effect for the specified duration.
        /// </summary>
        public void TriggerSlowMotion(float duration, float timeScale)
        {
            if (duration <= 0f || timeScale < 0f || timeScale > 1f)
            {
                Debug.LogWarning("[SlowMotionEffect] Invalid parameters.");
                return;
            }

            if (activeCoroutine != null)
                StopCoroutine(activeCoroutine);

            activeCoroutine = StartCoroutine(SlowMotionCoroutine(duration, timeScale));
        }

        /// <summary>
        /// Immediately resets time scale to normal.
        /// </summary>
        public void ResetTimeScale()
        {
            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
                activeCoroutine = null;
            }

            Time.timeScale = normalTimeScale;
            isInSlowMotion = false;
            OnSlowMotionEnd?.Invoke();
        }

        private IEnumerator SlowMotionCoroutine(float duration, float timeScale)
        {
            isInSlowMotion = true;
            OnSlowMotionStart?.Invoke();

            // Transition into slow motion
            float elapsed = 0f;
            float startScale = Time.timeScale;

            while (elapsed < 0.2f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = transitionCurve.Evaluate(elapsed / 0.2f);
                Time.timeScale = Mathf.Lerp(startScale, timeScale, t);
                yield return null;
            }

            Time.timeScale = timeScale;

            // Hold slow motion
            yield return new WaitForSecondsRealtime(duration);

            // Transition back to normal
            elapsed = 0f;
            startScale = Time.timeScale;

            while (elapsed < 0.3f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = transitionCurve.Evaluate(elapsed / 0.3f);
                Time.timeScale = Mathf.Lerp(startScale, normalTimeScale, t);
                yield return null;
            }

            Time.timeScale = normalTimeScale;
            isInSlowMotion = false;
            activeCoroutine = null;
            OnSlowMotionEnd?.Invoke();
        }
    }
}