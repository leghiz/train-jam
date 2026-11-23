using System;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class TrainSway : MonoBehaviour
{
    [Header("Настройки остановки")]
    [SerializeField] private float stopDuration = 5.0f;

    [Header("Ритм стыков")]
    [SerializeField] private float cycleFrequency = 0.2f;
    [SerializeField, Range(0.05f, 0.3f)] private float secondBumpDelay = 0.085f;
    [SerializeField] private float bumpSharpness = 30f;

    [Header("Сила провала на стыке (ослаблена)")]
    [SerializeField] private float dropAmount = 0.0001f;
    [SerializeField] private float kickAngle = 0.2f;  
    [SerializeField] private float sideJoltAmount = 0.0001f; 

    [Header("Фоновое покачивание рессор (в промежутках)")]
    [SerializeField] private float idleBobAmount = 0.018f; 
    [SerializeField] private float idleBobFrequency = 3f; 
    [SerializeField] private float idleRollAngle = 0.2f;    

    private Vector3 appliedPosOffset;
    private Quaternion baseRotation;
    private bool hasBaseRotation;

    private bool isStopping;
    private float stopTimer;
    private float currentIntensity = 1f;
    private float cycleProgress;
    private float idlePhase;
    private float currentSideDir = 1f;

    public event Action OnTrainStopped;
    public bool IsStopped => currentIntensity <= 0.001f;

    private void Update()
    {
        if (appliedPosOffset != Vector3.zero)
        {
            transform.position -= appliedPosOffset;
            appliedPosOffset = Vector3.zero;
        }
    }

    private void LateUpdate()
    {
        if (!hasBaseRotation)
        {
            baseRotation = transform.rotation;
            hasBaseRotation = true;
        }

        if (isStopping)
        {
            stopTimer += Time.deltaTime;
            float t = Mathf.Clamp01(stopTimer / stopDuration);
            currentIntensity = Mathf.SmoothStep(1f, 0f, t);

            if (t >= 1f)
            {
                isStopping = false;
                currentIntensity = 0f;
                OnTrainStopped?.Invoke();
            }
        }

        if (IsStopped)
        {
            transform.rotation = baseRotation;
            return;
        }

        float speedFactor = Mathf.Lerp(0.25f, 1f, currentIntensity);
        
        cycleProgress += Time.deltaTime * cycleFrequency * speedFactor;
        if (cycleProgress >= 1f)
        {
            cycleProgress -= 1f;
            currentSideDir = -currentSideDir;
        }

        float bump1 = EvaluateBump(cycleProgress, 0.65f);
        float bump2 = EvaluateBump(cycleProgress, 0.65f + secondBumpDelay) * 0.85f;

        float totalDrop = (bump1 + bump2) * dropAmount * currentIntensity;
        float totalKick = (bump1 + bump2) * kickAngle * currentIntensity;
        float totalSide = (bump1 - bump2 * 0.5f) * sideJoltAmount * currentSideDir * currentIntensity;
        
        idlePhase += Time.deltaTime * idleBobFrequency * speedFactor;
        float idleBob = Mathf.Sin(idlePhase) * idleBobAmount * currentIntensity;
        float idleRoll = Mathf.Cos(idlePhase * 0.5f) * idleRollAngle * currentIntensity;

        float verticalOffset = idleBob - totalDrop;
        appliedPosOffset = (Vector3.up * verticalOffset) + (transform.right * totalSide);
        transform.position += appliedPosOffset;
        
        float pitchOffset = totalKick + (idleBob * 2f);
        float rollOffset = (totalKick * 0.5f * currentSideDir) + idleRoll;
        transform.rotation = baseRotation * Quaternion.Euler(pitchOffset, 0f, rollOffset);
    }

    private float EvaluateBump(float t, float bumpCenter)
    {
        float dt = t - bumpCenter;
        if (dt < 0f) return 0f;

        float impulse = dt * bumpSharpness * Mathf.Exp(1f - dt * bumpSharpness);
        return Mathf.Clamp01(impulse);
    }

    public void StopTrain()
    {
        if (!isStopping && !IsStopped)
        {
            isStopping = true;
            stopTimer = 0f;
        }
    }
}
