using UnityEngine;

[CreateAssetMenu(fileName = "BattleTransition", menuName = "TrainJam/Battle Transition")]
public class BattleTransitionConfig : ScriptableObject
{
    [Header("Рывок")]
    [Min(0.05f)] public float zoomSeconds = 0.7f;
    [Range(1f, 30f)] public float punchFov = 8f;
    public float aimHeight = 1.2f;

    [Header("Проявление")]
    [Min(0.05f)] public float settleSeconds = 1.6f;
    [Range(1f, 60f)] public float revealFov = 22f;
    [Range(0f, 0.9f)] public float hudDelay = 0.45f;
    [Range(0f, 1f)] public float flash = 0.35f;
    [Min(0.01f)] public float flashSeconds = 0.45f;
}
