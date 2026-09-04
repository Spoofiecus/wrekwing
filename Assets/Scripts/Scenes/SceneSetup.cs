#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WreckWing.Core;
using WreckWing.Gameplay;
using WreckWing.Audio;
using WreckWing.UI;

namespace WreckWing.Scenes
{
    public static class SceneSetup
    {
        [MenuItem("WreckWing/Setup Main Menu Scene")]
        public static void SetupMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var gameManager = new GameObject("GameManager");
            gameManager.AddComponent<GameManager>();
            gameManager.AddComponent<ScoreManager>();
            gameManager.AddComponent<WindSystem>();
            gameManager.AddComponent<ProgressionManager>();
            gameManager.AddComponent<AchievementManager>();
            gameManager.AddComponent<DailyChallengeManager>();
            gameManager.AddComponent<BattlePassManager>();

            var destruction = new GameObject("Destruction");
            destruction.AddComponent<DestructionManager>();

            var effects = new GameObject("Effects");
            effects.AddComponent<SlowMotionEffect>();
            effects.AddComponent<ScreenShake>();

            var audio = new GameObject("Audio");
            audio.AddComponent<AudioManager>();

            var ui = new GameObject("UI");
            ui.AddComponent<UIManager>();

            var cameraObj = new GameObject("Camera");
            cameraObj.tag = "MainCamera";
            cameraObj.transform.position = new Vector3(0f, 5f, -10f);
            cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<AudioListener>();

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            var canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            canvasObj.AddComponent<GraphicRaycaster>();

            CreatePanel(canvasObj.transform, "MainMenuPanel", true);
            CreatePanel(canvasObj.transform, "HUDPanel", false);
            CreatePanel(canvasObj.transform, "PauseMenuPanel", false);
            CreatePanel(canvasObj.transform, "GameOverPanel", false);
            CreatePanel(canvasObj.transform, "PlaneSelectPanel", false);

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
            Debug.Log("[SceneSetup] Main Menu scene created and saved to Assets/Scenes/MainMenu.unity");
        }

        private static void CreatePanel(Transform parent, string name, bool active)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            panel.SetActive(active);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.AddComponent<CanvasGroup>();
        }
    }
}
#endif