using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class SubModuleInfoFetcher
{
    public static double LastRefreshTime => lastRefreshTime;
    private static double lastRefreshTime = 0;
    private static SubModuleSO _submoduleSaver;
    
    static SubModuleInfoFetcher()
    {
        _submoduleSaver = SubModuleSO.LoadOrCreate();
    }
    
    private static List<SubmoduleInfo> FetchSubModuleInfo()
    {
        List<SubmoduleInfo> submodules = new List<SubmoduleInfo>();
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;

        string output = RunGitCommand(
            "submodule foreach \"branch=$(git rev-parse --abbrev-ref HEAD); " +
            "status=$(git rev-list --left-right --count @{u}...HEAD 2>/dev/null || echo 0 0); " +
            "changes=$(git status --porcelain); " +
            "if [ -n \\\"$changes\\\" ]; then local_changes=YES; else local_changes=NO; fi; " +
            "echo \\\"$path | $branch | ${status%% *} | ${status##* } | $local_changes\\\"\"",
            projectRoot
        );

        if (string.IsNullOrEmpty(output))
            return submodules;

        var lines = output.Split('\n');

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.StartsWith("Entering '")) continue; // git foreach outputs "Entering 'path'", ignore it

            var parts = line.Split('|');
            if (parts.Length != 5) continue;

            var info = new SubmoduleInfo
            {
                Path = parts[0].Trim(),
                Branch = parts[1].Trim(),
                CommitsBehind = int.TryParse(parts[2].Trim(), out int behind) ? behind : 0,
                CommitsAhead = int.TryParse(parts[3].Trim(), out int ahead) ? ahead : 0,
                HasLocalChanges = parts[4].Trim().Equals("YES")
            };

            submodules.Add(info);
        }

        return submodules;
    }
    
    public static string RunGitCommand(string arguments, string relativePath)
    {
        try
        {
            string workingDir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, relativePath);

            var startInfo = new ProcessStartInfo("git", arguments)
            {
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using (var process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output.Trim();
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"Git error in [{relativePath}]: {ex.Message}");
            return null;
        }
    }
    
    public static List<SubmoduleInfo> RefreshSubmodulesModel()
    {
        lastRefreshTime = EditorApplication.timeSinceStartup;
        List<SubmoduleInfo> submoduleInfos = SubModuleInfoFetcher.FetchSubModuleInfo();
        if (submoduleInfos.Count == 0)
        {
            return submoduleInfos;
        }
        _submoduleSaver.Submodules.Clear();
        foreach (var submoduleInfo in submoduleInfos)
        {
            _submoduleSaver.Submodules.Add(submoduleInfo.Path);
        }
        EditorUtility.SetDirty(_submoduleSaver);
        AssetDatabase.SaveAssets();
        return submoduleInfos;
    }
}



public class SubmoduleInfo
{
    public string Path;
    public string Branch;
    public int CommitsBehind;
    public int CommitsAhead;
    public bool HasLocalChanges;
}
