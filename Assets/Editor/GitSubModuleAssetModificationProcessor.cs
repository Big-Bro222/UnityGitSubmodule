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
            EditorUtility.DisplayDialog(
                "Deleting Blocked",
                $"Deletion prevented: {assetPath} , please do that in the editor window instead\"",
                "OK"
            );
            return AssetDeleteResult.FailedDelete;
        }
        return AssetDeleteResult.DidDelete;
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
                EditorUtility.DisplayDialog(
                    "Renaming Blocked",
                    $"Rename of GitSubModule manually is not allowed : {oldPath}, please do that in the editor window instead",
                    "OK"
                );
                return AssetMoveResult.FailedMove;
            }
        }
        else
        {
            if (_subModuleSaver.Submodules.Contains(oldPath))
            {
                EditorUtility.DisplayDialog(
                    "Moving Blocked",
                    $"GitSubModule folder cannot be moved manually: {oldPath}, please do that in the editor window instead",
                    "OK"
                );
                return AssetMoveResult.FailedMove;
            }
        }

        return AssetMoveResult.DidMove;
    }
}
