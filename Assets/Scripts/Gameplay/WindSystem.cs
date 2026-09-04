using UnityEngine;

namespace WreckWing.Gameplay
{
    /// <summary>
    /// Singleton wind system that generates dynamic wind gusts affecting plane flight.
    /// Uses Perlin noise for smooth, natural wind transitions.
    /// </summary>
    public class WindSystem : MonoBehaviour
    {
        public static WindSystem Instance { get; private set; }

        [Header("Wind Settings")]
        [SerializeField] private float windIntensity = 5f;
        [SerializeField] private float windChangeInterval = 3f;

        private float baseNoiseOffsetX;
        private float baseNoiseOffsetY;
        private float gustNoiseOffset;
        private float timer;
        private Vector3 currentWindDirection;
        private float currentWindStrength;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            baseNoiseOffsetX = Random.Range(0f, 100f);
            baseNoiseOffsetY = Random.Range(0f, 100f);
            gustNoiseOffset = Random.Range(0f, 100f);
            currentWindDirection = Random.insideUnitSphere;
            currentWindDirection.y = 0f;
            currentWindDirection.Normalize();
        }

        private void Update()
        {
            timer += Time.deltaTime;
            float noiseTime = Time.time;

            // Smooth wind direction via Perlin noise
            float noiseX = Mathf.PerlinNoise(baseNoiseOffsetX + noiseTime * 0.2f, 0f);
            float noiseY = Mathf.PerlinNoise(0f, baseNoiseOffsetY + noiseTime * 0.2f);
            float angle = (noiseX + noiseY) * 180f - 180f;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 targetDirection = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)).normalized;

            // Gust strength variation
            float gustNoise = Mathf.PerlinNoise(gustNoiseOffset + noiseTime * 0.5f, 0f);
            float targetStrength = windIntensity * (0.5f + gustNoise * 1.5f);

            // Smooth interpolation
            float lerpSpeed = Mathf.Clamp01(Time.deltaTime / windChangeInterval);
            currentWindDirection = Vector3.Slerp(currentWindDirection, targetDirection, lerpSpeed).normalized;
            currentWindStrength = Mathf.Lerp(currentWindStrength, targetStrength, lerpSpeed);
        }

        /// <summary>
        /// Returns the wind force vector at the given world position.
        /// Combines base wind with a position-based gust variation.
        /// </summary>
        /// <param name="position">World space position to sample wind at.</param>
        /// <returns>Wind force vector in world space.</returns>
        public Vector3 GetWindForce(Vector3 position)
        {
            float heightFactor = 1f + position.y * 0.01f;
            float localGust = Mathf.PerlinNoise(position.x * 0.05f + Time.time * 0.3f, position.z * 0.05f);
            float gustMultiplier = 0.7f + localGust * 0.6f;
            return currentWindDirection * currentWindStrength * heightFactor * gustMultiplier;
        }
    }
}