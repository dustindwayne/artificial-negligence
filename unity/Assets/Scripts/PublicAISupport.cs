using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace ArtificialNegligence {
 public partial class GameController {
  static readonly string[] SupportNames={"DRONE","BRIDGE","SHIELD"};
  int supportKind;float supportCooldown,supportSaveTimer,speechTime;bool supportDirty;
  GameObject speechBubble;Text speechText,supportLabel,supportKindLabel;Image supportFill;
  string speech;Transform publicDrone;float droneLife,droneShot;
  class PublicConstruct {public Transform root;public float life;}
  readonly List<PublicConstruct> publicConstructs=new List<PublicConstruct>();
  void ResetSupportScene(){publicDrone=null;publicConstructs.Clear();supportCooldown=0;speechTime=0;}
  void EarnMomentum(int amount){int before=stats.momentum;stats.AddMomentum(amount);if(before!=stats.momentum){supportDirty=true;if(before<100&&stats.momentum==100)Toast("CHAT ASSIST EARNED / "+stats.compute+" COMPUTE");}}
  public void CycleSupport(){supportKind=(supportKind+1)%SupportNames.Length;Toast("CHAT / "+SupportNames[supportKind]+" / 3 COMPUTE + FULL MOMENTUM");}
  void SayToChat(string text){speech="HEY CHAT, A LITTLE HELP.\n"+text;speechTime=3;}
  public void AskChat(){
   if(mode!=ScreenMode.Play)return;
   if(stats.momentum<100){SayToChat("EARN IT: DODGE CLOSE / DEFEAT SINISTER");return;}
   if(stats.compute<RunStats.SupportCost){SayToChat("NEED 3 COMPUTE / COLLECT GPUS");return;}
   if(supportCooldown>0){SayToChat("NETWORK RECOVERING...");return;}
   if(supportKind==0&&publicDrone){SayToChat("DRONE ALREADY ACTIVE");return;}
   if(supportKind==2&&buffs[1]>0){SayToChat("SHIELD ALREADY ACTIVE");return;}
   Vector2 bridge=playerBody.position+new Vector2(facing*2.2f,.7f);
   if(supportKind==1&&Physics2D.OverlapBox(bridge+Vector2.down*.125f,new Vector2(3,.25f),0,1<<8)){SayToChat("NO CLEAR SPACE FOR A BRIDGE");return;}
   if(!stats.SpendSupport())return;supportCooldown=8;supportDirty=false;SaveProgress();SayToChat(SupportNames[supportKind]+" / PUBLIC AI ONLINE");Sound(successSound);
   if(supportKind==0){var sr=SpriteObject("Public AI helper drone",itemFrames[7],playerBody.position+new Vector2(-facing,1.3f),15,false);sr.transform.localScale=Vector3.one*.48f;sr.color=Cyan;publicDrone=sr.transform;droneLife=10;droneShot=.3f;PointLight(publicDrone,Cyan,2,.6f);}
   else if(supportKind==1){
    var sr=SpriteObject("Public AI temporary bridge",PlatformStyle(bridge.x),bridge+Vector2.down*.125f,2,false);sr.color=Cyan;sr.drawMode=SpriteDrawMode.Tiled;sr.size=new Vector2(3,.25f);sr.gameObject.layer=8;
    var box=sr.gameObject.AddComponent<BoxCollider2D>();box.size=sr.size;box.usedByEffector=true;sr.gameObject.AddComponent<PlatformEffector2D>().useOneWay=true;publicConstructs.Add(new PublicConstruct{root=sr.transform,life=12});Burst(bridge,Cyan,12);
   }else{buffs[1]=6;var sr=SpriteObject("Public AI projected shield",glowSprite,playerBody.position,12,false);sr.transform.SetParent(playerBody.transform,true);sr.transform.localPosition=Vector3.zero;sr.transform.localScale=new Vector3(2,2,1);sr.color=new Color(Cyan.r,Cyan.g,Cyan.b,.38f);publicConstructs.Add(new PublicConstruct{root=sr.transform,life=6});}
  }
  void TickPublicAI(float dt){
   supportCooldown=Mathf.Max(0,supportCooldown-dt);speechTime=Mathf.Max(0,speechTime-dt);supportSaveTimer+=dt;if(supportDirty&&supportSaveTimer>2){supportSaveTimer=0;supportDirty=false;SaveProgress();}
   if(publicDrone){
    droneLife-=dt;droneShot-=dt;publicDrone.position=Vector3.Lerp(publicDrone.position,playerBody.transform.position+new Vector3(-facing*1.1f,1.3f+Mathf.Sin(Time.time*3)*.12f,0),1-Mathf.Exp(-dt*7));
    var target=enemies.Where(e=>e&&e.hp>0&&Vector2.Distance(e.transform.position,publicDrone.position)<8&&!Physics2D.Linecast(publicDrone.position,e.transform.position,1<<8)).OrderBy(e=>Vector2.Distance(e.transform.position,publicDrone.position)).FirstOrDefault();
    if(target&&droneShot<=0){droneShot=.65f;var aim=((Vector2)target.transform.position-(Vector2)publicDrone.position).normalized;SpawnBullet(publicDrone.position,aim*14,false,2+stats.rank/5,0,true);Burst(publicDrone.position,Cyan,3);}
    if(droneLife<=0){Burst(publicDrone.position,Cyan,10);Destroy(publicDrone.gameObject);publicDrone=null;}
   }
   for(int i=publicConstructs.Count-1;i>=0;i--){var c=publicConstructs[i];if(!c.root){publicConstructs.RemoveAt(i);continue;}c.life-=dt;if(c.life<2){var sr=c.root.GetComponent<SpriteRenderer>();if(sr){var color=sr.color;color.a=.35f+Mathf.PingPong(c.life*4,.65f);sr.color=color;}}if(c.life<=0){Destroy(c.root.gameObject);publicConstructs.RemoveAt(i);}}
  }
  void ScoreNearMiss(Bullet bullet,Vector2 previous,Vector2 next){
   if(bullet.nearMissCredited||Mathf.Abs(bullet.velocity.x)<.01f)return;var center=PlayerHitCenter;
   float t=(center.x-previous.x)/(next.x-previous.x);if(t<0||t>1)return;
   float gap=Mathf.Abs(Mathf.Lerp(previous.y,next.y,t)-center.y),half=PlayerHitSize.y*.5f;
   if(gap>half+.07f&&gap<half+.85f){bullet.nearMissCredited=true;EarnMomentum(8);}
  }
  void BuildSupportHud(){
   bool touch=Application.isMobilePlatform||qaMode;
   var b=Button(screenRoot,"",touch?381:730,touch?480:77,touch?220:212,touch?42:35,Cyan,AskChat);supportLabel=b.GetComponentInChildren<Text>();supportLabel.fontSize=18;
   supportKindLabel=Label(screenRoot,"",touch?381:730,touch?458:116,touch?220:167,20,13,Cyan,TextAnchor.MiddleCenter);
   Button(screenRoot,">",touch?610:901,touch?480:114,43,32,Cyan,CycleSupport).GetComponentInChildren<Text>().fontSize=20;
   supportFill=Image(screenRoot,"Earned momentum",touch?381:730,touch?526:152,touch?220:212,3,Cyan);
   speechBubble=Panel(screenRoot,"Looper chat bubble",0,0,280,56,Cyan,.96f).gameObject;speechText=Label(speechBubble.transform,"",10,4,260,48,15,Cream,TextAnchor.MiddleCenter);speechText.resizeTextForBestFit=true;speechText.resizeTextMinSize=11;speechText.resizeTextMaxSize=15;speechBubble.SetActive(false);
  }
  void UpdateSupportHud(){
   if(supportLabel){supportLabel.text=stats.SupportReady&&supportCooldown<=0?"CHAT / READY":"CHAT / "+stats.momentum+"%";supportLabel.color=stats.SupportReady?Gold:Cyan;}
   if(supportKindLabel)supportKindLabel.text=SupportNames[supportKind]+" / COMPUTE "+stats.compute+" / "+RunStats.ComputeCapacity;
   if(supportFill)supportFill.rectTransform.sizeDelta=new Vector2((Application.isMobilePlatform||qaMode?220:212)*stats.momentum/100f,3);
   if(speechBubble){speechBubble.SetActive(speechTime>0);if(speechTime>0){speechText.text=speech;var view=gameCamera.WorldToViewportPoint(playerBody.position+Vector2.up*2);speechBubble.GetComponent<RectTransform>().anchoredPosition=new Vector2(Mathf.Clamp(view.x*960-140,12,668),-Mathf.Clamp((1-view.y)*540-56,165,402));}}
  }
  void OnApplicationQuit(){if(!qaMode&&mode!=ScreenMode.Title)SaveProgress();}
 }
}
