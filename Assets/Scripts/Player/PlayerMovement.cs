using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    
    private Rigidbody rb;
    private Animator animator;
    private Vector2 input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>(); 
    }

    private void Update()
    {
        input = Vector2.zero;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.wKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed)
            input.y += 1;

        if (Keyboard.current.sKey.isPressed ||
            Keyboard.current.downArrowKey.isPressed)
            input.y -= 1;

        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
            input.x += 1;

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
            input.x -= 1;

        input = input.normalized;
        if (animator != null)
        {
            bool isRunning = input.magnitude > 0.1f;
            animator.SetBool("IsRunning", isRunning);
        }
    }

    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(input.x, 0, input.y);

        rb.linearVelocity = movement * speed;
    }
}
