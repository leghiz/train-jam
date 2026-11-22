using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimFollow : MonoBehaviour
{
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float aimDistance = 30f;

    private void Update()
    {
        Camera cam = Camera.main;
        Mouse mouse = Mouse.current;

        if (cam == null || mouse == null)
            return;

        Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());
        Vector3 aimPoint = ray.origin + ray.direction * aimDistance;

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, aimDistance, ~0, QueryTriggerInteraction.Ignore) && hit.transform.root != transform)
            aimPoint = hit.point;

        Vector3 direction = aimPoint - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion target = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, turnSpeed * Time.deltaTime);
    }
}
