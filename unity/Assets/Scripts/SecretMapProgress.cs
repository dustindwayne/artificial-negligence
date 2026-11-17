using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ArtificialNegligence {
 public partial class RunStats {
  public List<string> discoveredSecretRoutes=new List<string>();
  public bool HasSecretRoute(string id){return !string.IsNullOrEmpty(id)&&discoveredSecretRoutes!=null&&discoveredSecretRoutes.Contains(id);}
 }
 public partial class GameController {
  [System.Serializable] class SecretMapBranch {public string id,label;public int stage;public Vector2 node;public Vector2[] points;}
  // Call only when an authored secret exit is actually reached, never on normal clear.
  void DiscoverSecretRoute(string id){if(string.IsNullOrEmpty(id)||stats.HasSecretRoute(id))return;if(stats.discoveredSecretRoutes==null)stats.discoveredSecretRoutes=new List<string>();stats.discoveredSecretRoutes.Add(id);SaveProgress();}
  void BuildDiscoveredMapBranches(RectTransform parent){
   if(districtLayout.secrets==null)return;
   foreach(var branch in districtLayout.secrets){
    // Gate the whole branch before creating any path, label or clickable object.
    if(!stats.HasSecretRoute(branch.id))continue;
    if(branch.points!=null)for(int i=1;i<branch.points.Length;i++){var a=ExpandMapPoint(branch.points[i-1])+Vector2.one*15;var b=ExpandMapPoint(branch.points[i])+Vector2.one*15;int steps=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(a,b)/10));for(int j=0;j<=steps;j++){var p=Vector2.Lerp(a,b,j/(float)steps);Image(parent,"Discovered secret trail",p.x,p.y,3,3,Gold);}}
    var node=ExpandMapPoint(branch.node);var hit=Image(parent,"Discovered secret / "+branch.id,node.x,node.y,40,40,new Color(Gold.r,Gold.g,Gold.b,.6f),frameSprite);hit.raycastTarget=true;var button=hit.gameObject.AddComponent<Button>();button.targetGraphic=hit;button.interactable=branch.stage>=0&&branch.stage<=stats.cityUnlocked;button.onClick.AddListener(()=>StartStage(branch.stage));Label(parent,branch.label,node.x-40,node.y+42,120,24,14,Cream,TextAnchor.MiddleCenter);
   }
  }
 }
}
