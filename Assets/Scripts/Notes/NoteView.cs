using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class NoteView : MonoBehaviour
{
    [SerializeField] private Canvas backdrop;
    [SerializeField] private Canvas shade;
    [SerializeField] private GameObject window;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text textLabel;
    [SerializeField] private GameObject closeHint;
    [SerializeField] private Light modelLight;

    [SerializeField] private float charactersPerSecond = 40f;
    [SerializeField] private float modelDistance = 0.6f;
    [SerializeField] private float modelScreenSize = 0.75f;
    [SerializeField] private float modelLean = 12f;
    [SerializeField] private float modelRoll = -8f;

    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerRotation playerRotation;
    [SerializeField] private PlayerCombatTargeting playerTargeting;
    [SerializeField] private Rigidbody playerRigidbody;

    private GameObject model;
    private float modelSize;
    private float visibleCharacters;
    private int totalCharacters;
    private bool isOpen;
    private int openedFrame = -1;
    private int closedFrame = -1;
    private bool wasMovementEnabled;
    private bool wasRotationEnabled;
    private bool wasTargetingEnabled;

    public bool IsOpen => isOpen;
    public bool IsBusy => isOpen || Time.frameCount == closedFrame;

    private void Awake()
    {
        FindPlayer();
        SetVisible(false);
    }

    private void Update()
    {
        if (!isOpen)
            return;

        Camera cam = Camera.main;

        if (cam != null && modelLight != null)
            modelLight.transform.position = cam.transform.position;

        if (Time.frameCount == openedFrame)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            Close();
            return;
        }

        if (visibleCharacters < totalCharacters)
        {
            if (IsAnyPressed())
                visibleCharacters = totalCharacters;
            else
                visibleCharacters = Mathf.Min(totalCharacters, visibleCharacters + charactersPerSecond * Time.deltaTime);
        }

        textLabel.maxVisibleCharacters = Mathf.FloorToInt(visibleCharacters);
    }

    public void Open(NoteInteraction note)
    {
        if (isOpen)
            return;

        backdrop.worldCamera = Camera.main;
        backdrop.planeDistance = modelDistance * 2f;
        shade.worldCamera = Camera.main;
        shade.planeDistance = modelDistance * 0.8f;
        SetVisible(true);

        titleLabel.text = note.Title;
        textLabel.text = note.Text;
        textLabel.maxVisibleCharacters = 0;
        textLabel.ForceMeshUpdate();
        totalCharacters = textLabel.textInfo.characterCount;
        visibleCharacters = 0f;

        CreateModel(note.Visual);

        if (Camera.main != null)
            PlaceModel(Camera.main);

        LockPlayer();

        isOpen = true;
        openedFrame = Time.frameCount;
    }

    public void Close()
    {
        if (!isOpen)
            return;

        if (model != null)
            Destroy(model);

        SetVisible(false);
        UnlockPlayer();

        isOpen = false;
        closedFrame = Time.frameCount;
    }

    private void SetVisible(bool isVisible)
    {
        backdrop.gameObject.SetActive(isVisible);
        shade.gameObject.SetActive(isVisible);
        window.SetActive(isVisible);

        if (modelLight != null)
            modelLight.gameObject.SetActive(isVisible);
    }

    private void CreateModel(GameObject source)
    {
        model = new GameObject("NoteModel");

        if (source == null)
            return;

        GameObject copy = Instantiate(source, model.transform);
        copy.transform.localPosition = Vector3.zero;
        copy.transform.localRotation = Quaternion.identity;

        foreach (Collider col in copy.GetComponentsInChildren<Collider>())
        {
            Destroy(col);
        }

        Renderer[] renderers = copy.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;

        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        copy.transform.localPosition = -bounds.center;
        modelSize = Mathf.Max(bounds.size.x, bounds.size.z);

        foreach (Renderer r in renderers)
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    private void PlaceModel(Camera cam)
    {
        if (model == null || modelSize <= 0f)
            return;

        float worldHeight = 2f * modelDistance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * modelScreenSize;

        Quaternion roll = Quaternion.Euler(0f, 0f, modelRoll);
        Quaternion standUp = Quaternion.Euler(-90f + modelLean, 0f, 0f);
        Quaternion portrait = Quaternion.Euler(0f, 90f, 0f);

        model.transform.SetParent(cam.transform, false);
        model.transform.localPosition = Vector3.forward * modelDistance;
        model.transform.localRotation = roll * standUp * portrait;
        model.transform.localScale = Vector3.one * (worldHeight / modelSize);
    }

    private bool IsAnyPressed()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (keyboard != null && keyboard.anyKey.wasPressedThisFrame)
            return true;

        if (mouse == null)
            return false;

        return mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame;
    }

    private void FindPlayer()
    {
        InventorySystem inventory = FindAnyObjectByType<InventorySystem>();

        if (inventory == null)
            return;

        if (playerMovement == null)
            playerMovement = inventory.GetComponent<PlayerMovement>();

        if (playerRotation == null)
            playerRotation = inventory.GetComponent<PlayerRotation>();

        if (playerTargeting == null)
            playerTargeting = inventory.GetComponent<PlayerCombatTargeting>();

        if (playerRigidbody == null)
            playerRigidbody = inventory.GetComponent<Rigidbody>();
    }

    private void LockPlayer()
    {
        wasMovementEnabled = playerMovement != null && playerMovement.enabled;
        wasRotationEnabled = playerRotation != null && playerRotation.enabled;
        wasTargetingEnabled = playerTargeting != null && playerTargeting.enabled;

        if (playerTargeting != null && playerTargeting.IsAiming)
            playerTargeting.StopAiming();

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
    }

    private void UnlockPlayer()
    {
        if (playerMovement != null)
            playerMovement.enabled = wasMovementEnabled;

        if (playerRotation != null)
            playerRotation.enabled = wasRotationEnabled;

        if (playerTargeting != null)
            playerTargeting.enabled = wasTargetingEnabled;
    }
}
