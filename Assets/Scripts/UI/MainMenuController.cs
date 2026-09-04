using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WreckWing.Core;
using WreckWing.Gameplay;

namespace WreckWing.UI
{
    /// <summary>
    /// Main menu controller: wires navigation buttons and displays the high score.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button planeSelectButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button quitButton;

        [Header("Text")]
        [SerializeField] private TMP_Text highScoreText;

        private void Start()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            else Debug.LogWarning("[MainMenu] playButton not assigned.");

            if (planeSelectButton != null) planeSelectButton.onClick.AddListener(OnPlaneSelectClicked);
            if (shopButton != null) shopButton.onClick.AddListener(OnShopClicked);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);

            RefreshHighScore();
        }

        private void OnDestroy()
        {
            if (playButton != null) playButton.onClick.RemoveListener(OnPlayClicked);
            if (planeSelectButton != null) planeSelectButton.onClick.RemoveListener(OnPlaneSelectClicked);
            if (shopButton != null) shopButton.onClick.RemoveListener(OnShopClicked);
            if (quitButton != null) quitButton.onClick.RemoveListener(OnQuitClicked);
        }

        private void OnEnable() => RefreshHighScore();

        /// <summary>Refreshes the displayed high score from ScoreManager.</summary>
        public void RefreshHighScore()
        {
            if (highScoreText == null) return;
            int hs = ScoreManager.Instance != null ? ScoreManager.Instance.GetHighScore() : 0;
            highScoreText.text = $"High Score: {hs:N0}";
        }

        private void OnPlayClicked()
        {
            if (GameManager.Instance != null) GameManager.Instance.SetGameState(GameState.Playing);
        }

        private void OnPlaneSelectClicked()
        {
            if (GameManager.Instance != null) GameManager.Instance.SetGameState(GameState.PlaneSelect);
        }

        private void OnShopClicked()
        {
            if (GameManager.Instance != null) GameManager.Instance.SetGameState(GameState.Shop);
        }

        private void OnQuitClicked()
        {
            if (GameManager.Instance != null) GameManager.Instance.QuitGame();
            else Debug.LogWarning("[MainMenu] GameManager.Instance missing; cannot quit.");
        }
    }
}