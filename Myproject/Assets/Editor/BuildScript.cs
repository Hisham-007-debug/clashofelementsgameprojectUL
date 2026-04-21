using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    public static void BuildWindows()
    {
        string[] scenes = {
            "Assets/Shayan/Scenes/TitleScene.unity",
            "Assets/Shayan/Scenes/ModeSelect.unity",
            "Assets/Shayan/Scenes/CharacterSelect.unity",
            "Assets/Shayan/Scenes/ArenaSelect.unity",
            "Assets/Shayan/Scenes/FightScene.unity"
        };

        string outputPath = System.IO.Path.GetFullPath(
            System.IO.Path.Combine(Application.dataPath, "..", "..", "Build", "ClashOfElements.exe"));

        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputPath));

        BuildPlayerOptions opts = new BuildPlayerOptions
        {
            scenes            = scenes,
            locationPathName  = outputPath,
            target            = BuildTarget.StandaloneWindows64,
            options           = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(opts);
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            Debug.Log($"BUILD SUCCESS: {outputPath}");
        else
            Debug.LogError($"BUILD FAILED: {report.summary.result}");
    }
}
