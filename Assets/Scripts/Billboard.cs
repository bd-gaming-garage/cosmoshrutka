using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private string cameraTag = "MainCamera";
    [SerializeField] private bool lockX = false;
    [SerializeField] private bool lockY = false;
    [SerializeField] private bool lockZ = false;
    [SerializeField] private bool flipForward = false;

    private Camera _camera;

    private void LateUpdate()
    {
        if (_camera == null)
        {
            var go = GameObject.FindGameObjectWithTag(cameraTag);
            if (go != null) _camera = go.GetComponent<Camera>();
            if (_camera == null) return;
        }

        Vector3 dir = transform.position - _camera.transform.position;
        if (flipForward) dir = -dir;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion rot = Quaternion.LookRotation(dir, _camera.transform.up);

        if (lockX || lockY || lockZ)
        {
            Vector3 e = rot.eulerAngles;
            if (lockX) e.x = 0f;
            if (lockY) e.y = 0f;
            if (lockZ) e.z = 0f;
            rot = Quaternion.Euler(e);
        }

        transform.rotation = rot;
    }
}