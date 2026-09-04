using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WreckWing.Core;
using WreckWing.Gameplay;

namespace WreckWing.UI
{
    /// <summary>
    /// Game over screen controller: displays run results and handles
    /// retry, main menu, and rewarded-ad continue actions.
    /// </summary>
    public class GameOverController : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField] private TMP_Text destructionText;
        [SerializeField] private TMP_Text coinsText;

        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button continueButton;

        private void Start()
        {
            if (retryButton != null) retryButton.onClick.AddListener(OnRetryClicked);
            else Debug.LogWarning("[GameOver] retryButton not assigned.");

            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
        }

        private void OnDestroy()
        {
            if (retryButton != null) retryButton.onClick.RemoveListener(OnRetryClicked);
            if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            if (continueButton != null) continueButton.onClick.RemoveListener(OnContinueClicked);
        }

        /// <summary>Fills the results panel and shows it.</summary>
        /// <param name="finalScore">Score achieved this run.</param>
        /// <param name="highScore">All-time high score.</param>
        /// <param name="destructionPct">Destruction percentage (0-100).</param>
        /// <param name="coinsEarned">Coins earned this run.</param>
        public void ShowResults(int finalScore, int highScore, float destructionPct, int coinsEarned)
        {
            if (finalScoreText != null) finalScoreText.text = $"Score: {finalScore:N0}";
            if (highScoreText != null) highScoreText.text = $"Best: {highScore:N0}";
            if (destructionText != null)
                destructionText.text = $"Destruction: {Mathf.Clamp(destructionPct, 0f, 100f):0}%";
            if (coinsText != null) coinsText.text = $"+{coinsEarned:N0} Coins";
            gameObject.SetActive(true);
        }

        private void OnRetryClicked()
        {
            if (GameManager.Instance != null) GameManager.Instance.RestartGame();
        }

        private void OnMainMenuClicked()
        {
            if (GameManager.Instance != null) GameManager.Instance.ReturnToMainMenu();
        }

        /// <summary>Rewarded ad placeholder: grants bonus coins.</summary>
        private void OnContinueClicked()
        {
            Debug.Log("[GameOver] Rewarded ad placeholder");
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.AddCoins(100);
                ProgressionManager.Instance.SaveAll();
                if (coinsText != null) coinsText.text = "+100 Bonus Coins";
                if (continueButton != null) continueButton.interactable = false;
            }
            else Debug.LogWarning("[GameOver] ProgressionManager.Instance missing.");
        }
    }
}