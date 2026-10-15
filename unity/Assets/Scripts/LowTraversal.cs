using System;
using System.Collections.Generic;
using UnityEngine;
namespace ArtificialNegligence {
 public partial class GameController {
  bool duckHeld,lowStance;float slideTime,slideCooldown,slideTrail;int slideFacing;
  BoxCollider2D playerCollider;readonly Dictionary<Sprite,Vector2> lowPoseBounds=new Dictionary<Sprite,Vector2>();
  public bool IsDucking {get{return lowStance;}}public bool IsSliding {get{return slideTime>0;}}
  public Vector2 PlayerHitCenter {get{return playerCollider?(Vector2)playerCollider.bounds.center:playerBody.position;}}
  public Vector2 PlayerHitSize {get{return playerCollider?(Vector2)playerCollider.bounds.size:new Vector2(.75f,1.375f);}}
  void ResetTraversal(){playerCollider=playerBody.GetComponent<BoxCollider2D>();lowStance=false;slideTime=slideCooldown=0;duckHeld=false;SetLowCollider(false,false);}
  bool CanStand(){
   foreach(var hit in Physics2D.OverlapBoxAll(playerBody.position+Vector2.up*.34f,new Vector2(.70f,.68f),0,1<<8))if(!hit.usedByEffector)return false;
   return true;
  }
  void SetLowCollider(bool low,bool sliding){
   if(!playerCollider)return;float height=low?(sliding?8f:10f):22f;playerCollider.size=new Vector2(12f/PPU,height/PPU);playerCollider.offset=new Vector2(0,-(22-height)/(PPU*2));lowStance=low;
  }
  public void Slide(){
   if(mode!=ScreenMode.Play||!grounded||slideCooldown>0)return;
   if(Mathf.Abs(playerBody.linearVelocity.x)<.8f){Toast("GET MOVING, THEN SLIDE");return;}
   slideFacing=playerBody.linearVelocity.x>0?1:-1;slideTime=.6f;slideCooldown=1;dashTime=0;SetLowCollider(true,true);
  }
  float LowMovement(float input,float speed,ref Vector2 velocity){
   float dt=Time.fixedDeltaTime;slideTime=Mathf.Max(0,slideTime-dt);slideCooldown=Mathf.Max(0,slideCooldown-dt);
   bool down=PadDuck||duckHeld||Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow);bool room=CanStand();
   if(jumpBuffer>0&&room){slideTime=0;down=false;}
   SetLowCollider(slideTime>0||grounded&&down||!room,slideTime>0);
   if(slideTime>0){velocity.x=slideFacing*Mathf.Lerp(6.5f,10,slideTime/.6f);slideTrail-=dt;if(slideTrail<=0){slideTrail=.08f;Burst(playerBody.position+Vector2.down*.6f,Cyan,2);}return 0;}
   return lowStance?speed*.4f:speed;
  }
  void ApplyStanceVisual(){
   if(!playerVisual)return;
   if(!lowStance){playerVisual.transform.localScale=Vector3.one;playerVisual.transform.localPosition=Vector3.up*(3f/PPU);return;}
   var frame=playerVisual.sprite;
   if(!lowPoseBounds.TryGetValue(frame,out var metric)){
    var texture=frame.texture;var pixels=texture.GetPixels32();int min=texture.height,max=0;for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)if(pixels[y*texture.width+x].a>24){min=Math.Min(min,y);max=Math.Max(max,y);}
    metric=new Vector2((min-texture.height*.5f)/frame.pixelsPerUnit,(max-min+1)/frame.pixelsPerUnit);lowPoseBounds[frame]=metric;
   }
   float scale=Mathf.Min(1,(IsSliding?.46f:.58f)/Mathf.Max(.01f,metric.y));playerVisual.transform.localScale=new Vector3(1,scale,1);playerVisual.transform.localPosition=Vector3.up*(-11f/PPU-metric.x*scale);
  }
  void BuildLowPassages(){
   foreach(var row in rows){if(row.kind!="crawl")continue;
    var roof=SpriteObject("Low maintenance duct",PlatformStyle((row.x+row.w*.5f)/PPU),PixelCenter(row.x+row.w*.5f,row.y+row.h*.5f),1,false);roof.drawMode=SpriteDrawMode.Tiled;roof.size=new Vector2(row.w/PPU,row.h/PPU);roof.gameObject.layer=8;roof.gameObject.AddComponent<BoxCollider2D>().size=roof.size;
    var lip=SpriteObject("Low clearance warning",white,PixelCenter(row.x+row.w*.5f,row.y+row.h-1),3,false);lip.transform.localScale=new Vector3(row.w/PPU,2f/PPU,1);lip.color=Gold;
    for(int side=0;side<2;side++){var stripe=SpriteObject("Duct safety stripe",white,PixelCenter(row.x+5+side*(row.w-10),row.y+6),4,false);stripe.transform.localScale=new Vector3(5f/PPU,4f/PPU,1);stripe.color=Gold;}
   }
  }
 }
}
