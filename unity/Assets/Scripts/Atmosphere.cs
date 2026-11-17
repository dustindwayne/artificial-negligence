using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ArtificialNegligence {
 public partial class GameController {
  [System.Serializable] class LampLayout {public Vector2[] points;}
  readonly List<Image> mapMist=new List<Image>(),mapLights=new List<Image>();
  readonly List<SpriteRenderer> driftingMist=new List<SpriteRenderer>();
  void BuildMapAtmosphere(RectTransform parent){
   mapMist.Clear();mapLights.Clear();var clip=Rect(parent,"Map atmosphere",0,0,MapArtWidth,MapArtHeight);clip.gameObject.AddComponent<RectMask2D>();BuildMapWater(clip);
   for(int i=0;i<3;i++)mapMist.Add(Image(clip,"Crossfading mist layer",-200,70+i*70,520+i*70,125,Color.clear,glowSprite));
   var lightData=Resources.Load<TextAsset>("Art/"+new[]{"fractured-city","market","transit","gardens","spire"}[mapDistrict]+"-lights");var lamps=lightData?JsonUtility.FromJson<LampLayout>(lightData.text).points:new Vector2[0];
   foreach(var lamp in lamps)mapLights.Add(Image(clip,"Flickering power light",lamp.x*MapArtWidth-22,lamp.y*MapArtHeight-22,44,44,Color.clear,glowSprite));
  }
  void TickMapAtmosphere(){float t=Time.unscaledTime;if(mapWaterMaterial)mapWaterMaterial.SetFloat("_FlowTime",t);for(int i=0;i<mapMist.Count;i++){if(!mapMist[i])continue;var rt=mapMist[i].rectTransform;rt.anchoredPosition=new Vector2(-420+(t*(12+i*5)+i*310)%1250,-(60+i*75+Mathf.Sin(t*.09f+i)*12));mapMist[i].color=new Color(.7f,.71f,.68f,.15f*(.4f+.6f*(.5f+.5f*Mathf.Sin(t*.27f+i*2.094f))));}for(int i=0;i<mapLights.Count;i++)if(mapLights[i]){float light=i%3==1?.35f+Mathf.PerlinNoise(t*2.2f,i*7.1f)*.35f:.5f+Mathf.Sin(t*.3f+i)*.045f;if(i%3==1&&Mathf.PerlinNoise(t*.47f,i+13)>.67f)light*=.4f;mapLights[i].color=new Color(1,.69f,.28f,light);}}
  void BuildDriftingMist(){driftingMist.Clear();for(int i=0;i<4;i++){var sr=SpriteObject("Background smoke crossfade",glowSprite,Vector2.zero,-70+i*12,false);sr.transform.localScale=new Vector3(10+i*3,1.8f+i*.6f,1);driftingMist.Add(sr);}}
  void TickDriftingMist(){float t=Time.unscaledTime;for(int i=0;i<driftingMist.Count;i++){var sr=driftingMist[i];if(!sr)continue;sr.enabled=activeGraphics>=2;if(!sr.enabled)continue;sr.transform.position=new Vector3(gameCamera.transform.position.x-26+(t*(.17f+i*.09f)+i*14)%52,-3-i*2+Mathf.Sin(t*.07f+i)*.3f,0);sr.color=new Color(.58f,.58f,.55f,.075f*(.5f+.5f*Mathf.Sin(t*.16f+i*Mathf.PI*.5f)));}}
 }
}
