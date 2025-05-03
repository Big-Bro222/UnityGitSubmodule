using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

public class GitSubmoduleStatusEditorWindow : EditorWindow
{
    private List<SubmoduleInfo> submodules = new List<SubmoduleInfo>();
    private const float refreshInterval = 10f; // seconds
    private Texture2D _submoduleIcon_Default;
    private Texture2D _submoduleIcon_Ahead;
    private Texture2D _submoduleIcon_Behind;
    private Texture2D _submoduleIcon_AheadAndBehind;
    private Texture2D _submoduleIcon_Unstaged;
    private SubModuleSO _submoduleSaver;
    private bool _pushable = false;
    private bool _pullable = false;
    private bool showIcons = false;

    [MenuItem("Window/My Tools/Git Submodule Manager")]
    public static void ShowWindow()
    {
        var window = GetWindow<GitSubmoduleStatusEditorWindow>();
        window.titleContent = new GUIContent("Git Submodules");
        window.Show();
    }

    private void OnEnable()
    {
        // Load the saved folder path when the window opens
        _submoduleSaver = SubModuleSO.LoadOrCreate();
        submodules = SubModuleInfoFetcher.RefreshSubmodulesModel();
        EditorApplication.update += OnEditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }

    private void OnEditorUpdate()
    {
        if (EditorApplication.timeSinceStartup - SubModuleInfoFetcher.LastRefreshTime > refreshInterval)
        {
            submodules = SubModuleInfoFetcher.RefreshSubmodulesModel();
            Repaint();
        }
    }

    private void OnGUI()
    {
        DrawSubModuleStatus();
        DrawIconSettings();
    }

    /// <summary>
    /// Block for showing Git SubModule status and simple git actions
    /// </summary>
    private void DrawSubModuleStatus()
    {
        if (GUILayout.Button("🔄 Manual Refresh"))
        {
            submodules = SubModuleInfoFetcher.RefreshSubmodulesModel();
        }

        GUILayout.Space(10);

        if (submodules.Count == 0)
        {
            EditorGUILayout.HelpBox("No Git submodules found. Please init a submodule first", MessageType.Info);
            return;
        }
        foreach (var sub in submodules)
        {
            DrawSubModuleBlock(sub);
        }
    }

    private void DrawSubModuleBlock(SubmoduleInfo submoduleInfo)
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Submodule:", submoduleInfo.Path);
        EditorGUILayout.LabelField("Path:", submoduleInfo.Path);
        EditorGUILayout.LabelField("Branch:", submoduleInfo.Branch ?? "(unknown)");
        switch (submoduleInfo.GitStatus)
        {
            case GitSubmoduleStatus.Unstaged:
                EditorGUILayout.HelpBox("✏️ Has local changes", MessageType.Warning);
                _pullable = false;
                _pushable = false;
                break;
            case GitSubmoduleStatus.AheadAndBehind:
                EditorGUILayout.HelpBox(
                    $"⬇️ Behind by {submoduleInfo.CommitsBehind} commits, ⬆️ ahead by {submoduleInfo.CommitsAhead}",
                    MessageType.Warning);
                _pullable = false;
                _pushable = false;
                break;
            case GitSubmoduleStatus.Behind:
                EditorGUILayout.HelpBox($"⬇️ Needs Pull ({submoduleInfo.CommitsBehind} commits behind)",
                    MessageType.Warning);
                _pullable = true;
                _pushable = false;
                break;
            case GitSubmoduleStatus.Ahead:
                EditorGUILayout.HelpBox($"⬆️ Needs Push ({submoduleInfo.CommitsAhead} commits ahead)", MessageType.Warning);
                _pullable = false;
                _pushable = true;
                break;
            default:
                EditorGUILayout.HelpBox("✅ Clean & up to date", MessageType.Info);
                _pullable = false;
                _pushable = false;
                break;
        }
        
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = _pullable;
        if (GUILayout.Button("Pull")) SubModuleInfoFetcher.RunGitCommand("pull", submoduleInfo.Path);
        GUI.enabled = true;
        GUI.enabled = _pushable;
        if (GUILayout.Button("Push")) SubModuleInfoFetcher.RunGitCommand("push", submoduleInfo.Path);
        EditorGUILayout.EndHorizontal();
        GUI.enabled = true;
        EditorGUILayout.EndVertical();
        GUILayout.Space(5);
    }

    /// <summary>
    /// Block for drawing git submodule Icons
    /// </summary>
    private void DrawIconSettings()
    {
        showIcons = EditorGUILayout.BeginFoldoutHeaderGroup(showIcons, "SubModule Folder Icon Settings");
        if (showIcons)
        {
            GUILayout.Space(10);
            // Object field that only accepts folders
            EditorGUI.BeginChangeCheck();
            _submoduleIcon_Default = (Texture2D)EditorGUILayout.ObjectField("Default",
                _submoduleSaver.SubmoduleIcon_Default, typeof(Texture2D), false);
            _submoduleIcon_Ahead = (Texture2D)EditorGUILayout.ObjectField("Ahead Commit",
                _submoduleSaver.SubmoduleIcon_Ahead, typeof(Texture2D), false);
            _submoduleIcon_Behind = (Texture2D)EditorGUILayout.ObjectField("Behind Commit",
                _submoduleSaver.SubmoduleIcon_Behind, typeof(Texture2D), false);
            _submoduleIcon_AheadAndBehind = (Texture2D)EditorGUILayout.ObjectField("Ahead&Behind Commit",
                _submoduleSaver.SubmoduleIcon_AheadAndBehind, typeof(Texture2D), false);
            _submoduleIcon_Unstaged = (Texture2D)EditorGUILayout.ObjectField("Unstaged",
                _submoduleSaver.SubmoduleIcon_Unstaged, typeof(Texture2D), false);
            if (EditorGUI.EndChangeCheck())
            {
                _submoduleSaver.SubmoduleIcon_Default = _submoduleIcon_Default;
                _submoduleSaver.SubmoduleIcon_Ahead = _submoduleIcon_Ahead;
                _submoduleSaver.SubmoduleIcon_Behind = _submoduleIcon_Behind;
                _submoduleSaver.SubmoduleIcon_AheadAndBehind = _submoduleIcon_AheadAndBehind;
                _submoduleSaver.SubmoduleIcon_Unstaged = _submoduleIcon_Unstaged;
                EditorUtility.SetDirty(_submoduleSaver);
                AssetDatabase.SaveAssets();
            }
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}