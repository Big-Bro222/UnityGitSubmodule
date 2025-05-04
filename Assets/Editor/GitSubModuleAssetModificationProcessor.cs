using System.IO;
using UnityEditor;
using UnityEngine;

public class GitSubModuleAssetModificationProcessor : AssetModificationProcessor
{
    private static SubModuleSO _subModuleSaver;
    
    static GitSubModuleAssetModificationProcessor()
    {
        _subModuleSaver = SubModuleSO.LoadOrCreate();
    }
    //Disable Asset deleting
    public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
    {
        if (_subModuleSaver.Submodules.Contains(assetPath))
        {
            int option=EditorUtility.DisplayDialogComplex(
                "Deleting Blocked",
                $"Deletion prevented: {assetPath} , please do that in the editor window instead\"",
                "OK",
                "Cancel",
                "Open Editor Window"
            );
            switch (option)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    GitSubmoduleStatusEditorWindow.ShowWindow();
                    break;
                default:
                    Debug.LogError("Unrecognized option.");
                    break;
            }
            return AssetDeleteResult.FailedDelete;
        }
        return AssetDeleteResult.DidNotDelete;
    }
    
    //Disable Asset Moving
    public static AssetMoveResult OnWillMoveAsset(string oldPath, string newPath)
    {
        // Detect if this is a rename (same directory, different name)
        bool isRename = Path.GetDirectoryName(oldPath) == Path.GetDirectoryName(newPath);

        if (isRename)
        {
            if (_subModuleSaver.Submodules.Contains(oldPath))
            {
                int option=EditorUtility.DisplayDialogComplex(
                    "Renaming Blocked",
                    $"Rename of GitSubModule manually is not allowed : {oldPath}, please do that in the editor window instead",
                    "OK",
                    "Cancel",
                    "Open Editor Window"
                );
                switch (option)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        GitSubmoduleStatusEditorWindow.ShowWindow();
                        break;
                    default:
                        Debug.LogError("Unrecognized option.");
                        break;
                }
                return AssetMoveResult.FailedMove;
            }
        }
        else
        {
            if (_subModuleSaver.Submodules.Contains(oldPath))
            {
                int option=EditorUtility.DisplayDialogComplex(
                    "Moving Blocked",
                    $"GitSubModule folder cannot be moved manually: {oldPath}, please do that in the editor window instead",
                    "OK",
                    "Cancel",
                    "Open Editor Window"
                );
                switch (option)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        GitSubmoduleStatusEditorWindow.ShowWindow();
                        break;
                    default:
                        Debug.LogError("Unrecognized option.");
                        break;
                }
                return AssetMoveResult.FailedMove;
            }
        }

        return AssetMoveResult.DidNotMove;
    }
}
