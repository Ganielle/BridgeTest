using UnityEngine;

[ExecuteAlways]
public class BoundsLogger : MonoBehaviour
{
    private void Start()
    {
        Log();
    }

    [ContextMenu("Log Bounds")]
    public void Log()
    {
        Bounds bounds = GetCombinedBounds();
        Debug.Log($"{gameObject.name}\nSize   : {bounds.size}\n Center : {bounds.center}\nX={bounds.size.x:F4}  Y={bounds.size.y:F4}  Z={bounds.size.z:F4}" + $"  " + $" " + $"  ", gameObject);
    }

    private Bounds GetCombinedBounds()
    {
        var renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return new Bounds(transform.position, Vector3.zero);

        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            combined.Encapsulate(renderers[i].bounds);

        return combined;
    }
}
