using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SingleSubModuleEditorWindow : EditorWindow
{
    private string _message;

    // Static entry point that receives a string
    public static void ShowWindow(string message)
    {
        var window = GetWindow<SingleSubModuleEditorWindow>();
        window.titleContent = new GUIContent("SubModuleEditor");
        window._message = message;  // set the string!
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("SubModule Path:", EditorStyles.boldLabel);
        GUILayout.Label(_message ?? "(no message)", EditorStyles.wordWrappedLabel);
    }
}
