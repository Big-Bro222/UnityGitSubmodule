using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SubModuleSO : ScriptableObject
{
    public Texture2D SubmoduleIcon_Default = default;
    public Texture2D SubmoduleIcon_Ahead = default;
    public Texture2D SubmoduleIcon_Behind = default;
    public Texture2D SubmoduleIcon_AheadAndBehind = default;
    public Texture2D SubmoduleIcon_Unstaged = default;
    public List<string> Submodules=new();
    public const string AssetPath = "Assets/Editor/GitSubModuleConfig.asset";
    public readonly Dictionary<string, Texture2D> DesignatedIconDic = new();

    public static SubModuleSO LoadOrCreate()
    {
        var settings = AssetDatabase.LoadAssetAtPath<SubModuleSO>(AssetPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<SubModuleSO>();
            AssetDatabase.CreateAsset(settings, AssetPath);
            AssetDatabase.SaveAssets();
        }
        return settings;
    }

    public void SetSubModuleIcon(GitSubmoduleStatus submoduleStatus,string repoPath)
    {
        switch (submoduleStatus)
        {
            case GitSubmoduleStatus.Ahead:
                DesignatedIconDic[repoPath] = SubmoduleIcon_Ahead;
                break;
            case GitSubmoduleStatus.Behind:
                DesignatedIconDic[repoPath] = SubmoduleIcon_Behind;
                break;
            case GitSubmoduleStatus.AheadAndBehind:
                DesignatedIconDic[repoPath] = SubmoduleIcon_AheadAndBehind;
                break;
            case GitSubmoduleStatus.Unstaged:
                DesignatedIconDic[repoPath] = SubmoduleIcon_Unstaged;
                break;
            default:
                DesignatedIconDic[repoPath] = SubmoduleIcon_Default;
                break;
        }
    }
}

public enum GitSubmoduleStatus
{
    Default,
    Ahead,
    Behind,
    AheadAndBehind,
    Unstaged
}
