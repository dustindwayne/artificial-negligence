using System;
using System.IO;
using System.Xml;
using UnityEditor.Android;
using UnityEngine;

// Unity 6's adaptive-only configuration otherwise emits a placeholder fallback.
public class AndroidLauncherResources:IPostGenerateGradleAndroidProject {
    public int callbackOrder {get{return 999;}}
    public void OnPostGenerateGradleAndroidProject(string path){
        string launcher=Path.Combine(Path.GetDirectoryName(path),"launcher/src/main");
        if(!Directory.Exists(launcher))throw new Exception("Android launcher module not found");
        var source=new Texture2D(2,2,TextureFormat.RGBA32,false);source.LoadImage(File.ReadAllBytes("Assets/Branding/AndroidLegacy.png"));var pixels=source.GetPixels32();
        string[] names={"ldpi","mdpi","hdpi","xhdpi","xxhdpi","xxxhdpi"};int[] sizes={36,48,72,96,144,192};
        for(int n=0;n<sizes.Length;n++){
            int size=sizes[n];var output=new Color32[size*size];for(int y=0;y<size;y++)for(int x=0;x<size;x++)output[y*size+x]=pixels[(y*source.height/size)*source.width+x*source.width/size];
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);texture.SetPixels32(output);texture.Apply();string directory=Path.Combine(launcher,"res/mipmap-"+names[n]);Directory.CreateDirectory(directory);File.WriteAllBytes(Path.Combine(directory,"app_icon.png"),texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
        }UnityEngine.Object.DestroyImmediate(source);
        string manifest=Path.Combine(launcher,"AndroidManifest.xml");var xml=new XmlDocument();xml.Load(manifest);var app=xml.SelectSingleNode("/manifest/application") as XmlElement;
        if(app==null)throw new Exception("Android application manifest missing");const string android="http://schemas.android.com/apk/res/android";
        app.SetAttribute("icon",android,"@mipmap/app_icon");app.SetAttribute("roundIcon",android,"@mipmap/app_icon");xml.Save(manifest);
        Debug.Log("AN_LAUNCHER_RESOURCES_PASS / six Looper fallback densities and explicit roundIcon / adaptive layers retained");
    }
}
