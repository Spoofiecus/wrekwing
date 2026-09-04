using UnityEngine;

namespace WreckWing.Gameplay
{
    /// <summary>
    /// ScriptableObject containing plane configuration data.
    /// </summary>
    [CreateAssetMenu(fileName = "PlaneData", menuName = "WreckWing/PlaneData")]
    public class PlaneData : ScriptableObject
    {
        [Header("Identity")]
        public string planeName;
        public string description;
        public Sprite icon;
        public int tier = 1;

        [Header("Flight Stats")]
        [Tooltip("Base speed of the plane")]
        public float baseSpeed = 30f;

        [Tooltip("Maximum horizontal movement speed")]
        public float maxHorizontalSpeed = 50f;

        [Tooltip("Maneuverability (higher = more responsive)")]
        [Range(0.1f, 2f)]
        public float maneuverability = 1f;

        [Tooltip("Weight affects impact force")]
        public float weight = 1f;

        [Tooltip("Pitch control sensitivity")]
        [Range(0.1f, 2f)]
        public float pitchSensitivity = 1f;

        [Header("Damage Stats")]
        [Tooltip("Damage multiplier on impact")]
        public float damageMultiplier = 1f;

        [Tooltip("Explosion radius on crash")]
        public float explosionRadius = 5f;

        [Tooltip("Fire chance on crash (0-1)")]
        [Range(0f, 1f)]
        public float fireChance = 0.3f;

        [Header("Fuel")]
        [Tooltip("Maximum fuel capacity")]
        public float maxFuel = 100f;

        [Tooltip("Fuel consumption per second")]
        public float fuelConsumptionRate = 5f;

        [Header("Visual")]
        [Tooltip("Plane prefab")]
        public GameObject prefab;

        [Tooltip("Custom trail effect")]
        public GameObject trailEffect;

        [Header("Unlock Requirements")]
        [Tooltip("Cost in coins to unlock")]
        public int unlockCost = 0;

        [Tooltip("Required player level to unlock")]
        public int requiredLevel = 1;

        [Tooltip("Is this plane unlocked by default")]
        public bool unlockedByDefault = false;

        [Header("Monetization")]
        [Tooltip("Is this a premium plane (requires gems or battle pass)")]
        public bool isPremium = false;

        [Tooltip("Gem cost for premium planes")]
        public int gemCost = 0;

        [Header("Audio")]
        [Tooltip("Engine sound clip")]
        public AudioClip engineSound;

        [Tooltip("Crash sound override")]
        public AudioClip crashSound;

        /// <summary>
        /// Calculate impact force based on speed and weight.
        /// </summary>
        public float CalculateImpactForce(float speed)
        {
            return speed * weight * damageMultiplier;
        }

        /// <summary>
        /// Get the display unlock cost string.
        /// </summary>
        public string GetUnlockCostString()
        {
            if (unlockedByDefault) return "FREE";
            if (isPremium) return $"{gemCost} Gems";
            return $"{unlockCost:N0} Coins";
        }
    }
}
