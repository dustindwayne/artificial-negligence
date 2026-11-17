using UnityEngine;
using UnityEngine.UI;
namespace ArtificialNegligence {
 public partial class GameController {
  float layoutWidth=960;Vector2Int layoutScreen;Rect layoutSafe;Rect? qaSafeArea;
  float UiWidth=>layoutWidth;
  bool WideScene=>mode==ScreenMode.Play||mode==ScreenMode.CityMap||mode==ScreenMode.Title;
  float ScreenUiWidth=>Screen.width/Mathf.Max(.01f,canvas?canvas.scaleFactor:1);
  void RefreshViewport(bool rebuild=true){
   if(!canvas)return;var safe=qaSafeArea??Screen.safeArea;float scale=Mathf.Min(safe.height/540f,safe.width/960f);canvas.scaleFactor=Mathf.Max(.01f,scale);uiRoot.anchoredPosition=(safe.center-new Vector2(Screen.width/2f,Screen.height/2f))/canvas.scaleFactor;
   float width=Mathf.Max(960,safe.width/canvas.scaleFactor);bool changed=Mathf.Abs(width-layoutWidth)>.5f||layoutScreen.x!=Screen.width||layoutScreen.y!=Screen.height||layoutSafe!=safe;layoutWidth=width;layoutScreen=new Vector2Int(Screen.width,Screen.height);layoutSafe=safe;
   if(changed){activeGraphics=-1;if(rebuild&&screenRoot)BuildScreen();}
  }
  void AnchorPlayRect(Transform parent,ref float x,ref float width){
   if(parent!=screenRoot||mode!=ScreenMode.Play)return;float extra=UiWidth-960;if(Mathf.Abs(width-960)<.1f){width=UiWidth;return;}if(x>=600)x+=extra;else if(x>=235)x+=extra*.5f;
  }
  Material warmUiMaterial,playHudMaterial;
  Material PlayHudMaterial(){if(!playHudMaterial)playHudMaterial=new Material(WarmUiMaterial());playHudMaterial.SetFloat("_Levels",stats.GraphicsTier==0?2:stats.GraphicsTier==1?4:0);return playHudMaterial;}
  Material WarmUiMaterial(){if(!warmUiMaterial)warmUiMaterial=new Material(Resources.Load<Shader>("WarmPixelUI"));return warmUiMaterial;}
 }
}
