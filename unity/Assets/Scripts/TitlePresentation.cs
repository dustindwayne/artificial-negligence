using System.Collections.Generic;
using UnityEngine;
namespace ArtificialNegligence {
 public partial class GameController {
  readonly List<UnityEngine.UI.Image> titleMistLayers=new List<UnityEngine.UI.Image>();UnityEngine.UI.Image titleSkyline;float titleCityX,titleCityY;
  void TickTitlePresentation(){if(mode!=ScreenMode.Title)return;float t=Time.unscaledTime;if(titleSkyline)titleSkyline.rectTransform.anchoredPosition=new Vector2(titleCityX+Mathf.Sin(t*.035f)*14,-titleCityY);for(int i=0;i<titleMistLayers.Count;i++)if(titleMistLayers[i]){titleMistLayers[i].rectTransform.anchoredPosition=new Vector2(-350+(t*(8+i*4)+i*470)%(UiWidth+500),-(180+i*90));titleMistLayers[i].color=new Color(.76f,.65f,.43f,.07f+.035f*Mathf.Sin(t*.2f+i*2));}}
  void BuildLivingTitle(){
   titleMistLayers.Clear();var texture=Resources.Load<Texture2D>("Art/background_settlement");var city=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),Vector2.one*.5f,100);float ratio=texture.width/(float)texture.height,width=Mathf.Max(ScreenUiWidth+120,600*ratio),height=width/ratio;titleCityX=(UiWidth-width)*.5f-uiRoot.anchoredPosition.x;titleCityY=(540-height)*.5f;
   titleSkyline=Image(screenRoot,"Ambient title city",titleCityX,titleCityY,width,height,new Color(.85f,.77f,.64f),city);titleSkyline.material=WarmUiMaterial();
   for(int i=0;i<3;i++)titleMistLayers.Add(Image(screenRoot,"Title mist",-200+i*350,180+i*90,620,140,Color.clear,glowSprite));
   Image(screenRoot,"Title shade",(UiWidth-ScreenUiWidth)*.5f-uiRoot.anchoredPosition.x,0,ScreenUiWidth,540,new Color(.015f,.012f,.009f,.35f));
   Label(screenRoot,"ARTIFICIAL\nNEGLIGENCE",(UiWidth-740)*.5f,66,740,140,54,Cream,TextAnchor.MiddleCenter);
   Label(screenRoot,"Follow the signal. Question who owns the picture.",(UiWidth-720)*.5f,262,720,40,18,Gold,TextAnchor.MiddleCenter);
   float dockX=(UiWidth-680)*.5f;Image(screenRoot,"Menu action dock",dockX,350,680,150,new Color(1,1,1,.88f),InfoFrame());
   Button(screenRoot,PlayerPrefs.HasKey("progress-v2")?"CONTINUE":"START",dockX+28,372,624,50,Gold,StartRun);
   Button(screenRoot,"SETTINGS",dockX+28,442,300,40,Accent,()=>{settingsReturn=ScreenMode.Title;SetMode(ScreenMode.Settings);});
#if UNITY_WEBGL && !UNITY_EDITOR
   Button(screenRoot,"FULLSCREEN",dockX+352,442,300,40,Accent,()=>Screen.fullScreen=!Screen.fullScreen);
#else
   Button(screenRoot,"EXIT",dockX+352,442,300,40,Accent,()=>Application.Quit());
#endif
  }
 }
}
