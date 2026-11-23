using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    [SerializeField] private AudioSource footstepSound;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float stepInterval = 0.33f;
    [SerializeField] private float minPitch = 0.85f;
    [SerializeField] private float maxPitch = 1.15f;

    private Rigidbody rb;
    private Animator animator;
    private Vector2 input;
    private float stepTimer;
    private int lastStep = -1;

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

        UpdateFootsteps();
    }

    private void UpdateFootsteps()
    {
        if (footstepSound == null)
            return;

        if (input.magnitude <= 0.1f)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;

        if (stepTimer > 0f)
            return;

        footstepSound.transform.position = transform.position;
        footstepSound.pitch = Random.Range(minPitch, maxPitch);
        footstepSound.clip = PickFootstep();
        footstepSound.Play();
        stepTimer = stepInterval;
    }

    private AudioClip PickFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0)
            return footstepSound.clip;

        int index = Random.Range(0, footstepClips.Length);

        if (footstepClips.Length > 1 && index == lastStep)
            index = (index + 1) % footstepClips.Length;

        lastStep = index;
        return footstepClips[index];
    }

    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(input.x, 0, input.y);

        rb.linearVelocity = movement * speed;
    }

    private void OnDisable()
    {
        input = Vector2.zero;
        stepTimer = 0f;

        if (animator != null)
            animator.SetBool("IsRunning", false);
    }
}
