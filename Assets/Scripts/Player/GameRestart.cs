using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameRestart : MonoBehaviour
{
    private PlayerStats playerStats;
    private InventoryController inventory;
    private PlayerMovement playerMovement;
    private TrainGenerator trainGenerator;

    private bool isRestarting;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        inventory = GetComponent<InventoryController>();
        playerMovement = GetComponent<PlayerMovement>();

        trainGenerator = FindFirstObjectByType<TrainGenerator>();
    }

    private void OnEnable()
    {
        if (playerStats != null)
            playerStats.Died += Restart;
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.Died -= Restart;
    }

    private void Restart()
    {
        if (isRestarting)
            return;

        if (trainGenerator == null)
        {
            Debug.LogError("TrainGenerator не найден!");
            return;
        }

        StartCoroutine(RestartRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        isRestarting = true;

        Debug.Log("Начинается новое прохождение!");
        
        if (playerMovement != null)
            playerMovement.enabled = false;

        if (inventory != null)
        {
            inventory.enabled = false;
            inventory.ResetInventory();
        }
        
        trainGenerator.Clear();
        
        yield return null;
        
        playerStats.ResetStats();
        
        trainGenerator.Generate();
        
        if (inventory != null)
            inventory.enabled = true;

        if (playerMovement != null)
            playerMovement.enabled = true;

        isRestarting = false;

        Debug.Log("Новое прохождение началось!");
    }
    
    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.kKey.wasPressedThisFrame &&
            !isRestarting)
        {
            playerStats.TakeDamage(playerStats.Health + 100);
        }
    }
    
}