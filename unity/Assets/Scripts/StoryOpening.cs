using UnityEngine;
namespace ArtificialNegligence {
 public partial class GameController {
  float storyFractureStarted;
  int StoryRenderTier(){return storyPage==0?Mathf.Max(4,stats.GraphicsTier):storyPage==1?Mathf.Max(stats.GraphicsTier,4-Mathf.FloorToInt((Time.unscaledTime-storyFractureStarted)/.24f)):stats.GraphicsTier;}
  int storyPage;ScreenMode storyReturn=ScreenMode.CityMap;
  static readonly string[] StoryHeadings={"THE MOMENT BEFORE","REALITY FRACTURES","FOLLOW THE SIGNAL","YOU ARE LOOPER"};
  static readonly string[] StoryPages={
   "Yesterday, anyone could use AI. We finally had the tools to build something of our own. Wait... what is happening to the street?",
   "The buildings. My hands. Everything is losing detail. Who did this?",
   "Masked saboteurs have fractured your city. Recover the GPUs. Bring them to our uplinks. Together we can restore reality.",
   "They call the masked ones Sinister. All I know is people need help. I will find those GPUs... and some answers."};
  static readonly string[] StorySpeakers={"LOOPER","LOOPER","PUBLIC SIGNAL","LOOPER"};
  void TickOpeningCamera(){float age=Time.unscaledTime-storyFractureStarted;float progress=1-Mathf.Exp(-age/5);float pan=storyPage==0?1.8f:storyPage==1?-.8f:1.1f;gameCamera.transform.position=new Vector3(15+pan*progress,-8.4375f+.18f*progress,-20);}
  void BeginStory(ScreenMode destination){storyPage=0;storyReturn=destination;storyFractureStarted=Time.unscaledTime;SetMode(ScreenMode.Story);RefreshGraphics(true);}
  void AdvanceStory(){if(++storyPage>=StoryPages.Length)FinishStory();else {storyFractureStarted=Time.unscaledTime;BuildScreen();RefreshGraphics(true);}}
  void FinishStory(){if(storyReturn==ScreenMode.CityMap){stats.storySeen=true;SaveProgress();}SetMode(storyReturn);}
  void BuildStory(){
   var heading=Panel(screenRoot,"Opening heading",28,20,650,48,Accent);Label(heading,StoryHeadings[storyPage],22,9,606,30,24,Cream);
   var panel=Panel(screenRoot,"Opening dialogue",28,352,904,180,Accent);
   Label(panel,StorySpeakers[storyPage],32,18,840,22,18,Gold);
   Label(panel,StoryPages[storyPage],32,48,840,70,20,Cream);
   Label(panel,(storyPage+1)+" / 4",32,132,110,28,16,Accent);
   Button(panel,"SKIP INTRO",480,132,180,34,Accent,FinishStory).GetComponentInChildren<UnityEngine.UI.Text>().fontSize=16;
   Button(panel,storyPage==3?"ENTER THE CITY":"NEXT",678,132,202,34,Gold,AdvanceStory).GetComponentInChildren<UnityEngine.UI.Text>().fontSize=16;
  }
 }
}
