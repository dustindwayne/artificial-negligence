using UnityEngine;
using UnityEngine.UI;
namespace ArtificialNegligence {
 public partial class GameController {
  [System.Serializable] class WaterShape {public Vector2[] points;}
  [System.Serializable] class WaterLayout {public WaterShape[] polygons;}
  Material mapWaterMaterial;Texture2D mapWaterMask;
  static bool WithinWater(Vector2 p,Vector2[] polygon){bool inside=false;for(int i=0,j=polygon.Length-1;i<polygon.Length;j=i++){var a=polygon[i];var b=polygon[j];if((a.y>p.y)!=(b.y>p.y)&&p.x<(b.x-a.x)*(p.y-a.y)/(b.y-a.y)+a.x)inside=!inside;}return inside;}
  void BuildMapWater(RectTransform parent){
   if(mapWaterMaterial){Destroy(mapWaterMaterial);mapWaterMaterial=null;}if(mapWaterMask){Destroy(mapWaterMask);mapWaterMask=null;}
   string name=new[]{"fractured-city","market","transit","gardens","spire"}[mapDistrict];var data=Resources.Load<TextAsset>("Art/"+name+"-water");if(!data)return;
   var layout=JsonUtility.FromJson<WaterLayout>(data.text);if(layout.polygons==null||layout.polygons.Length==0)return;
   var mask=new Texture2D(512,256,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};var colors=new Color32[512*256];
   for(int y=0;y<256;y++)for(int x=0;x<512;x++){var point=new Vector2((x+.5f)/512,1-(y+.5f)/256);foreach(var shape in layout.polygons)if(WithinWater(point,shape.points)){colors[y*512+x]=new Color32(255,255,255,255);break;}}
   mask.SetPixels32(colors);mask.Apply();mapWaterMask=mask;var image=Rect(parent,"Moving canal reflections",0,0,MapArtWidth,MapArtHeight).gameObject.AddComponent<RawImage>();image.texture=mask;image.raycastTarget=false;mapWaterMaterial=new Material(Resources.Load<Shader>("MapWater"));image.material=mapWaterMaterial;
  }
 }
}
