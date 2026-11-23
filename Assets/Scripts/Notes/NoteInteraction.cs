using UnityEngine;
using UnityEngine.InputSystem;

public class NoteInteraction : MonoBehaviour
{
    [SerializeField] private string title;
    [SerializeField, TextArea(5, 20)] private string text;
    [SerializeField] private GameObject visual;
    [SerializeField] private NoteView view;

    private bool playerNearby;
    private InventoryView inventoryView;

    public bool PlayerNearby => playerNearby;
    public string Title => title;
    public string Text => text;
    public GameObject Visual => visual;

    private void Start()
    {
        if (view == null)
            view = FindAnyObjectByType<NoteView>();

        inventoryView = FindAnyObjectByType<InventoryView>();
    }

    private void Update()
    {
        if (!playerNearby || view == null || view.IsBusy || Keyboard.current == null)
            return;

        if (inventoryView != null && inventoryView.IsOpen)
            return;

        if (Keyboard.current.fKey.wasPressedThisFrame)
            view.Open(this);
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
