using UnityEngine;
using UnityEngine.InputSystem;

public class DoorInteraction : MonoBehaviour
{
    [SerializeField] private bool requiresKey;

    private TrainGenerator generator;
    private PlayerStats playerStats;
    private bool playerNearby;

    private void Start()
    {
        generator = FindFirstObjectByType<TrainGenerator>();
    }

    private void Update()
    {
        if (!playerNearby || Keyboard.current == null)
            return;

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (requiresKey && !playerStats.HasKey)
            {
                Debug.Log("Дверь заперта! Нужен ключ.");
                return;
            }

            if (generator == null)
            {
                Debug.LogError("TrainGenerator не найден!");
                return;
            }

            if (!generator.EnterNextWagon())
                Debug.Log("Следующего вагона нет!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerStats = other.GetComponent<PlayerStats>();
            playerNearby = playerStats != null;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            playerStats = null;
        }
    }
}
