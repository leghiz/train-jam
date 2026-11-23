
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Scene Settings")]
    [SerializeField] private string battleSceneName = "BattleScene";

    [Header("Exploration References")]
    [SerializeField] private Camera explorationCamera;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerRotation playerRotation;
    [SerializeField] private PlayerCombatTargeting playerTargeting;
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private PlayerStats originalPlayerStats;

    [Header("Transition")]
    [SerializeField] private BattleTransitionView transitionView;
    [SerializeField] private BattleTransitionConfig transitionConfig;
    [SerializeField] private AudioSource transitionSound;
    [SerializeField] private AudioSource ambienceSound;

    private Camera battleCamera;
    private AsyncOperation loadingOperation;
    private BattleSetup battleSetup;

    private Scene explorationScene;
    private bool isBattleStarted;
    public bool IsBattleStarted => isBattleStarted;

    private bool wasMovementEnabled;
    private bool wasRotationEnabled;
    private bool wasTargetingEnabled;
    private bool wasExplorationCameraEnabled;
    private bool wasPlayerVisualActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (transitionView != null)
            transitionView.Covered += OnTransitionCovered;
    }

    private void OnDestroy()
    {
        if (transitionView != null)
            transitionView.Covered -= OnTransitionCovered;
    }

    private void OnTransitionCovered()
    {
        if (loadingOperation != null)
            loadingOperation.allowSceneActivation = true;
    }

    public void StartBattle(EnemyEncounter enemy, bool enemyAttackedFirst = false)
    {
        if (isBattleStarted || enemy == null)
            return;

        if (enemy.EncounterData == null)
        {
            return;
        }

        if (originalPlayerStats == null)
        {
            return;
        }

        isBattleStarted = true;

        explorationScene = SceneManager.GetActiveScene();

        BattleContext.SetEncounter(enemy.EncounterData);
        BattleContext.SetEnemy(enemy);
        BattleContext.SetEnemyAttackedFirst(enemyAttackedFirst);

        BattleContext.SetInventory(originalPlayerStats.GetComponent<InventorySystem>());
        
        if (playerTargeting != null)
            playerTargeting.StopAiming();
        
        wasMovementEnabled =
            playerMovement != null && playerMovement.enabled;

        wasRotationEnabled =
            playerRotation != null && playerRotation.enabled;

        wasTargetingEnabled =
            playerTargeting != null && playerTargeting.enabled;

        wasExplorationCameraEnabled =
            explorationCamera != null && explorationCamera.enabled;
        
        
        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerRotation != null)
            playerRotation.enabled = false;

        if (playerTargeting != null)
            playerTargeting.enabled = false;
        
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        if (ambienceSound != null)
            ambienceSound.Pause();

        if (transitionSound != null)
            transitionSound.Play();

        StartCoroutine(LoadBattleScene(enemy));
    }

    private IEnumerator LoadBattleScene(EnemyEncounter enemy)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(
            battleSceneName,
            LoadSceneMode.Additive
        );

        bool useTransition =
            transitionView != null &&
            transitionConfig != null &&
            explorationCamera != null;

        if (useTransition)
        {
            loadingOperation = operation;
            operation.allowSceneActivation = false;

            Vector3 aimPoint = enemy.transform.position + Vector3.up * transitionConfig.aimHeight;
            transitionView.Play(explorationCamera, aimPoint, transitionConfig);
        }

        yield return operation;

        loadingOperation = null;

        SetCameraEnabled(explorationCamera, false);

        Scene battleScene =
            SceneManager.GetSceneByName(battleSceneName);

        if (!battleScene.IsValid() || !battleScene.isLoaded)
        {
            RestoreExploration();
            yield break;
        }

        SceneManager.SetActiveScene(battleScene);
        
        foreach (GameObject root in battleScene.GetRootGameObjects())
        {
            battleSetup = root.GetComponentInChildren<BattleSetup>(true);

            if (battleSetup != null)
                break;
        }

        if (battleSetup == null)
        {
            StartCoroutine(UnloadBattleScene());
            yield break;
        }
        
        foreach (GameObject root in battleScene.GetRootGameObjects())
        {
            Camera[] cameras =
                root.GetComponentsInChildren<Camera>(true);

            foreach (Camera cam in cameras)
            {
                if (cam.CompareTag("MainCamera"))
                {
                    battleCamera = cam;
                    break;
                }
            }

            if (battleCamera != null)
                break;
        }

        if (battleCamera == null)
        {
            StartCoroutine(UnloadBattleScene());
            yield break;
        }

        battleSetup.Setup(originalPlayerStats);
        
        SetCameraEnabled(battleCamera, true);
    }

    public void EndBattle()
    {
        if (!isBattleStarted)
            return;

        StartCoroutine(UnloadBattleScene());
    }

    private IEnumerator UnloadBattleScene()
    {
        if (battleSetup != null && originalPlayerStats != null)
            battleSetup.CopyPlayerStatsTo(originalPlayerStats);

        SetCameraEnabled(battleCamera, false);

        AsyncOperation operation = SceneManager.UnloadSceneAsync(
            battleSceneName
        );

        if (operation != null)
            yield return operation;

        SceneManager.SetActiveScene(explorationScene);

        battleSetup = null;
        battleCamera = null;

        RestoreExploration();

        BattleContext.Clear();

        isBattleStarted = false;

        if (originalPlayerStats != null && originalPlayerStats.Health <= 0)
            originalPlayerStats.TakeDamage(0);
    }

    private void RestoreExploration()
    {
        if (transitionView != null)
            transitionView.Cancel();

        if (ambienceSound != null)
            ambienceSound.UnPause();

        if (playerMovement != null)
            playerMovement.enabled = wasMovementEnabled;

        if (playerRotation != null)
            playerRotation.enabled = wasRotationEnabled;

        if (playerTargeting != null)
            playerTargeting.enabled = wasTargetingEnabled;
        
        SetCameraEnabled(
            explorationCamera,
            wasExplorationCameraEnabled
        );
    }

    private void SetCameraEnabled(Camera cam, bool enabled)
    {
        if (cam == null)
            return;

        cam.enabled = enabled;

        AudioListener listener = cam.GetComponentInChildren<AudioListener>(true);

        if (listener != null)
            listener.enabled = enabled;
    }
}
