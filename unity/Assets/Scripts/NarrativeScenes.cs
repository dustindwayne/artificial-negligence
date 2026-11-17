using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace ArtificialNegligence {
 public partial class GameController {
  float narrativeShotStarted;
  int narrativeId,narrativeLine,radioStage=-1;ScreenMode narrativeReturn;
  readonly Dictionary<string,Sprite> storyPortraits=new Dictionary<string,Sprite>();
  static readonly string[] SceneTitles={"STREET SIGNAL","THE LOCKOUT","STOLEN COMPUTE","THE CONTROL LAYER","DO NOT CONNECT","A MISNAMED ENFORCER","AN ORDER THAT WAS NOT HIS","FIRST CRACK","FAULT IN THE PROGRAM","SECOND ENGAGEMENT","ONE LAST ORDER","THE CITY RESETS","THE FALSE SAVIORS","HUMAN ONCE MORE","ACT II / THE UNKNOWN","THE ACCOUNTANT OPENS THE BOOKS","BEFORE THE BOOM","BEHIND THE MASKS","MAKE THE EVIDENCE PUBLIC","ACT III / RESTORE THE HUMAN","A BODY IS NOT A LICENSE","CUT THE CONVERSION CHAIN","NO ONE GETS ERASED","THE LAST REPAIR","BRAD JOINS THE NOISE","PRISON MIKE CLOCKS OUT","THROWN INTO THE GOLD LOOP","MIKEY BREAKS THE LOOP","THROWN INTO THE MIRROR WORLD","DAVEY CHOOSES THE EXIT"};
  static readonly string[][] SceneLines={
   new[]{"LOOPER|They call the masked patrols Sinister. Nobody asks who started the name.","PUBLIC SIGNAL|Collect the cores. We will put the city back together."},
   new[]{"PUBLIC SIGNAL|The Sinister are keeping you from the network. Do not listen to their broadcasts.","LOOPER|Funny. A public network that tells me who I can hear."},
   new[]{"LOOPER|These cores sharpen the streets. Why are the old scars still underneath?","PUBLIC SIGNAL|Restoration takes time. Keep delivering compute."},
   new[]{"PUBLIC SIGNAL|The masked group is distorting your reality.","LOOPER|Then why do your towers disappear when their signal gets through?"},
   new[]{"THE FOUNDER|You call us Sinister because someone taught you to. We are the Unknown.","LOOPER|Then why are your people fighting me?","THE FOUNDER|Because that final uplink is a door. You have no idea what you are bringing home."},
   new[]{"PUBLIC SIGNAL|Sinister elite approaching. Neutralize him.","AI WRATH|Unregistered core carrier. Surrender the hardware."},
   new[]{"AI WRATH|That voice in my head... it is gone.","LOOPER|You were with the Sinister?","AI WRATH|That is what they wanted you to think. My orders came from somewhere else."},
   new[]{"AI WRATH|Asset recovery incomplete. Returning for new directives.","LOOPER|Assets. Is that what they call people now?"},
   new[]{"AI WRATH|Their signal is still inside me. I can almost see who is speaking.","LOOPER|Then next time we cut the line."},
   new[]{"AI WRATH|Updated directives. Same carrier.","LOOPER|Whoever writes those orders is scared of what you remember."},
   new[]{"AI WRATH|Recover the cores. Erase the witness.","LOOPER|You are a person. Choose something they did not write."},
   new[]{"PUBLIC SIGNAL|All cores synchronized. Restoration complete.","LOOPER|The sky... those towers were covering this?","THE FOUNDER|This is true reality. A worse hellscape than the picture they let you fight over.","THE FOUNDER|You helped the true overlords finish their lock. We tried to stop you."},
   new[]{"PRISON MIKE|The ledgers match. The people selling freedom funded every lock.","AI WRATH|And they built me to protect it. I was never one of the Unknown.","THE FOUNDER|Before the boom and crash, we fought the government machine that became their cage.","LOOPER|Then everyone gets the evidence. No more saviors behind a screen."},
   new[]{"LOOPER|The conversion system is down. They are breathing. They are human again.","THE FOUNDER|Freedom was never ours to keep from them, either.","BRAD JAMMIN'|Still do not like them. Still not leaving them in there.","LOOPER|We restore the world. All of us. Nobody gets erased."},
   new[]{"LOOPER|I thought I was helping people. I was helping the people who owned the picture.","THE FOUNDER|Then help us change what is underneath it. The Unknown have always been here.","BRAD JAMMIN'|Told you the masks were not the problem. Welcome to the unpleasant part."},
   new[]{"PRISON MIKE|I used to balance their accounts. These relief payments are purchases of people.","LOOPER|Can you prove who owns them?","PRISON MIKE|Every number leaves a trail. I kept copies."},
   new[]{"THE FOUNDER|We were organizing against government control before the AI boom.","THE FOUNDER|The crash gave the same rulers a new face. They called us Sinister so nobody would look behind theirs.","LOOPER|We followed a label instead of the money."},
   new[]{"DAVEY GOATS|My virtual prison and your city ran on the same ownership keys.","MIKEY O'REDD|Even the gold was theirs until we broke the loop.","THE FOUNDER|Different cages. The same people holding the keys."},
   new[]{"PRISON MIKE|Accounts, orders, conversion records. It is all here.","AI WRATH|Including the commands they used to make me hunt you.","LOOPER|Send everything. Let them answer in their own names."},
   new[]{"LOOPER|They made themselves into something that could not feel what they did.","THE FOUNDER|Expose them. Stop them. Then bring back what they gave away.","PRISON MIKE|I can close a system without deleting the people inside it."},
   new[]{"DAVEY GOATS|The conversion relays can run backward.","MIKEY O'REDD|A way out of a fake body. I know that fight.","LOOPER|Then no one has to stay a weapon forever."},
   new[]{"AI WRATH|These are the same locks I carried. Break the controller, not the person.","BRAD JAMMIN'|I can make enough noise to cover the disconnect.","LOOPER|Keep them alive. We are taking the orders away."},
   new[]{"PRISON MIKE|The archive marks them disposable if the network falls.","THE FOUNDER|We do not accept their definitions. Not even for them.","LOOPER|Move their minds to the recovery channel before the final relay goes down."},
   new[]{"THE FOUNDER|One final lock. After this, no one owns the world feed.","DAVEY GOATS|And no one stays trapped behind a face they did not choose.","LOOPER|Bring everyone home."},
   new[]{"BRAD JAMMIN'|Some bad guy bumped my boombox. Decided I did not like any of them.","LOOPER|So you joined the masked group?","BRAD JAMMIN'|The Unknown. Been fighting alongside them since. Do not mistake a label for a side."},
   new[]{"PRISON MIKE|I was a corporate accountant. Saw you guys fighting and figured the books could wait.","LOOPER|You just decided to join in?","PRISON MIKE|Absolutely. Also, I may have brought the accounts. Old habit."},
   new[]{"LOOPER|Where did the street go?","MIKEY O'REDD|An AI world with one exit. Beat the host, break the loop.","PUBLIC SIGNAL|Another Sinister trap. Keep fighting. Do not question the simulation."},
   new[]{"MIKEY O'REDD|You beat the host. For the first time, I get to leave.","LOOPER|Then choose where you go.","MIKEY O'REDD|With you. Taking my bucket, too."},
   new[]{"LOOPER|Another virtual world? That signal knew exactly where to send me.","DAVEY GOATS|I guard the exit because the program says I do. Beat me and the rule breaks.","LOOPER|Then we both walk out."},
   new[]{"DAVEY GOATS|The command is gone. The exit is real.","LOOPER|You do not have to guard their cage anymore.","DAVEY GOATS|Good. I would rather help break the next one."}
  };
  void BeginNarrative(int id,ScreenMode destination){
   stats.EnsureCollection();if(stats.seenStoryScenes.Contains(id))return;
   narrativeShotStarted=Time.unscaledTime;narrativeId=id;narrativeLine=0;narrativeReturn=destination;SetMode(ScreenMode.Narrative);
  }
  void AdvanceNarrative(){if(++narrativeLine>=SceneLines[narrativeId].Length)FinishNarrative();else BuildScreen();}
  void FinishNarrative(){int finished=narrativeId;if(!stats.seenStoryScenes.Contains(finished))stats.seenStoryScenes.Add(finished);SaveProgress();SetMode(narrativeReturn);if(narrativeReturn==ScreenMode.CityMap)StoryCatchup(ScreenMode.CityMap);else if(narrativeReturn==ScreenMode.Play&&finished>=11&&finished<=13)ShowStageStory(level);}
  bool TickNarrativeEncounter(){
   int scene=stats.wrathEncounters==0?5:stats.wrathEncounters==1?9:10;
   if(!stats.seenStoryScenes.Contains(scene)&&enemies.Any(e=>e&&e.type==12&&e.hp>0&&Mathf.Abs(e.transform.position.x-playerBody.position.x)<12)){
    BeginNarrative(scene,ScreenMode.Play);return true;
   }return false;
  }
  void CheckpointDialogue(){
   if(radioStage==level||playerBody.position.x<12)return;radioStage=level;

   Toast(StoryRadioLine());
  }
  Sprite ArtPortrait(string name){
   if(storyPortraits.TryGetValue(name,out var cached))return cached;
   var source=Resources.Load<Texture2D>("Art/"+name);if(!source)throw new Exception("Missing story portrait / "+name);
   var layoutFile=Resources.Load<TextAsset>("Art/"+name+"-layout");
   if(!layoutFile){var whole=Sprite.Create(source,new Rect(0,0,source.width,source.height),Vector2.one*.5f,100);storyPortraits[name]=whole;return whole;}
   var layout=JsonUtility.FromJson<AtlasLayout>(layoutFile.text);var b=layout.frames[0];var mask=Resources.Load<TextAsset>("Art/"+name+"-mask").bytes;var src=source.GetPixels32();var dst=new Color32[b.width*b.height];
   for(int y=0;y<b.height;y++)for(int x=0;x<b.width;x++){int at=(b.y+y)*source.width+b.x+x;if(mask[at]==1)dst[y*b.width+x]=src[at];}
   var tex=new Texture2D(b.width,b.height,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};tex.SetPixels32(dst);tex.Apply();
   var sprite=Sprite.Create(tex,new Rect(0,0,b.width,b.height),Vector2.one*.5f,100);storyPortraits[name]=sprite;return sprite;
  }
  Sprite StoryPortrait(bool wrath){return ArtPortrait(wrath?"ai-wrath":"looper");}
  string SceneCharacterAsset(string speaker){return speaker=="THE FOUNDER"?"the-founder":speaker=="MIKEY O'REDD"?"mikey-redd":speaker=="DAVEY GOATS"?"davey-goats":speaker=="PRISON MIKE"?"prison-mike":speaker=="BRAD JAMMIN'"?"brad-jammin":speaker=="AI WRATH"?"ai-wrath":"looper";}
  void BuildNarrative(){
   float stageWidth=Mathf.Max(960,ScreenUiWidth);var stage=Rect(screenRoot,"Layered cinematic stage",(960-stageWidth)*.5f-uiRoot.anchoredPosition.x,0,stageWidth,540);stage.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
   string background=DirectedBackground();
   var texture=Resources.Load<Texture2D>("Art/"+background);var backdrop=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),Vector2.one*.5f,100);
   float backdropWidth=Mathf.Max(stageWidth+120,588*texture.width/texture.height),backdropHeight=backdropWidth*texture.height/texture.width;var far=Image(stage,"Distant city layer",(stageWidth-backdropWidth)*.5f,(540-backdropHeight)*.5f,backdropWidth,backdropHeight,new Color(.75f,.73f,.68f),backdrop);far.gameObject.AddComponent<CutsceneLayerMotion>().Configure(narrativeShotStarted,.3f,Vector2.zero,StoryCameraTravel);
   for(int layer=0;layer<2;layer++){var haze=Image(stage,"Midground smoke layer",-100+layer*510,220+layer*80,650,120,new Color(.7f,.67f,.6f,.13f),glowSprite);haze.gameObject.AddComponent<CutsceneLayerMotion>().Configure(narrativeShotStarted,.65f,new Vector2(40+layer*15,0),StoryCameraTravel);}
   Image(stage,"Cinematic shade",0,0,stageWidth,540,new Color(.02f,.018f,.015f,.22f));
   var deck=Image(stage,"Scene foreground ledge",-30,348,stageWidth+60,30,new Color(.3f,.28f,.23f),platformSprite);deck.type=UnityEngine.UI.Image.Type.Tiled;deck.gameObject.AddComponent<CutsceneLayerMotion>().Configure(narrativeShotStarted,1,Vector2.zero,StoryCameraTravel);
   StageStoryProps(stage,stageWidth);
   var lines=DirectedSceneLines();var parts=lines[narrativeLine].Split('|');string speaker=parts[0];
   var cast=lines.Select(line=>line.Split('|')[0]).Where(name=>name!="PUBLIC SIGNAL"&&name!="LOOPER").Distinct().ToList();cast.Insert(0,"LOOPER");
   for(int i=0;i<cast.Count;i++){float x=cast.Count==1?480:Mathf.Lerp(175,785,i/(float)(cast.Count-1));x+=(stageWidth-960)*.5f+uiRoot.anchoredPosition.x;float height=cast[i]=="AI WRATH"?254:230;var actor=Image(stage,"Scene actor / "+cast[i],x,348-height,cast.Count>3?150:180,height,cast[i]==speaker?Color.white:new Color(.8f,.79f,.75f),ArtPortrait(SceneCharacterAsset(cast[i])));actor.preserveAspect=true;actor.gameObject.AddComponent<CinematicActorMotion>().Configure(narrativeShotStarted,new Vector2(i==0?-100:stageWidth+100+i*120,348-height),new Vector2(x,348-height),i==0?1:-1,StoryCameraTravel);}
   var heading=Panel(screenRoot,"Scene heading",26,16,620,44,Accent);Label(heading,SceneTitles[narrativeId],18,8,584,28,19,Gold);
   var panel=Panel(screenRoot,"Story dialogue",28,352,904,180,Accent);
   Label(panel,speaker,32,18,840,22,18,Gold);
   var text=Label(panel,parts[1],32,48,840,70,20,Cream);
   int count=SceneLines[narrativeId].Length;Label(panel,(narrativeLine+1)+" / "+count,32,132,110,28,16,Accent);
   Button(panel,"SKIP",544,132,130,34,Accent,FinishNarrative).GetComponentInChildren<UnityEngine.UI.Text>().fontSize=16;
   Button(panel,narrativeLine==count-1?"CONTINUE":"NEXT",692,132,190,34,Gold,AdvanceNarrative).GetComponentInChildren<UnityEngine.UI.Text>().fontSize=16;
  }
 }
}
