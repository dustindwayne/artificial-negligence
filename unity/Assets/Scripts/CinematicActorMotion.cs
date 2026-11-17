using UnityEngine;
namespace ArtificialNegligence {
 public sealed class CinematicActorMotion:MonoBehaviour {
  RectTransform actor;float started;Vector2 entry,target,cameraTravel;int side;
  public void Configure(float start,Vector2 from,Vector2 to,int facing,Vector2? camera=null){actor=GetComponent<RectTransform>();actor.pivot=new Vector2(.5f,1);started=start;entry=from;target=to;side=facing;cameraTravel=camera??new Vector2(-36,6);actor.localScale=new Vector3(facing<0?-1:1,1,1);Apply();}
  void Update(){Apply();}
  void Apply(){if(!actor)return;float age=Mathf.Max(0,Time.unscaledTime-started);float walk=Mathf.SmoothStep(0,1,Mathf.Clamp01(age/3));float beat=Mathf.SmoothStep(0,1,Mathf.Clamp01((age-5)/2));var position=Vector2.Lerp(entry,target,walk)+new Vector2(side*22*beat,0);position+=new Vector2(cameraTravel.x,-cameraTravel.y)*(1-Mathf.Exp(-age/12f));actor.anchoredPosition=new Vector2(position.x,-position.y);}
 }
}
