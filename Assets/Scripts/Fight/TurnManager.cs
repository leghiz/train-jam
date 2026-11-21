using UnityEngine;
using System;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    public enum Turn { Player, Enemy }

    public static TurnManager Instance { get; private set; }

    [Header("Тайминги")]
    public float playerTurnDuration = 5f;
    public float enemyAimDelay = 0.8f;
    public float enemyAfterFireDelay = 0.8f;

    [Header("Случайность")]
    public float playerTurnChance = 0.5f;
    public int maxConsecutiveEnemyTurns = 3;

    public Turn CurrentTurn { get; private set; }
    public float TimeLeft { get; private set; }

    private int consecutiveEnemyTurns = 0;

    void Awake()
    {
        if (Instance == null) 
            Instance = this;
        else 
        { 
            Destroy(gameObject); 
            return; 
        }
    }

    void Start()
    {
        StartCoroutine(TurnLoop());
    }

    IEnumerator TurnLoop()
    {
        yield return StartCoroutine(PlayerTurn());

        while (true)
        {
            if (RollPlayerTurn())
            {
                Debug.Log("ходит игрок");
                yield return StartCoroutine(PlayerTurn());
            }
            else
                yield return StartCoroutine(EnemyTurn());
        }
    }

    private bool RollPlayerTurn()
    {
        var enemies = FindObjectsOfType<EnemyShooter>();
        int alive = 0;
        foreach (var e in enemies) 
            if (e != null) 
                alive++;

        if (alive == 0) 
            return true;

        if (consecutiveEnemyTurns >= maxConsecutiveEnemyTurns)
        {
            return true;
        }

        return playerTurnChance > UnityEngine.Random.value;
    }

    IEnumerator PlayerTurn()
    {
        consecutiveEnemyTurns = 0;

        CurrentTurn = Turn.Player;
        TimeLeft = playerTurnDuration;

        while (TimeLeft > 0f)
        {
            TimeLeft -= Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator EnemyTurn()
    {
        consecutiveEnemyTurns++;

        CurrentTurn = Turn.Enemy;

        yield return new WaitForSeconds(enemyAimDelay);
        ShootRandomEnemy();
        yield return new WaitForSeconds(enemyAfterFireDelay);
    }

    private void ShootRandomEnemy()
    {
        var enemies = FindObjectsOfType<EnemyShooter>();
        var alive = System.Array.FindAll(enemies, e => e != null);

        if (alive.Length == 0) return;

        var enemy = alive[UnityEngine.Random.Range(0, alive.Length)];
        Debug.Log($"ходит {enemy.gameObject.name}");
        enemy.Shoot();

    }
}
