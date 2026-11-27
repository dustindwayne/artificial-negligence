using UnityEngine;
using UnityEngine.UI;
namespace ArtificialNegligence {
 public partial class RunStats {public int mapEntrance=-1;}
 public partial class GameController {
  Vector2[] mapSegment;int mapWaypoint,mapSegmentTarget=-1;
  bool advanceMapOnOpen,mapArriving;int mapAutoTarget=-1;Image mapTraveler;Text mapTravelLabel;Button mapEnter;Vector2[] mapNodes;Vector2 mapPosition;int mapStage,mapDestination,mapOffset;float mapInputRepeat;
  void BeginMapTravel(UnityEngine.RectTransform parent,Vector2[] nodes,int offset){
   enterOnArrival=false;mapSegmentTarget=-1;mapNodes=nodes;mapOffset=offset;mapStage=Mathf.Clamp(stats.mapEntrance<0?stats.cityUnlocked:stats.mapEntrance,offset,Mathf.Min(offset+nodes.Length-1,stats.cityUnlocked));mapDestination=advanceMapOnOpen?Mathf.Clamp(mapAutoTarget<0?stats.cityUnlocked:mapAutoTarget,offset,offset+nodes.Length-1):mapStage;mapArriving=advanceMapOnOpen&&stats.mapEntrance<offset;advanceMapOnOpen=false;mapAutoTarget=-1;mapPosition=mapArriving?new Vector2(30,nodes[0].y):nodes[mapStage-offset];
   mapTraveler=Image(parent,"Walking map character",mapPosition.x-3,mapPosition.y-24,38,42,Color.white,CharacterFrame(stats.character,0));mapTraveler.preserveAspect=true;
  }
  bool enterOnArrival;
  void SelectMapDestination(int index,bool enter=false){if(index<mapOffset||index>Mathf.Min(mapOffset+mapNodes.Length-1,stats.cityUnlocked))return;mapDestination=index;enterOnArrival=enter;}
  void EnterMapStage(){if(!mapArriving&&mapStage==mapDestination)StartStage(mapStage);}
  void TickMapTravel(){
   if(mode!=ScreenMode.CityMap||!mapTraveler||mapNodes==null)return;TickMapAtmosphere();if(Input.GetKeyDown(KeyCode.C)){characterReturn=ScreenMode.CityMap;SetMode(ScreenMode.Characters);return;}
   bool moving=mapArriving||mapStage!=mapDestination;
   if(!moving&&enterOnArrival){enterOnArrival=false;EnterMapStage();return;}
   if(!moving&&Time.unscaledTime>=mapInputRepeat){
    Vector2 direction=new Vector2((Input.GetKey(KeyCode.RightArrow)||Input.GetKey(KeyCode.D)?1:0)-(Input.GetKey(KeyCode.LeftArrow)||Input.GetKey(KeyCode.A)?1:0),(Input.GetKey(KeyCode.UpArrow)||Input.GetKey(KeyCode.W)?1:0)-(Input.GetKey(KeyCode.DownArrow)||Input.GetKey(KeyCode.S)?1:0));
    if(direction==Vector2.zero&&controllerInUse)direction=new Vector2(!padGate?pad.x:0,!padGate?pad.y:0);
    if(direction.magnitude>.5f){if(direction.x>.5f&&mapStage==mapOffset+mapNodes.Length-1&&SwitchMapDistrict(1))return;if(direction.x<-.5f&&mapStage==mapOffset&&SwitchMapDistrict(-1))return;int best=mapStage;float score=0;foreach(int next in new[]{mapStage-1,mapStage+1}){if(next<mapOffset||next>Mathf.Min(mapOffset+mapNodes.Length-1,stats.cityUnlocked))continue;var delta=mapNodes[next-mapOffset]-mapNodes[mapStage-mapOffset];delta.y=-delta.y;float dot=Vector2.Dot(delta.normalized,direction.normalized);if(dot>score){score=dot;best=next;}}SelectMapDestination(best);mapInputRepeat=Time.unscaledTime+.25f;}
   }
   moving=mapArriving||mapStage!=mapDestination;
   if(mapArriving){mapPosition=Vector2.MoveTowards(mapPosition,mapNodes[0],170*Time.unscaledDeltaTime);if(Vector2.Distance(mapPosition,mapNodes[0])<.1f){mapArriving=false;stats.mapEntrance=mapStage;SaveProgress();}}
   else if(moving){int next=mapStage+(mapDestination>mapStage?1:-1);if(mapSegmentTarget!=next){mapSegmentTarget=next;mapSegment=MapWalkPath(mapStage-mapOffset,next-mapOffset);mapWaypoint=0;}var target=mapSegment[mapWaypoint];mapPosition=Vector2.MoveTowards(mapPosition,target,170*Time.unscaledDeltaTime);if(Vector2.Distance(mapPosition,target)<.1f){mapWaypoint++;if(mapWaypoint>=mapSegment.Length){mapStage=next;mapSegmentTarget=-1;if(mapStage==mapDestination){stats.mapEntrance=mapStage;SaveProgress();}}}}

   mapTraveler.rectTransform.anchoredPosition=new Vector2(mapPosition.x-3,-mapPosition.y+24);mapTraveler.sprite=CharacterFrame(stats.character,moving?1+(int)(Time.unscaledTime*7)%2:0);
   mapEnter.interactable=!moving;mapTravelLabel.text=moving?"WALKING TO "+MapPlaces[mapDistrict][mapDestination%6]:MapPlaces[mapDistrict][mapStage%6]+(UiWidth>=1120?"":" / ENTER OR A");
   if(!moving&&Input.GetKeyDown(KeyCode.Return))EnterMapStage();
  }
 }
}
