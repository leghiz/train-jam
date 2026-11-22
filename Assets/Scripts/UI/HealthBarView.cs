using UnityEngine;
using UnityEngine.UI;

public class HealthBarView : MonoBehaviour
{
    [SerializeField] private Image fill;

    private PlayerStats player;

    private void Update()
    {
        if (player == null)
            player = FindBattlePlayer();

        if (player == null || player.MaxHealth <= 0f)
            return;

        fill.fillAmount = Mathf.Clamp01(player.Health / player.MaxHealth);
    }

    private PlayerStats FindBattlePlayer()
    {
        PlayerStats[] players = FindObjectsByType<PlayerStats>();

        foreach (PlayerStats candidate in players)
        {
            if (candidate.gameObject.scene == gameObject.scene)
                return candidate;
        }

        return null;
    }
}
