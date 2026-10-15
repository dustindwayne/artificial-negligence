using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

public static class AndroidBranding {
    public static void Configure(){
        const string dir="Assets/Branding/";string path=dir+"LooperIconSource.png";
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        if(importer==null)throw new Exception("Missing Looper Android icon source");
        importer.isReadable=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.npotScale=TextureImporterNPOTScale.None;importer.mipmapEnabled=false;importer.SaveAndReimport();
        var source=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var pixels=source.GetPixels32();int left=source.width,right=0,bottom=source.height,top=0;
        for(int y=0;y<source.height;y++)for(int x=0;x<source.width;x++)if(pixels[y*source.width+x].a>24){left=Math.Min(left,x);right=Math.Max(right,x);bottom=Math.Min(bottom,y);top=Math.Max(top,y);}
        int w=right-left+1,h=top-bottom+1;float scale=Math.Min(308f/w,308f/h);int width=Mathf.RoundToInt(w*scale),height=Mathf.RoundToInt(h*scale);
        var foreground=new Color32[512*512];var background=new Color32[512*512];var legacy=new Color32[512*512];Color32 ink=new Color(.005f,.045f,.07f,1);
        for(int i=0;i<background.Length;i++)background[i]=ink;
        for(int y=0;y<height;y++)for(int x=0;x<width;x++)foreground[(y+(512-height)/2)*512+x+(512-width)/2]=pixels[(bottom+Math.Min(h-1,(int)(y/scale)))*source.width+left+Math.Min(w-1,(int)(x/scale))];
        for(int i=0;i<legacy.Length;i++){Color c=foreground[i];legacy[i]=Color.Lerp(ink,new Color(c.r,c.g,c.b,1),c.a);}
        var arrays=new[]{foreground,background,legacy};var names=new[]{"AndroidForeground","AndroidBackground","AndroidLegacy"};var textures=new Texture2D[3];
        for(int n=0;n<3;n++){
            var t=new Texture2D(512,512,TextureFormat.RGBA32,false);t.SetPixels32(arrays[n]);t.Apply();string output=dir+names[n]+".png";File.WriteAllBytes(output,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);AssetDatabase.ImportAsset(output,ImportAssetOptions.ForceSynchronousImport);textures[n]=AssetDatabase.LoadAssetAtPath<Texture2D>(output);
        }
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown,new[]{textures[2]},IconKind.Any);
        foreach(var kind in PlayerSettings.GetSupportedIconKinds(NamedBuildTarget.Android)){
            var icons=PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android,kind);
            foreach(var icon in icons){if(icon.minLayerCount>1){icon.SetTexture(textures[1],0);icon.SetTexture(textures[0],1);}else icon.SetTexture(textures[2],0);}
            PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android,kind,icons);Debug.Log("AN_ICON_KIND / "+kind+" / "+icons.Length);
        }

        Debug.Log("AN_ANDROID_ICONS_PASS / explicit adaptive, legacy and round Looper icons / 60% safe area");
    }
}
