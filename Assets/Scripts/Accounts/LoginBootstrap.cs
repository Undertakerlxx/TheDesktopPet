using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DesktopPet.Save;

namespace DesktopPet.Accounts
{
    public class LoginBootstrap : MonoBehaviour
    {
        private readonly List<GameObject> hiddenSceneRoots = new();

        private AccountService accountService;
        private LoginPanelController loginPanel;
        private bool reloadSceneAfterLogin;
        private int lastProcessedSceneHandle = int.MinValue;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateRuntimeInstance()
        {
            GameObject existing = GameObject.Find(nameof(LoginBootstrap));
            if (existing != null)
            {
                return;
            }

            GameObject root = new(nameof(LoginBootstrap));
            root.AddComponent<LoginBootstrap>();
            DontDestroyOnLoad(root);
        }

        private void Awake()
        {
            AccountDatabase database = new();
            AccountMigrationService migrationService = new(database);
            migrationService.TryStageLegacyData();

            accountService = new AccountService(database, migrationService);
            accountService.TryRestoreRememberedLogin();
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void Start()
        {
            ProcessScene(SceneManager.GetActiveScene());
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        public void HandleLoginSuccess()
        {
            if (reloadSceneAfterLogin)
            {
                reloadSceneAfterLogin = false;
                HideLoginPanel();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }

            RestoreSceneRoots();
            HideLoginPanel();
        }

        public void SwitchAccount()
        {
            ThePetStatsManager statsManager = FindFirstObjectByType<ThePetStatsManager>();
            statsManager?.SaveCurrentStats();

            accountService?.Logout();
            reloadSceneAfterLogin = true;
            HideActiveSceneRoots(SceneManager.GetActiveScene());
            ShowLoginPanel();
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            ProcessScene(scene);
        }

        private void ProcessScene(Scene scene)
        {
            if (!scene.IsValid() || scene.handle == lastProcessedSceneHandle)
            {
                return;
            }

            lastProcessedSceneHandle = scene.handle;

            if (AccountSession.IsLoggedIn)
            {
                RestoreSceneRoots();
                HideLoginPanel();
                return;
            }

            HideActiveSceneRoots(scene);
            ShowLoginPanel();
        }

        private void HideActiveSceneRoots(Scene scene)
        {
            hiddenSceneRoots.Clear();
            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                if (rootObject == null || !rootObject.activeSelf || ShouldKeepActiveDuringLogin(rootObject))
                {
                    continue;
                }

                hiddenSceneRoots.Add(rootObject);
                rootObject.SetActive(false);
            }
        }

        private static bool ShouldKeepActiveDuringLogin(GameObject rootObject)
        {
            return rootObject.GetComponent<global::WindowController>() != null ||
                   rootObject.GetComponent<Camera>() != null;
        }

        private void RestoreSceneRoots()
        {
            foreach (GameObject rootObject in hiddenSceneRoots)
            {
                if (rootObject != null)
                {
                    rootObject.SetActive(true);
                }
            }

            hiddenSceneRoots.Clear();
        }

        private void ShowLoginPanel()
        {
            if (loginPanel == null)
            {
                loginPanel = gameObject.GetComponent<LoginPanelController>();
                if (loginPanel == null)
                {
                    loginPanel = gameObject.AddComponent<LoginPanelController>();
                }
            }

            loginPanel.Initialize(this, accountService);
            loginPanel.enabled = true;
        }

        private void HideLoginPanel()
        {
            if (loginPanel != null)
            {
                loginPanel.enabled = false;
            }
        }
    }
}
