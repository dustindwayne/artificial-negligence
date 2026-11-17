using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public static class AndroidBranding {
    static Color32[] Fit(Color32[] pixels,int sourceWidth,int left,int bottom,int w,int h,int extent){
        var output=new Color32[512*512];float scale=Math.Min((float)extent/w,(float)extent/h);int width=Mathf.RoundToInt(w*scale),height=Mathf.RoundToInt(h*scale);
        for(int y=0;y<height;y++)for(int x=0;x<width;x++){
            var pixel=pixels[(bottom+Math.Min(h-1,(int)(y/scale)))*sourceWidth+left+Math.Min(w-1,(int)(x/scale))];
            // The supplied white-on-black master remains untouched. Black becomes
            // transparent only in this derived launcher foreground.
            pixel.a=(byte)(pixel.a*Math.Max(pixel.r,Math.Max(pixel.g,pixel.b))/255);
            output[(y+(512-height)/2)*512+x+(512-width)/2]=pixel;
        }return output;
    }
    public static void Configure(){
        const string dir="Assets/Branding/";string path=dir+"UnknownIconSource.png";
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        if(importer==null)throw new Exception("Missing Unknown mask icon source");
        importer.isReadable=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.npotScale=TextureImporterNPOTScale.None;importer.mipmapEnabled=false;importer.SaveAndReimport();
        var source=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var pixels=source.GetPixels32();int left=source.width,right=-1,bottom=source.height,top=-1;
        for(int y=0;y<source.height;y++)for(int x=0;x<source.width;x++){var c=pixels[y*source.width+x];if(c.a>24&&Math.Max(c.r,Math.Max(c.g,c.b))>32){left=Math.Min(left,x);right=Math.Max(right,x);bottom=Math.Min(bottom,y);top=Math.Max(top,y);}}
        if(right<left||top<bottom)throw new Exception("Unknown icon has no visible emblem");
        int w=right-left+1,h=top-bottom+1;
        var foreground=Fit(pixels,source.width,left,bottom,w,h,308);
        var desktop=Fit(pixels,source.width,left,bottom,w,h,420);
        var legacy=Fit(pixels,source.width,left,bottom,w,h,400);
        var background=Enumerable.Repeat(new Color32(0,0,0,255),512*512).ToArray();
        // Opaque icons preserve the supplied black backdrop.
        foreach(var image in new[]{legacy,desktop})for(int i=0;i<image.Length;i++){Color c=image[i];image[i]=Color.Lerp(Color.black,new Color(c.r,c.g,c.b,1),c.a);}
        var arrays=new[]{foreground,background,legacy,desktop};var names=new[]{"AndroidForeground","AndroidBackground","AndroidLegacy","DesktopIcon"};var textures=new Texture2D[4];
        for(int n=0;n<arrays.Length;n++){
            var t=new Texture2D(512,512,TextureFormat.RGBA32,false);t.SetPixels32(arrays[n]);t.Apply();string output=dir+names[n]+".png";File.WriteAllBytes(output,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);AssetDatabase.ImportAsset(output,ImportAssetOptions.ForceSynchronousImport);textures[n]=AssetDatabase.LoadAssetAtPath<Texture2D>(output);
        }
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown,new[]{textures[3]},IconKind.Any);
        int[] sizes=PlayerSettings.GetIconSizesForTargetGroup(BuildTargetGroup.Standalone,IconKind.Any);
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Standalone,Enumerable.Repeat(textures[3],sizes.Length).ToArray(),IconKind.Any);
        foreach(var kind in PlayerSettings.GetSupportedIconKinds(NamedBuildTarget.Android)){
            var icons=PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android,kind);
            foreach(var icon in icons){if(icon.minLayerCount>1){icon.SetTexture(textures[1],0);icon.SetTexture(textures[0],1);}else icon.SetTexture(textures[2],0);}
            PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android,kind,icons);Debug.Log("AN_ICON_KIND / "+kind+" / "+icons.Length);
        }
        AssetDatabase.SaveAssets();Debug.Log("AN_UNKNOWN_ICONS_PASS / Android adaptive, legacy and round; Windows and Linux standalone; original supplied master preserved");
    }
}
