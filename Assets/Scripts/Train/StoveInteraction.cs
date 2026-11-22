using UnityEngine;
using UnityEngine.InputSystem;

public class StoveInteraction : MonoBehaviour
{
    [SerializeField] private EndingView endingView;

    private bool playerNearby;
    private WagonLayout wagon;

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
    }

    private void Update()
    {
        if (!playerNearby || endingView == null || endingView.IsShown || Keyboard.current == null)
            return;

        if (!Keyboard.current.fKey.wasPressedThisFrame)
            return;

        if (HasEnemiesLeft)
            return;

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
