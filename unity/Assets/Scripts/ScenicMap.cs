using UnityEngine;
namespace ArtificialNegligence {
 public partial class GameController {
  [System.Serializable] class MapEdge {public Vector2[] points;}
  [System.Serializable] class DistrictLayout {public Vector2[] nodes;public MapEdge[] edges;public SecretMapBranch[] secrets;}
  DistrictLayout districtLayout;
  int mapDistrict=-1;
  static readonly string[][] MapPlaces={
   new[]{"SURVIVOR CAMP","SHUTTERED SHOPS","DRAINAGE CROSSING","SERVER YARD","OFFICE APPROACH","CONTROL GATE"},
   new[]{"ROOFLINE START","MARKET AWNINGS","SERVICE CATWALK","SKYBRIDGE","LIFT SHAFT","CHECKPOINT ROOF"},
   new[]{"MAINTENANCE DEPOT","TRAIN PLATFORMS","POWER SWITCHYARD","FLOODED UNDERPASS","CARGO LIFT","TRANSIT CONTROL"},
   new[]{"ARCHIVE WALK","GREENHOUSE DECK","COOLING CANAL","MODEL VAULT","SEED LIBRARY","ARCHIVE GATE"},
   new[]{"ENTRY PLAZA","LICENSE OFFICES","SERVER TERRACES","SURVEILLANCE DECK","EXECUTIVE LIFT","CONTROL SPIRE"}};
  string MapStageName(int stage){return CityRoute.Areas[CityRoute.Area(stage)]+" / "+MapPlaces[CityRoute.Area(stage)][stage%6];}
  Vector2[] MapWalkPath(int from,int to){var points=districtLayout.edges[Mathf.Min(from,to)].points;return to>from?points:System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Reverse(points));}
  bool SwitchMapDistrict(int direction){int next=mapDistrict+direction;int cycle=stats.cityUnlocked/30;if(next<0||next>4||cycle*30+next*6>stats.cityUnlocked)return false;mapDistrict=next;stats.mapEntrance=cycle*30+next*6+(direction<0?5:0);SetMode(ScreenMode.CityMap);return true;}
  float MapArtWidth=>Mathf.Max(1080,ScreenUiWidth);float MapArtHeight=>MapArtWidth*.5f;float MapArtY=>(540-MapArtHeight)*.5f;
  Vector2 ExpandMapPoint(Vector2 point){return new Vector2((point.x+16-116)*MapArtWidth/728-16,(point.y+15-74)*MapArtHeight/364-15);}

  void BuildScenicMap(){
   var window=Rect(screenRoot,"Fullscreen district overworld",(UiWidth-MapArtWidth)*.5f-uiRoot.anchoredPosition.x,MapArtY,MapArtWidth,MapArtHeight);
   int cycle=stats.cityUnlocked/30;if(mapDistrict<0)mapDistrict=CityRoute.Area(stats.cityUnlocked);int offset=cycle*30+mapDistrict*6;
   string mapAsset=new[]{"fractured-city-overworld","market-overworld","transit-overworld","gardens-overworld","spire-overworld"}[mapDistrict];var art=Resources.Load<Texture2D>("Art/"+mapAsset);
   if(art){var sprite=Sprite.Create(art,new Rect(0,0,art.width,art.height),Vector2.one*.5f,100);Image(window,"Illustrated district",0,0,MapArtWidth,MapArtHeight,Color.white,sprite);}
   else Image(window,"District terrain",0,0,MapArtWidth,MapArtHeight,new Color(.13f,.12f,.09f));
   districtLayout=JsonUtility.FromJson<DistrictLayout>(Resources.Load<TextAsset>("Art/"+mapAsset+"-layout").text);
   for(int i=0;i<districtLayout.nodes.Length;i++)districtLayout.nodes[i]=ExpandMapPoint(districtLayout.nodes[i]);
   foreach(var edge in districtLayout.edges)for(int i=0;i<edge.points.Length;i++)edge.points[i]=ExpandMapPoint(edge.points[i]);
   var nodes=districtLayout.nodes;BuildMapAtmosphere(window);
   for(int i=0;i<districtLayout.edges.Length;i++){var points=districtLayout.edges[i].points;for(int j=0;j<points.Length-1;j++){var from=points[j]+Vector2.one*15;var to=points[j+1]+Vector2.one*15;int steps=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(from,to)/12));for(int step=0;step<=steps;step++){var spot=Vector2.Lerp(from,to,step/(float)steps);Image(window,"Walking trail",spot.x,spot.y,2,2,offset+i<stats.cityUnlocked?new Color(.9f,.8f,.57f,.65f):Color.clear);}}}
   for(int i=0;i<6;i++){int stage=offset+i;bool open=stage<=stats.cityUnlocked;var node=nodes[i];
    var hit=Image(window,"Level entrance",node.x-8,node.y-8,48,48,Color.clear);hit.raycastTarget=true;var entry=hit.gameObject.AddComponent<UnityEngine.UI.Button>();entry.targetGraphic=hit;entry.transition=UnityEngine.UI.Selectable.Transition.None;entry.interactable=open;entry.onClick.AddListener(()=>SelectMapDestination(stage,true));
    Image(window,"Entrance signal glow",node.x+2,node.y+9,30,30,new Color(Gold.r,Gold.g,Gold.b,open?.5f:.08f),glowSprite);
    Image(window,"Entrance marker rim",node.x+9,node.y+17,14,12,open?Cream:new Color(.3f,.28f,.23f));Image(window,"Entrance marker core",node.x+11,node.y+19,10,8,open?Gold:Ink);
    var number=Label(window,(stage+1).ToString("00"),node.x-36,Mathf.Clamp(node.y+14,MapHudBottom-MapArtY,510-MapArtY),28,20,14,open?Cream:Accent,TextAnchor.MiddleCenter);Image(window,"Marker caption backing",node.x-35,Mathf.Clamp(node.y+15,MapHudBottom-MapArtY,511-MapArtY),26,18,new Color(.025f,.025f,.02f,.85f)).transform.SetSiblingIndex(number.transform.GetSiblingIndex());
   }
   BeginMapTravel(window,nodes,offset);
   BuildDiscoveredMapBranches(window);
   BuildOverworldHud();
  }
 }
}
