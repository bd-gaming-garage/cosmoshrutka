using UnityEngine;

public class FenceBuilder : MonoBehaviour
{
    [SerializeField] private GameObject fenceSegmentPrefab;
    [SerializeField] private float sideLength = 20f;
    [SerializeField] private float segmentLength = 2f;
    [SerializeField] private float segmentHeight = 2f;
    [SerializeField] private bool buildOnStart = true;

    private void Start()
    {
        if (buildOnStart) Build();
    }

    [ContextMenu("Build")]
    public void Build()
    {
        Clear();

        float half = sideLength * 0.5f;

        BuildSide(Vector3.forward, Vector3.right, new Vector3(0f, 0f, half));
        BuildSide(Vector3.right, Vector3.forward, new Vector3(half, 0f, 0f));
        BuildSide(Vector3.back, Vector3.right, new Vector3(0f, 0f, -half));
        BuildSide(Vector3.left, Vector3.forward, new Vector3(-half, 0f, 0f));
    }

    private void BuildSide(Vector3 outward, Vector3 along, Vector3 offset)
    {
        int count = Mathf.Max(1, Mathf.RoundToInt(sideLength / segmentLength));
        float step = sideLength / count;

        Quaternion rot = Quaternion.LookRotation(outward, Vector3.up);
        Vector3 start = offset - along * sideLength * 0.5f + Vector3.up * segmentHeight * 0.5f;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = start + along * (step * (i + 0.5f));
            var seg = Instantiate(fenceSegmentPrefab, pos, rot, transform);
            seg.name = $"Fence_{i}";
        }
    }

    private void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }
    }
}
