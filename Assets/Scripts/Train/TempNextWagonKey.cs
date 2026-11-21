using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TrainGenerator))]
public class TempNextWagonKey : MonoBehaviour
{
    private TrainGenerator _generator;

    private void Awake()
    {
        _generator = GetComponent<TrainGenerator>();
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.fKey.wasPressedThisFrame) return;

        _generator.EnterNextWagon();
    }
}
