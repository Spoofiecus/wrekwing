using UnityEngine;

namespace WreckWing.Core
{
    /// <summary>
    /// ScriptableObject containing all configurable game settings.
    /// </summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "WreckWing/GameSettings")]
    public class GameSettings : ScriptableObject
    {
        [Header("Flight Settings")]
        [Tooltip("Base gravity applied to planes")]
        public float gravity = -9.81f;

        [Tooltip("Base descent rate multiplier")]
        public float descentRate = 1.0f;

        [Tooltip("Maximum horizontal speed")]
        public float maxHorizontalSpeed = 50f;

        [Tooltip("Maximum vertical speed")]
        public float maxVerticalSpeed = 100f;

        [Tooltip("Base plane speed")]
        public float baseSpeed = 30f;

        [Tooltip("Throttle boost multiplier")]
        public float throttleMultiplier = 1.5f;

        [Tooltip("Wind force intensity")]
        public float windIntensity = 5f;

        [Tooltip("Wind change interval in seconds")]
        public float windChangeInterval = 3f;

        [Header("Fuel Settings")]
        [Tooltip("Maximum fuel capacity")]
        public float maxFuel = 100f;

        [Tooltip("Fuel consumption rate per second")]
        public float fuelConsumptionRate = 5f;

        [Tooltip("Low fuel threshold for warning")]
        public float lowFuelThreshold = 20f;

        [Header("Scoring Settings")]
        [Tooltip("Points per structure destroyed")]
        public int pointsPerStructure = 100;

        [Tooltip("VIP box hit bonus")]
        public int vipBoxBonus = 5000;

        [Tooltip("Chain reaction multiplier")]
        public float chainReactionMultiplier = 1.5f;

        [Tooltip("Maximum combo multiplier")]
        public float maxComboMultiplier = 10f;

        [Tooltip("Combo decay time in seconds")]
        public float comboDecayTime = 2f;

        [Header("Destruction Settings")]
        [Tooltip("Impact force multiplier")]
        public float impactForceMultiplier = 1.0f;

        [Tooltip("Debris lifetime in seconds")]
        public float debrisLifetime = 5f;

        [Tooltip("Maximum active debris objects")]
        public int maxDebrisObjects = 50;

        [Tooltip("Chain reaction delay in seconds")]
        public float chainReactionDelay = 0.3f;

        [Header("Camera Settings")]
        [Tooltip("Camera follow speed")]
        public float cameraFollowSpeed = 5f;

        [Tooltip("Camera shake intensity")]
        public float cameraShakeIntensity = 0.5f;

        [Tooltip("Camera shake duration")]
        public float cameraShakeDuration = 0.3f;

        [Tooltip("Slow motion time scale")]
        public float slowMotionTimeScale = 0.3f;

        [Tooltip("Slow motion duration")]
        public float slowMotionDuration = 1.5f;

        [Header("Performance Settings")]
        [Tooltip("Target frame rate")]
        public int targetFrameRate = 60;

        [Tooltip("Minimum frame rate before quality reduction")]
        public int minFrameRate = 45;

        [Tooltip("Quality check interval in seconds")]
        public float qualityCheckInterval = 2f;

        [Header("Monetization Settings")]
        [Tooltip("Coins earned per crash")]
        public int coinsPerCrash = 50;

        [Tooltip("Coins earned per 1000 points")]
        public int coinsPer1000Points = 10;

        [Tooltip("Rewarded ad continue cooldown")]
        public float continueCooldown = 30f;

        [Tooltip("Rewarded ad score multiplier duration")]
        public float scoreMultiplierDuration = 30f;

        [Header("Battle Pass Settings")]
        [Tooltip("XP earned per match")]
        public int xpPerMatch = 50;

        [Tooltip("XP earned per 1000 points")]
        public int xpPer1000Points = 10;

        [Tooltip("Premium battle pass cost in gems")]
        public int premiumPassCost = 1000;

        [Header("Daily Challenge Settings")]
        [Tooltip("Number of daily challenges")]
        public int dailyChallengeCount = 3;

        [Tooltip("Daily challenge reset hour (UTC)")]
        public int dailyResetHour = 0;

        [Header("Audio Settings")]
        [Tooltip("Master volume (0-1)")]
        [Range(0f, 1f)]
        public float masterVolume = 1f;

        [Tooltip("Music volume (0-1)")]
        [Range(0f, 1f)]
        public float musicVolume = 0.7f;

        [Tooltip("SFX volume (0-1)")]
        [Range(0f, 1f)]
        public float sfxVolume = 0.8f;
    }
}
