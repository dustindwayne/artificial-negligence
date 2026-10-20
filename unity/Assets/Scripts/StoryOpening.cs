using UnityEngine;
namespace ArtificialNegligence {
 public partial class GameController {
  float storyFractureStarted;
  int StoryRenderTier(){return storyPage==0?Mathf.Max(4,stats.GraphicsTier):storyPage==1?Mathf.Max(stats.GraphicsTier,4-Mathf.FloorToInt((Time.unscaledTime-storyFractureStarted)/.24f)):stats.GraphicsTier;}
  int storyPage;ScreenMode storyReturn=ScreenMode.CityMap;
  static readonly string[] StoryHeadings={"THE MOMENT BEFORE","REALITY FRACTURES","FOLLOW THE SIGNAL","YOU ARE LOOPER"};
  static readonly string[] StoryPages={
   "The AI boom opened the world. Knowledge, tools, and possibility reached ordinary people.\n\nThen, in the middle of an ordinary day, the picture began to tear.",
   "Buildings shuddered. Faces lost detail. The city collapsed into crude pixels.\n\nNobody knew who had broken reality, or what was hiding behind it.",
   "The Public Signal blamed masked enemies. Outsiders called them Sinister.\n\nIts broadcasts promised that recovering the GPUs would restore the world and return AI to everyone.",
   "You are Looper. Follow the signal across the city.\n\nFind allies and equipment. Every recovered GPU sharpens the world.\n\nA clearer picture is not always the truth."};
  void BeginStory(ScreenMode destination){storyPage=0;storyReturn=destination;storyFractureStarted=Time.unscaledTime;SetMode(ScreenMode.Story);RefreshGraphics(true);}
  void AdvanceStory(){if(++storyPage>=StoryPages.Length)FinishStory();else {storyFractureStarted=Time.unscaledTime;BuildScreen();RefreshGraphics(true);}}
  void FinishStory(){if(storyReturn==ScreenMode.CityMap){stats.storySeen=true;SaveProgress();}SetMode(storyReturn);}
  void BuildStory(){
   Image(screenRoot,"Story shade",0,0,960,540,new Color(.002f,.018f,.025f,.38f));
   Label(screenRoot,"ARTIFICIAL NEGLIGENCE / ORIGIN",46,27,820,28,20,Cyan);
   for(int i=0;i<4;i++)Image(screenRoot,"Chapter marker",48+i*217,76,203,3,i<=storyPage?Gold:new Color(.1f,.23f,.26f));
   Label(screenRoot,StoryHeadings[storyPage],48,117,750,61,38,Cream);
   Image(screenRoot,"Dialogue backing",40,184,665,212,new Color(.002f,.018f,.025f,.87f));
   var text=Label(screenRoot,StoryPages[storyPage],50,196,645,188,27,Cream);text.resizeTextForBestFit=true;text.resizeTextMinSize=22;text.resizeTextMaxSize=27;
   Image(screenRoot,"Looper",745,220,140,160,Color.white,looperFrames[storyPage==3?5:0]).preserveAspect=true;
   Label(screenRoot,(storyPage+1)+" / 4",48,465,130,30,21,Cyan);
   Button(screenRoot,"SKIP INTRO",446,453,193,47,Cyan,FinishStory);
   Button(screenRoot,storyPage==3?"ENTER THE CITY >":"NEXT >",661,453,250,47,Gold,AdvanceStory);
  }
 }
}
