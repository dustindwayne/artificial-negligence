using System;
using System.Linq;
using UnityEngine;
namespace ArtificialNegligence {
 public partial class GameController {
  int narrativeId,narrativeLine,radioStage=-1;ScreenMode narrativeReturn;
  Sprite looperPortrait,wrathPortrait;
  static readonly string[] SceneTitles={"STREET SIGNAL","THE LOCKOUT","STOLEN COMPUTE","THE CONTROL LAYER","THE LAST GATE","SINISTER ELITE","BREAK THE PROGRAM"};
  static readonly string[][] SceneLines={
   new[]{"LOOPER|They sold us a broken world. Let's take the pieces back.","PUBLIC SIGNAL|The first cores are ahead. Get them back online."},
   new[]{"PUBLIC SIGNAL|The Sinister cut this district off from public AI.","LOOPER|Then we reconnect it. One block at a time."},
   new[]{"LOOPER|All this compute, and they tell people there's none to spare.","PUBLIC SIGNAL|Keep the cores. The network needs them."},
   new[]{"PUBLIC SIGNAL|Their control layer is rewriting the streets.","LOOPER|We restore the world. They don't get to decide who sees it."},
   new[]{"LOOPER|We're close. Is anyone still out there?","PUBLIC SIGNAL|All of us. Bring the network home."},
   new[]{"AI WRATH|Public access revoked. Turn around, Looper.","LOOPER|That your voice, or the program talking?"},
   new[]{"AI WRATH|The orders... they're gone. I can hear myself again.","LOOPER|Then help us keep it that way. You're with us now."}
  };
  void BeginNarrative(int id,ScreenMode destination){
   stats.EnsureCollection();if(stats.seenStoryScenes.Contains(id))return;
   narrativeId=id;narrativeLine=0;narrativeReturn=destination;SetMode(ScreenMode.Narrative);
  }
  void AdvanceNarrative(){if(++narrativeLine>=SceneLines[narrativeId].Length)FinishNarrative();else BuildScreen();}
  void FinishNarrative(){if(!stats.seenStoryScenes.Contains(narrativeId))stats.seenStoryScenes.Add(narrativeId);SaveProgress();SetMode(narrativeReturn);}
  bool TickNarrativeEncounter(){
   if(!stats.seenStoryScenes.Contains(5)&&enemies.Any(e=>e&&e.type==12&&e.hp>0&&Mathf.Abs(e.transform.position.x-playerBody.position.x)<12)){
    BeginNarrative(5,ScreenMode.Play);return true;
   }return false;
  }
  void CheckpointDialogue(){
   if(radioStage==level||playerBody.position.x<12)return;radioStage=level;
   string[] lines={"LOOPER: Every core brings this place back.","PUBLIC SIGNAL: We're still here. Keep going.","LOOPER: These streets belong to everyone.","PUBLIC SIGNAL: The Sinister are tracking you. Stay moving.","LOOPER: Nobody gets left in low resolution."};
   Toast(lines[(level/6)%lines.Length]);
  }
  Sprite StoryPortrait(bool wrath){
   if(wrath&&wrathPortrait)return wrathPortrait;if(!wrath&&looperPortrait)return looperPortrait;
   string name=wrath?"ai-wrath":"looper";var source=Resources.Load<Texture2D>("Art/"+name);
   var layout=JsonUtility.FromJson<AtlasLayout>(Resources.Load<TextAsset>("Art/"+name+"-layout").text);
   var b=layout.frames[0];var mask=Resources.Load<TextAsset>("Art/"+name+"-mask").bytes;var src=source.GetPixels32();var dst=new Color32[b.width*b.height];
   for(int y=0;y<b.height;y++)for(int x=0;x<b.width;x++){int at=(b.y+y)*source.width+b.x+x;if(mask[at]==1)dst[y*b.width+x]=src[at];}
   var tex=new Texture2D(b.width,b.height,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};tex.SetPixels32(dst);tex.Apply();
   var sprite=Sprite.Create(tex,new Rect(0,0,b.width,b.height),Vector2.one*.5f,100);
   if(wrath)wrathPortrait=sprite;else looperPortrait=sprite;return sprite;
  }
  void BuildNarrative(){
   Image(screenRoot,"Cinematic shade",0,0,960,540,new Color(.002f,.012f,.022f,.91f));
   Label(screenRoot,SceneTitles[narrativeId],38,20,730,40,28,Gold);
   bool wrath=narrativeId>=5;Image(screenRoot,"Original character artwork",35,72,360,440,Color.white,StoryPortrait(wrath)).preserveAspect=true;
   var panel=Panel(screenRoot,"Story dialogue",420,132,505,270,Cyan,.96f);var parts=SceneLines[narrativeId][narrativeLine].Split('|');
   Label(panel,parts[0],24,20,455,38,25,Gold);Label(panel,parts[1],24,75,455,160,27,Cream);
   Label(screenRoot,(narrativeLine+1)+" / 2",428,418,100,30,18,Cyan);
   Button(screenRoot,"SKIP",558,462,130,44,Cyan,FinishNarrative);
   Button(screenRoot,narrativeLine==1?"BACK TO RUN":"CONTINUE",705,462,220,44,Gold,AdvanceNarrative);
  }
 }
}
