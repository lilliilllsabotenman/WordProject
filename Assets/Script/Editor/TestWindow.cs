using UnityEditor;
using UnityEngine;

public class TestInspectorWindow : EditorWindow
{
    private Object targetObject;

    [MenuItem("Tools/Test Inspector")]
    public static void Open()
    {
        GetWindow<TestInspectorWindow>();
    }

    private void OnGUI()
    {
        targetObject = EditorGUILayout.ObjectField(
            "Target",
            targetObject,
            typeof(Object),
            false);

        if (targetObject == null)
            return;

        SerializedObject so = new SerializedObject(targetObject);
        SerializedProperty prop = so.GetIterator();

        bool enterChildren = true;

        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;

            EditorGUILayout.PropertyField(
                prop,
                true);
        }

        so.ApplyModifiedProperties();
    }
}