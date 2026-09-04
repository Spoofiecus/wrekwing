using UnityEngine;
using WreckWing.Core;

namespace WreckWing.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject planeSelectPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject shopPanel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            HideAllScreens();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState newState)
        {
            HideAllScreens();
            ShowScreen(newState);
        }

        public void ShowScreen(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    if (mainMenuPanel) mainMenuPanel.SetActive(true);
                    break;
                case GameState.PlaneSelect:
                    if (planeSelectPanel) planeSelectPanel.SetActive(true);
                    break;
                case GameState.Playing:
                    if (hudPanel) hudPanel.SetActive(true);
                    break;
                case GameState.Paused:
                    if (pauseMenuPanel) pauseMenuPanel.SetActive(true);
                    break;
                case GameState.GameOver:
                    if (gameOverPanel) gameOverPanel.SetActive(true);
                    break;
                case GameState.Shop:
                    if (shopPanel) shopPanel.SetActive(true);
                    break;
            }
        }

        public void HideAllScreens()
        {
            if (mainMenuPanel) mainMenuPanel.SetActive(false);
            if (planeSelectPanel) planeSelectPanel.SetActive(false);
            if (hudPanel) hudPanel.SetActive(false);
            if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
            if (gameOverPanel) gameOverPanel.SetActive(false);
            if (shopPanel) shopPanel.SetActive(false);
        }

        public void ShowHUD() => ShowScreen(GameState.Playing);
        public void ShowPauseMenu() => ShowScreen(GameState.Paused);
        public void ShowGameOver() => ShowScreen(GameState.GameOver);
    }
}
