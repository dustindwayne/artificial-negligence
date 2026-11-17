using UnityEngine;
namespace ArtificialNegligence {
 // A shared virtual camera moves each still layer by its depth; UI stays fixed.
 public sealed class CutsceneLayerMotion:MonoBehaviour {
  RectTransform rect;Vector2 home;float started,depth;Vector2 actorTravel,cameraTravel;
  public void Configure(float shotStart,float parallax,Vector2 travel,Vector2? camera=null){rect=GetComponent<RectTransform>();home=rect.anchoredPosition;started=shotStart;depth=parallax;actorTravel=travel;cameraTravel=camera??new Vector2(-36,6);Apply();}
  void Update(){Apply();}
  void Apply(){if(!rect)return;float age=Mathf.Max(0,Time.unscaledTime-started);float camera=1-Mathf.Exp(-age/12f);float entrance=Mathf.SmoothStep(0,1,Mathf.Clamp01(age/2.5f));rect.anchoredPosition=home+cameraTravel*camera*depth+actorTravel*entrance;}
 }
}
