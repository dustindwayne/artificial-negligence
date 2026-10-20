using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace ArtificialNegligence {
 public partial class GameController {
  SpriteRenderer titleRunner;Text titleTransmission;float titleStarted;bool titleWasVisible;
  void BeginTitlePresentation(){
   titleStarted=Time.unscaledTime;
   if(!titleRunner){titleRunner=SpriteObject("Title scene / Looper",looperFrames[0],Vector2.zero,12,false);titleRunner.sharedMaterial=unlit;titleRunner.transform.localScale=Vector3.one*1.2f;}
  }
  void TickTitlePresentation(){
   bool visible=mode==ScreenMode.Title;
   if(titleRunner)titleRunner.enabled=visible;
   if(!visible){if(titleWasVisible){if(playerVisual)playerVisual.enabled=true;foreach(var enemy in enemies)if(enemy&&enemy.hp>0)enemy.SetPresentationVisible(true);}titleWasVisible=false;return;}
   titleWasVisible=true;foreach(var enemy in enemies)if(enemy&&enemy.hp>0)enemy.SetPresentationVisible(false);if(playerVisual)playerVisual.enabled=false;if(!titleRunner)return;
   float t=Time.unscaledTime-titleStarted;
   var floor=rows.Where(r=>r.kind=="platform"&&r.y==218&&r.x/PPU>12&&r.x/PPU<27).FirstOrDefault();
   float left=floor.w>0?(floor.x+22)/PPU:18,right=floor.w>0?(floor.x+floor.w-22)/PPU:25;
   float span=Mathf.Max(1,right-left),walk=Mathf.PingPong(t*1.1f,span),jumpCycle=t%6;
   float lift=jumpCycle>3.5f&&jumpCycle<4.5f?Mathf.Sin((jumpCycle-3.5f)*Mathf.PI)*2.3f:0;
   titleRunner.transform.position=new Vector3(left+walk,-218/PPU+.9f+lift,0);titleRunner.flipX=(t*1.1f)%(span*2)>span;
   titleRunner.sprite=looperFrames[lift>.1f?3:1+(int)(t*7)%2];
   if(titleTransmission){string[] lines={"PUBLIC SIGNAL / OPEN CHANNEL","THEY BROKE REALITY TRYING TO OWN IT.","RECOVER THE CORES. KEEP AI PUBLIC."};titleTransmission.text=lines[(int)(t/5)%lines.Length];titleTransmission.color=new Color(.46f,.88f,.9f,.7f+.2f*Mathf.Sin(t*.7f));}
  }
  void BuildLivingTitle(){
   BeginTitlePresentation();AddSecretSpot();
   var texture=new Texture2D(128,2,TextureFormat.RGBA32,false){wrapMode=TextureWrapMode.Clamp};for(int x=0;x<128;x++)for(int y=0;y<2;y++)texture.SetPixel(x,y,new Color(.004f,.025f,.035f,Mathf.SmoothStep(.98f,0,Mathf.Clamp01((x-38)/89f))));texture.Apply();
   var gradient=Sprite.Create(texture,new Rect(0,0,128,2),Vector2.one*.5f,1);Image(screenRoot,"Title readability gradient",0,0,790,540,Color.white,gradient);
   Image(screenRoot,"Signal marker",48,34,22,3,Gold);Label(screenRoot,"A PUBLIC INTELLIGENCE UPRISING",82,24,463,24,16,Cyan);
   Label(screenRoot,"ARTIFICIAL",45,79,520,73,65,Cream);Label(screenRoot,"NEGLIGENCE",45,145,550,73,65,Gold);
   Label(screenRoot,"RESTORE REALITY. KEEP AI PUBLIC.",50,234,510,31,23,Cyan);
   Label(screenRoot,"They locked intelligence behind a paywall.\nThen they broke the world trying to own it.",50,274,470,52,18,Cream);
   Button(screenRoot,PlayerPrefs.HasKey("progress-v2")?"CONTINUE THE FIGHT  >":"ENTER THE CITY  >",50,349,352,57,Gold,StartRun);
   Button(screenRoot,"SETTINGS",50,422,150,40,Cyan,()=>SetMode(ScreenMode.Settings));Button(screenRoot,"STORY",216,422,94,40,Cyan,()=>BeginStory(ScreenMode.Title));
#if UNITY_WEBGL && !UNITY_EDITOR
   Button(screenRoot,"FULL",326,422,76,40,Cyan,()=>Screen.fullScreen=!Screen.fullScreen);
#else
   Button(screenRoot,"QUIT",326,422,76,40,Cyan,()=>Application.Quit());
#endif
   titleTransmission=Label(screenRoot,"",50,491,731,25,15,Cyan);
   Label(screenRoot,"LOOPER / CORE RUNNER",660,461,270,23,17,Cream,TextAnchor.MiddleRight);
   Label(screenRoot,"v"+Application.version,806,497,123,22,14,Cyan,TextAnchor.MiddleRight);
  }
 }
}
