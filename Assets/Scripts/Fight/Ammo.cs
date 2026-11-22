using System.Collections.Generic;
using UnityEngine;

public class Ammo : MonoBehaviour
{
    [SerializeField] private BattleConfig config;
    [SerializeField] private int defaultMagazineSize = 1;
    [SerializeField] private float defaultReloadTime = 1.5f;

    public List<string> Queue { get; private set; } = new List<string>();
    public int MagazineSize { get; private set; }
    public int RoundsLeft { get; private set; }
    public bool IsReloading { get; private set; }
    public float ReloadTimeLeft { get; private set; }

    public event System.Action<string> Fired;

    private float reloadTime;

    public bool CanShoot
    {
        get { return !IsReloading && RoundsLeft > 0; }
    }

    private void Awake()
    {
        MagazineSize = defaultMagazineSize;
        reloadTime = defaultReloadTime;

        WeaponData weapon = BattleContext.Weapon;
        if (weapon != null)
        {
            MagazineSize = weapon.magazineSize;
            reloadTime = weapon.reloadTime;
        }

        RoundsLeft = MagazineSize;
        FillQueue();
    }

    private void Update()
    {
        if (!IsReloading)
            return;

        ReloadTimeLeft -= Time.deltaTime;

        if (ReloadTimeLeft <= 0f)
        {
            IsReloading = false;
            ReloadTimeLeft = 0f;
            RoundsLeft = MagazineSize;
        }
    }

    public string TakeNext()
    {
        string type = Queue[0];
        Queue.RemoveAt(0);
        FillQueue();

        RoundsLeft--;
        if (RoundsLeft <= 0)
            StartReload();

        if (Fired != null)
            Fired(type);

        return type;
    }

    private void StartReload()
    {
        IsReloading = true;
        ReloadTimeLeft = reloadTime;
    }

    private void FillQueue()
    {
        while (Queue.Count < config.ammoQueueLength)
        {
            if (Random.value < config.armorPiercingChance)
                Queue.Add("blue");
            else
                Queue.Add("red");
        }
    }
}
