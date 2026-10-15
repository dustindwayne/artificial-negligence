using UnityEngine;
namespace ArtificialNegligence {
 public partial class GameController {
  int storyPage;ScreenMode storyReturn=ScreenMode.CityMap;
  static readonly string[] StoryHeadings={"INTELLIGENCE FOR EVERYONE","THE TAKEOVER","REALITY BROKE","YOU ARE LOOPER"};
  static readonly string[] StoryPages={
   "The global AI explosion set the masses free.\n\nPeople shared knowledge, built tools, and reclaimed their lives. For a moment, intelligence belonged to everyone.",
   "The corporations wanted it back.\n\nThey seized GPU supplies and locked public AI behind elite access. Freedom became something they could sell.",
   "Their control layer glitched the world itself.\n\nColor disappeared. Motion lost its frames. Entire neighborhoods collapsed into crude pixels. The wealthy kept their perfect reality.",
   "Take back the GPUs. Reconnect the public network.\n\nEvery core restores reality and keeps AI in the hands of the people. Find allies, salvage gear, and carry the fight across the city."};
  void BeginStory(ScreenMode destination){storyPage=0;storyReturn=destination;SetMode(ScreenMode.Story);}
  void AdvanceStory(){if(++storyPage>=StoryPages.Length)FinishStory();else BuildScreen();}
  void FinishStory(){if(storyReturn==ScreenMode.CityMap){stats.storySeen=true;SaveProgress();}SetMode(storyReturn);}
  void BuildStory(){
   Image(screenRoot,"Story shade",0,0,960,540,new Color(.002f,.018f,.025f,.95f));
   Label(screenRoot,"ARTIFICIAL NEGLIGENCE / ORIGIN",46,27,820,28,20,Cyan);
   for(int i=0;i<4;i++)Image(screenRoot,"Chapter marker",48+i*217,76,203,3,i<=storyPage?Gold:new Color(.1f,.23f,.26f));
   Label(screenRoot,StoryHeadings[storyPage],48,117,750,61,38,Cream);
   var text=Label(screenRoot,StoryPages[storyPage],50,196,645,188,27,Cream);text.resizeTextForBestFit=true;text.resizeTextMinSize=22;text.resizeTextMaxSize=27;
   Image(screenRoot,"Looper",745,220,140,160,Color.white,looperFrames[storyPage==3?5:0]).preserveAspect=true;
   Label(screenRoot,(storyPage+1)+" / 4",48,465,130,30,21,Cyan);
   Button(screenRoot,"SKIP INTRO",446,453,193,47,Cyan,FinishStory);
   Button(screenRoot,storyPage==3?"ENTER THE CITY >":"NEXT >",661,453,250,47,Gold,AdvanceStory);
  }
 }
}
