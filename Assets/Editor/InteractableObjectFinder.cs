using UnityEditor;
using UnityEngine;

public class InteractableObjectFinder : EditorWindow
{
    private Vector2 scrollPosition;
    private InteractableObject[] interactableObjects;

    [MenuItem("Tools/Interactable Object Finder")]
    public static void ShowWindow()
    {
        InteractableObjectFinder window = GetWindow<InteractableObjectFinder>("Interactable Object Finder");
        window.RefreshObjects();
    }

    private void RefreshObjects()
    {
        // Находим все объекты с компонентом InteractableObject
        interactableObjects = FindObjectsOfType<InteractableObject>();
        // Сортируем по uniqueID (порядковое сравнение строк)
        System.Array.Sort(interactableObjects, (a, b) => string.Compare(a.uniqueID, b.uniqueID, System.StringComparison.Ordinal));
    }

    private void OnGUI()
    {
        GUILayout.Label("Найденные InteractableObject", EditorStyles.boldLabel);

        if (GUILayout.Button("Обновить список"))
        {
            RefreshObjects();
        }

        if (interactableObjects == null || interactableObjects.Length == 0)
        {
            GUILayout.Label("Объекты не найдены.");
            return;
        }

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

        for (int i = 0; i < interactableObjects.Length; i++)
        {
            InteractableObject obj = interactableObjects[i];
            if (obj == null) continue;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"ID: {obj.uniqueID}", GUILayout.Width(250));
            GUILayout.Label($"Объект: {obj.gameObject.name}", GUILayout.Width(150));

            if (GUILayout.Button("Выбрать", GUILayout.Width(100)))
            {
                Selection.activeGameObject = obj.gameObject;
                EditorGUIUtility.PingObject(obj.gameObject);
            }

            EditorGUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }
}
