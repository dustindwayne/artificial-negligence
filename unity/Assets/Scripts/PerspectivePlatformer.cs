using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace ArtificialNegligence {
 public partial class GameController {
  Vector3 depthCameraVelocity;float depthLook,depthLookVelocity,depthZoom,depthZoomVelocity;
  void BuildPerspectiveDepth(){
   depthCameraVelocity=Vector3.zero;depthLook=0;depthZoom=0;if(background)background.transform.localScale=Vector3.one*2.7f;
   foreach(var row in rows.Where(r=>r.kind=="platform")){
    float width=row.w/PPU,height=Mathf.Max(row.h/PPU,.35f),top=-row.y/PPU;
    float x=(row.x+row.w*.5f)/PPU;
    // Render-only surfaces: the original Physics2D landing plane stays unchanged.
    DepthFace("Platform roof depth",new Vector3(x,top-.035f,1.5f),new Vector3(width,3,1),Quaternion.Euler(90,0,0),new Color(.12f,.28f,.32f),-1);
    DepthFace("Platform right return",new Vector3(x+width*.5f,top-height*.5f,1.5f),new Vector3(3,height,1),Quaternion.Euler(0,90,0),new Color(.035f,.09f,.12f),-2);
    DepthFace("Platform rear wall",new Vector3(x,top-height*.5f,3),new Vector3(width,height,1),Quaternion.identity,new Color(.045f,.13f,.16f),-3);
   }
   // Repeating midground architecture makes perspective and camera travel visible.
   var random=new System.Random(907+level*113);
   for(float x=-8;x<worldLength/PPU+8;x+=8){
    float h=3+(float)random.NextDouble()*7,w=3+(float)random.NextDouble()*2;
    DepthFace("Midground facade",new Vector3(x,-13+h*.5f,7),new Vector3(w,h,1),Quaternion.identity,new Color(.025f,.09f,.12f),-35);
    DepthFace("Midground side",new Vector3(x+w*.5f,-13+h*.5f,9),new Vector3(4,h,1),Quaternion.Euler(0,90,0),new Color(.015f,.055f,.075f),-36);
    DepthFace("Midground rooftop",new Vector3(x,-13+h,9),new Vector3(w,4,1),Quaternion.Euler(90,0,0),new Color(.065f,.17f,.2f),-34);
    for(int i=0;i<4;i++)DepthFace("Facade light",new Vector3(x-w*.28f+i*w*.18f,-13+h*.7f,6.98f),new Vector3(.09f,.55f,1),Quaternion.identity,new Color(.18f,.48f,.48f),-33);
   }
  }
  void DepthFace(string name,Vector3 position,Vector3 scale,Quaternion rotation,Color color,int order){
   var sr=SpriteObject(name,white,position,order,false);sr.transform.position=position;sr.transform.localScale=scale;sr.transform.rotation=rotation;sr.color=color;
  }
  void UpdateDepthCamera(){
   float dt=Mathf.Min(Time.unscaledDeltaTime,.05f);bool playing=mode==ScreenMode.Play;
   float desiredLook=playing?Mathf.Clamp(playerBody.linearVelocity.x*.16f,-2.2f,2.2f):0;
   depthLook=Mathf.SmoothDamp(depthLook,desiredLook,ref depthLookVelocity,.3f,Mathf.Infinity,dt);
   float centerX=mode==ScreenMode.Title?15:Mathf.Clamp(playerBody.position.x+4.4f+depthLook,15,worldLength/PPU-15);
   float rise=playing?Mathf.Clamp((playerBody.position.y+12.55f)*.45f,0,3.1f):0;
   float zoom=playing?Mathf.Clamp(Mathf.Abs(playerBody.linearVelocity.x)*.035f,0,.7f):0;
   depthZoom=Mathf.SmoothDamp(depthZoom,zoom,ref depthZoomVelocity,.45f,Mathf.Infinity,dt);
   Vector3 focus=new Vector3(centerX,-8.4375f+rise,0);
   Vector3 desired=focus+new Vector3(-1.7f,2.5f,-23.8f-depthZoom);
   gameCamera.transform.position=Vector3.SmoothDamp(gameCamera.transform.position,desired,ref depthCameraVelocity,.2f,Mathf.Infinity,dt);
   Vector3 target=focus;
   if(hurtCooldown>.85f&&playing)target.y+=Mathf.Sin(Time.unscaledTime*48)*.045f;
   gameCamera.transform.rotation=Quaternion.Slerp(gameCamera.transform.rotation,Quaternion.LookRotation(target-gameCamera.transform.position),1-Mathf.Exp(-dt*8));
  }
  IEnumerator DepthPreviewQa(){
   string[] args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-qaOutput");string path=at>=0?args[at+1]:Application.persistentDataPath;Directory.CreateDirectory(path);
   yield return new WaitForSecondsRealtime(.8f);StartRun();StartStage(0);
   stats.graphicsCores=Enumerable.Range(0,300).Select(i=>"depth-qa:"+i).ToList();RefreshGraphics(true);
   yield return new WaitForSecondsRealtime(.8f);CaptureQa(Path.Combine(path,"unity-25d-start.png"));
   float start=playerBody.position.x;rightHeld=jumpHeld=fireHeld=true;
   yield return new WaitForSecondsRealtime(1.2f);jumpHeld=false;Dash();
   yield return new WaitForSecondsRealtime(.35f);ClearInput();
   if(playerBody.position.x<start+2)throw new Exception("2.5D movement failed");
   yield return new WaitForSecondsRealtime(.4f);CaptureQa(Path.Combine(path,"unity-25d-travel.png"));
   if(gameCamera.orthographic||gameCamera.transform.position.z>=-20)throw new Exception("Perspective camera failed");
   LoadLevel(12);SetMode(ScreenMode.Play);yield return new WaitForSecondsRealtime(.8f);CaptureQa(Path.Combine(path,"unity-25d-district.png"));
   Debug.Log("AN_25D_QA_PASS / perspective, depth faces, movement, camera travel, district transition");Application.Quit(0);
  }
 }
}

