using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GrabArea))]
public class GrabAreaEditor : Editor
{
    private void OnSceneGUI()
    {
        var area = (GrabArea)target;
        serializedObject.Update();

        SerializedProperty points = serializedObject.FindProperty("points");
        Handles.color = Color.green;

        for (int i = 0; i < points.arraySize; i++)
        {
            SerializedProperty point = points.GetArrayElementAtIndex(i);
            Vector3 world = area.transform.TransformPoint(point.vector2Value);

            Handles.Label(world, i.ToString());

            EditorGUI.BeginChangeCheck();
            Vector3 moved = Handles.PositionHandle(world, area.transform.rotation);

            if (EditorGUI.EndChangeCheck())
            {
                Vector3 local = area.transform.InverseTransformPoint(moved);
                point.vector2Value = new Vector2(local.x, local.y);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
