using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

public static class BuildWeb {
 public static void Build(){
  BuildGame.Configure();BuildGame.Validate();
  if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL,BuildTarget.WebGL))throw new Exception("Web Build Support is missing");
  foreach(string path in AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/Resources/Music"})){
   var importer=AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(path)) as AudioImporter;if(!importer)continue;
   var sample=importer.defaultSampleSettings;sample.loadType=AudioClipLoadType.CompressedInMemory;sample.preloadAudioData=true;sample.quality=.2f;
   importer.SetOverrideSampleSettings("WebGL",sample);importer.SaveAndReimport();
  }
  PlayerSettings.SetScriptingBackend(NamedBuildTarget.WebGL,ScriptingImplementation.IL2CPP);
  PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL,ManagedStrippingLevel.Low);
  PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;
  PlayerSettings.WebGL.decompressionFallback=true;PlayerSettings.WebGL.dataCaching=true;
  PlayerSettings.WebGL.nameFilesAsHashes=true;PlayerSettings.WebGL.initialMemorySize=512;
  PlayerSettings.WebGL.maximumMemorySize=2048;
  PlayerSettings.WebGL.template="APPLICATION:Default";
  PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL,new[]{GraphicsDeviceType.OpenGLES3});
  PlayerSettings.runInBackground=false;
  var output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../ArtificialNegligence-Web"));
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/ArtificialNegligence.unity"},locationPathName=output,target=BuildTarget.WebGL,options=BuildOptions.None});
  if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Web build failed: "+report.summary.result);
  Debug.Log("AN_WEB_BUILD_PASS / "+output+" / "+report.summary.totalSize);
 }
}
