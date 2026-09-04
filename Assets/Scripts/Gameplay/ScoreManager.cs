using UnityEngine;
using System;

namespace WreckWing.Gameplay
{
    /// <summary>
    /// Manages scoring, combo multipliers, and high score persistence for the game.
    /// Implements a singleton pattern for global access.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance of the ScoreManager.
        /// </summary>
        public static ScoreManager Instance { get; private set; }

        [Header("Score Settings")]
        [SerializeField] private int startingScore = 0;
        [SerializeField] private float comboDecayTime = 2f;
        [SerializeField] private float comboIncrement = 0.5f;
        [SerializeField] private float maxComboMultiplier = 5f;

        [Header("Runtime Values")]
        [SerializeField] private int currentScore;
        [SerializeField] private int highScore;
        [SerializeField] private float comboMultiplier = 1f;
        [SerializeField] private float lastHitTime;

        /// <summary>
        /// Event invoked when the current score changes.
        /// </summary>
        public event Action<int> OnScoreChanged;

        /// <summary>
        /// Event invoked when the combo multiplier changes.
        /// </summary>
        public event Action<float> OnComboChanged;

        /// <summary>
        /// Event invoked when a new high score is reached.
        /// </summary>
        public event Action<int> OnHighScoreReached;

        private const string HighScoreKey = "WreckWing_HighScore";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadHighScore();
            ResetScore();
        }

        /// <summary>
        /// Adds points to the current score, applying the combo multiplier.
        /// </summary>
        /// <param name="points">Base points to add before multiplier.</param>
        public void AddScore(int points)
        {
            float multiplier = GetComboMultiplier();
            int finalPoints = Mathf.RoundToInt(points * multiplier);
            currentScore += finalPoints;

            OnScoreChanged?.Invoke(currentScore);

            if (currentScore > highScore)
            {
                highScore = currentScore;
                SaveHighScore();
                OnHighScoreReached?.Invoke(highScore);
            }
        }

        /// <summary>
        /// Resets the current score and combo multiplier to starting values.
        /// </summary>
        public void ResetScore()
        {
            currentScore = startingScore;
            comboMultiplier = 1f;
            OnScoreChanged?.Invoke(currentScore);
            OnComboChanged?.Invoke(comboMultiplier);
        }

        /// <summary>
        /// Gets the current score.
        /// </summary>
        /// <returns>The current score value.</returns>
        public int GetScore()
        {
            return currentScore;
        }

        /// <summary>
        /// Gets the saved high score.
        /// </summary>
        /// <returns>The high score value.</returns>
        public int GetHighScore()
        {
            return highScore;
        }

        /// <summary>
        /// Registers a hit, increasing the combo multiplier.
        /// Call this when the player successfully hits a target.
        /// </summary>
        public void RegisterHit()
        {
            comboMultiplier = Mathf.Min(comboMultiplier + comboIncrement, maxComboMultiplier);
            lastHitTime = Time.time;
            OnComboChanged?.Invoke(comboMultiplier);
        }

        /// <summary>
        /// Gets the current combo multiplier value.
        /// </summary>
        /// <returns>The current combo multiplier.</returns>
        public float GetComboMultiplier()
        {
            return comboMultiplier;
        }

        private void Update()
        {
            if (comboMultiplier > 1f && Time.time - lastHitTime > comboDecayTime)
            {
                comboMultiplier = Mathf.Max(1f, comboMultiplier - comboIncrement);
                OnComboChanged?.Invoke(comboMultiplier);
            }
        }

        private void SaveHighScore()
        {
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }

        private void LoadHighScore()
        {
            highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        }
    }
}