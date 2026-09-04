using UnityEngine;
using UnityEngine.UI;
using WreckWing.Core;

namespace WreckWing.UI
{
    /// <summary>
    /// Pause menu controller: wires resume, restart, and main menu buttons
    /// to the GameManager's flow control methods.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        private void Start()
        {
            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
            else Debug.LogWarning("[PauseMenu] resumeButton not assigned.");

            if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
            else Debug.LogWarning("[PauseMenu] restartButton not assigned.");

            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            else Debug.LogWarning("[PauseMenu] mainMenuButton not assigned.");
        }

        private void OnDestroy()
        {
            if (resumeButton != null) resumeButton.onClick.RemoveListener(OnResumeClicked);
            if (restartButton != null) restartButton.onClick.RemoveListener(OnRestartClicked);
            if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }

        private void OnResumeClicked()
        {
            if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
            else Debug.LogWarning("[PauseMenu] GameManager.Instance missing.");
        }

        private void OnRestartClicked()
        {
            if (GameManager.Instance != null) GameManager.Instance.RestartGame();
            else Debug.LogWarning("[PauseMenu] GameManager.Instance missing.");
        }

        private void OnMainMenuClicked()
        {
            if (GameManager.Instance != null) GameManager.Instance.ReturnToMainMenu();
            else Debug.LogWarning("[PauseMenu] GameManager.Instance missing.");
        }
    }
}