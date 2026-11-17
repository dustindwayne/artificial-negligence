using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
public static class OverhaulPreview {
 public static void Build(){BuildGame.Configure();BuildGame.Validate();string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../outputs/Overhaul-Preview/ArtificialNegligence.exe"));Directory.CreateDirectory(Path.GetDirectoryName(output));var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/ArtificialNegligence.unity"},locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});if(result.summary.result!=BuildResult.Succeeded)throw new System.Exception("Overhaul preview build failed");Debug.Log("AN_OVERHAUL_BUILD_PASS");}
}
