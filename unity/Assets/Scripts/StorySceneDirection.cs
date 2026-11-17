using System.Linq;
using UnityEngine;
namespace ArtificialNegligence {
 public partial class GameController {
  // Shot locations follow the story, rather than reusing a street for every conversation.
  static readonly string[] SceneLocations={"street","market","relay","archive","citadel","pursuit","disconnect","retreat","disconnect","pursuit","citadel","reset","evidence","restored","hideout","office","archive","simulation","broadcast","recovery","recovery","relay","archive","citadel","street","office","gold","gold","mirror","mirror"};
  Vector2 StoryCameraTravel=>narrativeId==11?new Vector2(-54,-18):narrativeId==13?new Vector2(36,-16):narrativeId%3==0?new Vector2(-48,6):narrativeId%3==1?new Vector2(36,-4):new Vector2(-20,14);
  string[] DirectedSceneLines(){return SceneLines[narrativeId].Select(RecruitAwareLine).ToArray();}
  string RecruitAwareLine(string line){
   if(narrativeId>=24)return line;string speaker=line.Split('|')[0];int id=speaker=="DAVEY GOATS"?5:speaker=="BRAD JAMMIN'"?6:speaker=="MIKEY O'REDD"?7:speaker=="PRISON MIKE"?8:-1;
   if(id<0||stats.CharacterUnlocked(id))return line;
   // The main plot is complete even when every optional branch is missed.
   if(narrativeId==12||narrativeId==18)return "LOOPER|The records are here. The relief network bought people, and every order leads back to the same owners.";
   if(narrativeId==15)return line.Contains("kept copies")?"THE FOUNDER|Every transaction leaves a trail. We have the evidence now.":"THE FOUNDER|These relief payments are purchases of people. The accounts prove it.";
   if(narrativeId==17)return speaker=="DAVEY GOATS"?"THE FOUNDER|The city and those virtual prisons use the same ownership keys.":"LOOPER|Different cages. The same people holding the keys.";
   if(narrativeId==20)return "THE FOUNDER|The conversion relays can run backward. We can free the people inside.";
   if(narrativeId==22)return "LOOPER|The archive says to erase them if the network fails. We move them to recovery first.";
   if(narrativeId==19)return "LOOPER|Close the system. Save the people inside it.";
   return "LOOPER|We are getting everyone out. Nobody gets left in their machine.";
  }
  string DirectedBackground(){string place=SceneLocations[narrativeId];return place=="gold"||place=="mirror"||place=="simulation"||place=="relay"||place=="recovery"?"background_datafarm":place=="citadel"||place=="reset"||place=="archive"||place=="office"||place=="evidence"||place=="broadcast"?"background_citadel":"background_settlement";}
  void StageStoryProps(RectTransform stage,float width){
   string place=SceneLocations[narrativeId];bool virtualWorld=place=="gold"||place=="mirror"||place=="simulation";
   if(virtualWorld){
    var wash=Image(stage,"Virtual world color",0,0,width,350,place=="gold"?new Color(.42f,.23f,.02f,.25f):new Color(.12f,.1f,.35f,.28f));
    for(int i=0;i<5;i++){var core=Image(stage,"Suspended simulation core",width*.15f+i*width*.16f,100+(i%2)*50,32,32,new Color(1,1,1,.55f),itemFrames[0]);core.gameObject.AddComponent<CutsceneLayerMotion>().Configure(narrativeShotStarted,.6f,new Vector2(0,-18),StoryCameraTravel);}
   }
   bool terminal=place=="office"||place=="archive"||place=="evidence"||place=="broadcast"||place=="recovery";
   if(terminal){var machine=Image(stage,"Scene network console",width*.5f-64,220,128,128,Color.white,itemFrames[2]);machine.preserveAspect=true;machine.gameObject.AddComponent<CutsceneLayerMotion>().Configure(narrativeShotStarted,1,Vector2.zero,StoryCameraTravel);}
   if(place=="citadel"||place=="reset"||place=="relay"||place=="disconnect"){
    var relay=Image(stage,"Scene uplink",width*.52f-70,160,140,188,Color.white,itemFrames[3]);relay.preserveAspect=true;relay.gameObject.AddComponent<CutsceneLayerMotion>().Configure(narrativeShotStarted,.8f,Vector2.zero,StoryCameraTravel);
   }
   if(place=="reset"){
    var rupture=Image(stage,"Reality reset",0,0,width,350,Color.clear);rupture.gameObject.AddComponent<RealityRevealMotion>().started=narrativeShotStarted;
   }
  }
 }
 public sealed class RealityRevealMotion:MonoBehaviour {
  public float started;UnityEngine.UI.Image image;
  void Update(){if(!image)image=GetComponent<UnityEngine.UI.Image>();float age=Time.unscaledTime-started;float flash=Mathf.Clamp01(1-Mathf.Abs(age-1.8f)/.3f);float aftermath=Mathf.SmoothStep(0,.3f,Mathf.Clamp01((age-2.2f)/2));image.color=new Color(flash>.01f?1:.25f,flash>.01f?1:.025f,flash>.01f?1:.025f,Mathf.Max(flash,aftermath));}
 }
}
