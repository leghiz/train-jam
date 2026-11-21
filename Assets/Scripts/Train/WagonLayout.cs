using UnityEngine;

public class WagonLayout : MonoBehaviour
{
    [Header("Вход и выход")]
    public Transform entry;

    public Transform exit;

    public Transform playerSpawn;

    [Header("Пол")]
    [Min(0.5f)] public float floorWidth = 9.6f;

    [Min(0f)] public float doorClearance = 1.2f;

    public Furniture[] GetFurniture() => GetComponentsInChildren<Furniture>(true);

    public Rect GetFloorRect()
    {
        if (!entry || !exit) return default;
        var a = transform.InverseTransformPoint(entry.position);
        var b = transform.InverseTransformPoint(exit.position);
        float zCenter = (a.z + b.z) / 2f;
        return new Rect(Mathf.Min(a.x, b.x), zCenter - floorWidth / 2f, Mathf.Abs(b.x - a.x), floorWidth);
    }

    public float GetFloorHeight() => entry ? transform.InverseTransformPoint(entry.position).y : 0f;

    private void OnDrawGizmos()
    {
        if (entry)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(entry.position, Vector3.one * 0.5f);
        }
        if (exit)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(exit.position, Vector3.one * 0.5f);
        }
        if (playerSpawn)
        {
            Gizmos.color = new Color(0.7f, 1f, 0.2f, 0.9f);
            Gizmos.DrawSphere(playerSpawn.position + Vector3.up * 0.5f, 0.3f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!entry || !exit) return;

        var floor = GetFloorRect();
        float y = GetFloorHeight() + 0.02f;
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.9f);
        Gizmos.DrawWireCube(new Vector3(floor.center.x, y, floor.center.y), new Vector3(floor.width, 0f, floor.height));

        float d = Mathf.Min(doorClearance, floor.width / 2f);
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.25f);
        Gizmos.DrawCube(new Vector3(floor.xMin + d / 2f, y, floor.center.y), new Vector3(d, 0f, floor.height));
        Gizmos.DrawCube(new Vector3(floor.xMax - d / 2f, y, floor.center.y), new Vector3(d, 0f, floor.height));

        Gizmos.matrix = Matrix4x4.identity;
    }
}
