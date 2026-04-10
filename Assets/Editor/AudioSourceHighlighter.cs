using UnityEditor;
using UnityEngine;

public class AudioSourceFinderWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private AudioSource[] audioSources;

    [MenuItem("Tools/AudioSource Finder")]
    public static void ShowWindow()
    {
        AudioSourceFinderWindow window = GetWindow<AudioSourceFinderWindow>("AudioSource Finder");
        window.RefreshAudioSources();
    }

    private void RefreshAudioSources()
    {
        audioSources = FindObjectsOfType<AudioSource>();
    }

    private void OnGUI()
    {
        GUILayout.Label("Найденные AudioSource в сцене", EditorStyles.boldLabel);

        if (GUILayout.Button("Обновить список"))
        {
            RefreshAudioSources();
        }

        if (audioSources == null || audioSources.Length == 0)
        {
            GUILayout.Label("AudioSource не найдены.");
            return;
        }

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource == null) continue;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(audioSource.gameObject.name, GUILayout.Width(200));

            if (GUILayout.Button("Выбрать", GUILayout.Width(100)))
            {
                Selection.activeGameObject = audioSource.gameObject;
                EditorGUIUtility.PingObject(audioSource.gameObject);
            }

            EditorGUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }
}
