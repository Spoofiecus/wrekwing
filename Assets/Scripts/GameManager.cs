using UnityEngine;
using UnityEngine.SceneManagement;

namespace WreckWing.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Settings")]
        [SerializeField] private GameSettings gameSettings;
        [SerializeField] private bool dontDestroyOnLoad = true;

        [Header("Game State")]
        public GameState CurrentState { get; private set; }
        public bool IsPaused { get; private set; }

        public System.Action<GameState> OnGameStateChanged;
        public System.Action OnGamePaused;
        public System.Action OnGameResumed;

        public PlaneController ActivePlane { get; set; }
        public ScoreManager ScoreManager { get; private set; }
        public DestructionManager DestructionManager { get; private set; }
        public AudioManager AudioManager { get; private set; }
        public UIManager UIManager { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            InitializeSystems();
        }

        private void Start()
        {
            SetGameState(GameState.MainMenu);
        }

        private void Update()
        {
            HandlePauseInput();
            QualityManager.Update();
        }

        private void InitializeSystems()
        {
            ScoreManager = FindObjectOfType<ScoreManager>();
            DestructionManager = FindObjectOfType<DestructionManager>();
            AudioManager = FindObjectOfType<AudioManager>();
            UIManager = FindObjectOfType<UIManager>();
            QualityManager.Initialize();
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        public void SetGameState(GameState newState)
        {
            if (CurrentState == newState) return;

            CurrentState = newState;
            OnGameStateChanged?.Invoke(newState);

            switch (newState)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    IsPaused = false;
                    break;
                case GameState.PlaneSelect:
                    Time.timeScale = 1f;
                    IsPaused = false;
                    break;
                case GameState.Playing:
                    Time.timeScale = 1f;
                    IsPaused = false;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    IsPaused = true;
                    OnGamePaused?.Invoke();
                    break;
                case GameState.GameOver:
                    Time.timeScale = 0.3f;
                    IsPaused = false;
                    break;
                case GameState.Shop:
                    Time.timeScale = 1f;
                    IsPaused = false;
                    break;
            }
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                SetGameState(GameState.Playing);
                OnGameResumed?.Invoke();
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene.buildIndex);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            SetGameState(GameState.MainMenu);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    public enum GameState
    {
        MainMenu,
        PlaneSelect,
        Playing,
        Paused,
        GameOver,
        Shop
    }
}
