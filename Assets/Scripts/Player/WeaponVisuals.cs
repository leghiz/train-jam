using UnityEngine;

public class WeaponVisuals : MonoBehaviour
{
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private Transform weaponHolder;

    private GameObject currentWeaponModel;
    private WeaponData lastEquippedWeapon;

    public Transform WeaponHolder => weaponHolder;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = FindFirstObjectByType<InventorySystem>();
        }
    }

    private void OnEnable()
    {
        if (inventory != null)
        {
            inventory.Changed += UpdateWeaponModel;
        }
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.Changed -= UpdateWeaponModel;
        }
    }

    private void Start()
    {
        UpdateWeaponModel();
    }

    private void UpdateWeaponModel()
    {
        if (inventory == null) 
            return;

        lastEquippedWeapon = inventory.Weapon;
        
        for (int i = weaponHolder.childCount - 1; i >= 0; i--)
        {
            Destroy(weaponHolder.GetChild(i).gameObject);
        }
        currentWeaponModel = null;
        
        if (lastEquippedWeapon == null || lastEquippedWeapon.worldPrefab == null) 
            return;
        
        currentWeaponModel = Instantiate(lastEquippedWeapon.worldPrefab, weaponHolder);
        currentWeaponModel.transform.localPosition = Vector3.zero;
        currentWeaponModel.transform.localRotation = Quaternion.identity;

        foreach (var pickup in currentWeaponModel.GetComponentsInChildren<ItemPickup>())
            Destroy(pickup);
    }
}
