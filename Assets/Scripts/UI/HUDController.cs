using UnityEngine;
using TMPro;
using WreckWing.Gameplay;
using WreckWing.Core;

namespace WreckWing.UI
{
    /// <summary>
    /// HUD controller: displays score, combo, destruction percent, and fuel.
    /// Subscribes to manager events and spawns floating score popups.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        public static HUDController Instance { get; private set; }

        [Header("Text Fields")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text comboText;
        [SerializeField] private TMP_Text destructionPercentText;

        [Header("Bars & Popups")]
        [SerializeField] private RectTransform fuelBar;
        [SerializeField] private GameObject scorePopupPrefab;

        private PlaneController _plane;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged += UpdateScore;
                ScoreManager.Instance.OnComboChanged += UpdateCombo;
            }
            else Debug.LogWarning("[HUD] ScoreManager.Instance is null; score HUD disabled.");

            if (GameManager.Instance?.DestructionManager != null)
                GameManager.Instance.DestructionManager.OnDestructionPercentChanged += UpdateDestruction;
            else Debug.LogWarning("[HUD] DestructionManager not found; destruction HUD disabled.");
        }

        private void OnDestroy()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged -= UpdateScore;
                ScoreManager.Instance.OnComboChanged -= UpdateCombo;
            }
            if (GameManager.Instance?.DestructionManager != null)
                GameManager.Instance.DestructionManager.OnDestructionPercentChanged -= UpdateDestruction;
            if (_plane != null) _plane.OnFuelChanged -= UpdateFuel;
            if (Instance == this) Instance = null;
        }

        /// <summary>Binds the HUD to a plane instance for fuel tracking.</summary>
        public void SetPlane(PlaneController plane)
        {
            if (_plane != null) _plane.OnFuelChanged -= UpdateFuel;
            _plane = plane;
            if (_plane != null)
            {
                _plane.OnFuelChanged += UpdateFuel;
                UpdateFuel(_plane.FuelPercent);
            }
        }

        /// <summary>Updates the score label.</summary>
        public void UpdateScore(int score)
        {
            if (scoreText != null) scoreText.text = $"Score: {score:N0}";
        }

        /// <summary>Updates the combo multiplier label; hides at 1x.</summary>
        public void UpdateCombo(float combo)
        {
            if (comboText == null) return;
            comboText.text = $"x{combo:0.0}";
            comboText.gameObject.SetActive(combo > 1.01f);
        }

        /// <summary>Updates the destruction percentage label (expects 0-100).</summary>
        public void UpdateDestruction(float percent)
        {
            if (destructionPercentText != null)
                destructionPercentText.text = $"{Mathf.Clamp(percent, 0f, 100f):0}%";
        }

        /// <summary>Updates the fuel bar fill (expects 0-1).</summary>
        public void UpdateFuel(float fuelPercent)
        {
            if (fuelBar != null)
            {
                fuelBar.anchorMax = new Vector2(Mathf.Clamp01(fuelPercent), fuelBar.anchorMax.y);
            }
        }

        /// <summary>Spawns a floating score popup at a world position.</summary>
        public void ShowScorePopup(int points, Vector3 worldPos)
        {
            if (scorePopupPrefab == null) return;
            Vector3 screenPos = Camera.main != null
                ? Camera.main.WorldToScreenPoint(worldPos)
                : worldPos;
            var popup = Instantiate(scorePopupPrefab, screenPos, Quaternion.identity, transform);
            var label = popup.GetComponent<TMP_Text>();
            if (label != null) label.text = $"+{points:N0}";
            Destroy(popup, 1f);
        }
    }
}