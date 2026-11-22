using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public enum Turn { Player, Enemy }

    public static TurnManager Instance { get; private set; }

    [Header("Конфиг")]
    public BattleConfig config;

    public Turn CurrentTurn { get; private set; }
    public float TimeLeft { get; private set; }
    public List<Turn> Queue { get; private set; } = new List<Turn>();
    public bool AttackChosen { get; private set; }

    private int consecutiveEnemyTurns = 0;
    private int consecutivePlayerTurns = 0;
    private bool isPlayerTurnOver;

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
        FillQueue();

        while (true)
        {
            Turn turn = Queue[0];
            Queue.RemoveAt(0);
            AddNextTurn();

            if (turn == Turn.Player)
            {
                Debug.Log("ходит игрок");
                yield return StartCoroutine(PlayerTurn());
            }
            else
                yield return StartCoroutine(EnemyTurn());
        }
    }

    private void FillQueue()
    {
        Queue.Clear();

        if (BattleContext.EnemyAttackedFirst)
            AddTurn(Turn.Enemy);
        else
            AddTurn(Turn.Player);

        while (Queue.Count < config.turnQueueLength)
            AddNextTurn();
    }

    private void AddNextTurn()
    {
        if (RollPlayerTurn())
            AddTurn(Turn.Player);
        else
            AddTurn(Turn.Enemy);
    }

    private void AddTurn(Turn turn)
    {
        if (turn == Turn.Enemy)
        {
            consecutiveEnemyTurns++;
            consecutivePlayerTurns = 0;
        }
        else
        {
            consecutivePlayerTurns++;
            consecutiveEnemyTurns = 0;
        }

        Queue.Add(turn);
    }

    private bool RollPlayerTurn()
    {
        if (consecutivePlayerTurns >= config.maxConsecutivePlayerTurns)
            return false;

        if (consecutiveEnemyTurns >= config.maxConsecutiveEnemyTurns)
        {
            return true;
        }

        return config.playerTurnChance > UnityEngine.Random.value;
    }

    IEnumerator PlayerTurn()
    {
        CurrentTurn = Turn.Player;
        TimeLeft = config.playerTurnDuration;
        AttackChosen = false;
        isPlayerTurnOver = false;

        while (!AttackChosen && !isPlayerTurnOver)
            yield return null;

        while (TimeLeft > 0f && !isPlayerTurnOver)
        {
            TimeLeft -= Time.deltaTime;
            yield return null;
        }
    }

    public void ChooseAttack()
    {
        if (CurrentTurn == Turn.Player)
            AttackChosen = true;
    }

    public void EndPlayerTurn()
    {
        if (CurrentTurn == Turn.Player)
            isPlayerTurnOver = true;
    }

    IEnumerator EnemyTurn()
    {
        CurrentTurn = Turn.Enemy;

        yield return new WaitForSeconds(config.enemyAimDelay);
        ShootRandomEnemy();
        yield return new WaitForSeconds(config.enemyAfterFireDelay);
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
