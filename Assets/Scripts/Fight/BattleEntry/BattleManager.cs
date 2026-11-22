
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

    private Camera battleCamera;
    private BattleSetup battleSetup;

    private Scene explorationScene;
    private bool isBattleStarted;

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
        
        
        SetCameraEnabled(explorationCamera, false);

        StartCoroutine(LoadBattleScene());
    }

    private IEnumerator LoadBattleScene()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(
            battleSceneName,
            LoadSceneMode.Additive
        );

        yield return operation;

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

        AudioListener listener = cam.GetComponent<AudioListener>();

        if (listener != null)
            listener.enabled = enabled;
    }
}
