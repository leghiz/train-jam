using UnityEngine;

public class PlayerBattleAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsAiming", false);
        }
    }

    private void Start()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnChanged += HandleTurnChanged;
            HandleTurnChanged(TurnManager.Instance.CurrentTurn);
        }
    }

    private void OnDestroy()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.OnTurnChanged -= HandleTurnChanged;
    }

    private void HandleTurnChanged(TurnManager.Turn currentTurn)
    {
        if (animator == null) return;

        bool isPlayerTurn = (currentTurn == TurnManager.Turn.Player);
        animator.SetBool("IsAiming", isPlayerTurn);
    }
}