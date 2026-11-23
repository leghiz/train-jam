using UnityEngine;
using UnityEngine.InputSystem;

public class StoveInteraction : MonoBehaviour
{
    [SerializeField] private EndingView endingView;
    [SerializeField] private TrainSway trainSway;
    [SerializeField] private bool showEndingAfterStop = true;

    private bool playerNearby;
    private bool interactionTriggered;
    private WagonLayout wagon;
    private InventoryView inventoryView;

    public bool PlayerNearby => playerNearby;

    public bool HasEnemiesLeft
    {
        get
        {
            if (wagon == null)
                wagon = GetComponentInParent<WagonLayout>();

            return wagon != null && wagon.GetComponentsInChildren<EnemyEncounter>().Length > 0;
        }
    }

    private void Start()
    {
        if (endingView == null)
            endingView = FindAnyObjectByType<EndingView>();

        if (trainSway == null)
            trainSway = FindAnyObjectByType<TrainSway>();

        inventoryView = FindAnyObjectByType<InventoryView>();

        if (trainSway != null && showEndingAfterStop)
            trainSway.OnTrainStopped += HandleTrainStopped;
    }

    private void OnDestroy()
    {
        if (trainSway != null)
            trainSway.OnTrainStopped -= HandleTrainStopped;
    }

    private void Update()
    {
        if (interactionTriggered || !playerNearby || endingView == null || endingView.IsShown || Keyboard.current == null)
            return;

        if (inventoryView != null && inventoryView.IsOpen)
            return;

        if (!Keyboard.current.fKey.wasPressedThisFrame)
            return;

        if (HasEnemiesLeft)
            return;

        interactionTriggered = true;

        if (trainSway != null)
        {
            trainSway.StopTrain();
            
            if (!showEndingAfterStop)
                endingView.Show();
        }
        else
        {
            endingView.Show();
        }
    }

    private void HandleTrainStopped()
    {
        if (endingView != null && !endingView.IsShown)
            endingView.Show();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNearby = false;
    }
}
