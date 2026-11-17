using System;
using System.Collections.Generic;
using UnityEngine;
namespace ArtificialNegligence {
 public partial class GameController {
  bool duckHeld,lowStance;float slideTime,slideCooldown,slideTrail;int slideFacing;
  public const float StandingHeight=22f,CrouchHeight=19f;
  const float SlideHeight=16f, PlayerWidth=12f;
  BoxCollider2D playerCollider;
  readonly Collider2D[] standingContacts=new Collider2D[16];
  readonly Dictionary<Sprite,Vector2> lowPoseBounds=new Dictionary<Sprite,Vector2>();
  float crouchStride;
  public bool IsDucking {get{return lowStance;}}public bool IsSliding {get{return slideTime>0;}}
  public Vector2 PlayerHitCenter {get{return playerCollider?(Vector2)playerCollider.bounds.center:playerBody.position;}}
  public Vector2 PlayerHitSize {get{return playerCollider?(Vector2)playerCollider.bounds.size:new Vector2(.75f,1.375f);}}
  void ResetTraversal(){EndClimb();playerCollider=playerBody.GetComponent<BoxCollider2D>();lowStance=false;slideTime=slideCooldown=crouchStride=0;duckHeld=false;SetLowCollider(false,false);}
  bool CanStand(){
   // Check the swept headroom above the current body, across its full width.
   float lowTop=(SlideHeight-StandingHeight*.5f)/PPU;
   float standingTop=StandingHeight*.5f/PPU;
   var center=playerBody.position+Vector2.up*((lowTop+standingTop)*.5f);
   var size=new Vector2(PlayerWidth/PPU,standingTop-lowTop);
   var filter=new ContactFilter2D();
   filter.SetLayerMask(1<<8);
   filter.useTriggers=false;
   int count=Physics2D.OverlapBox(center,size,0,filter,standingContacts);
   for(int i=0;i<count;i++)if(!standingContacts[i].usedByEffector)return false;
   return true;
  }
  void SetLowCollider(bool low,bool sliding){
   if(!playerCollider)return;
   float height=low?(sliding?SlideHeight:CrouchHeight):StandingHeight;
   playerCollider.size=new Vector2(PlayerWidth/PPU,height/PPU);
   playerCollider.offset=Vector2.down*((StandingHeight-height)/(2*PPU));
   lowStance=low;
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
   if(slideTime>0){velocity.x=slideFacing*Mathf.Lerp(6.5f,10,slideTime/.6f);slideTrail-=dt;if(slideTrail<=0){slideTrail=.08f;Burst(playerBody.position+Vector2.down*.6f,Accent,2);}return 0;}
   return lowStance?speed*.52f:speed;
  }
  void ApplyStanceVisual(){
   if(!playerVisual)return;
   if(!lowStance){crouchStride=0;playerVisual.transform.localScale=Vector3.one;playerVisual.transform.localPosition=Vector3.up*(3f/PPU);return;}
   var frame=playerVisual.sprite;
   if(!lowPoseBounds.TryGetValue(frame,out var metric)){
    var texture=frame.texture;var pixels=texture.GetPixels32();int min=texture.height,max=0;for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)if(pixels[y*texture.width+x].a>24){min=Math.Min(min,y);max=Math.Max(max,y);}
    metric=new Vector2((min-texture.height*.5f)/frame.pixelsPerUnit,(max-min+1)/frame.pixelsPerUnit);lowPoseBounds[frame]=metric;
   }
   // Authored crouch and slide poses keep their proportions. Align their feet to the floor.
   bool walking=!IsSliding&&grounded&&Mathf.Abs(playerBody.linearVelocity.x)>.15f;
   crouchStride=walking?crouchStride+Mathf.Abs(playerBody.linearVelocity.x)*Time.deltaTime:0;
   float step=walking&&(Mathf.FloorToInt(crouchStride*PPU/3)%2==1)?1f/(PPU*2):0;
   float available=(CityRoute.DuctClearance-.25f)/PPU;
   float scale=Mathf.Min(1,available/Mathf.Max(.01f,metric.y+step));
   playerVisual.transform.localScale=Vector3.one*scale;
   playerVisual.transform.localPosition=new Vector3(step*(playerVisual.flipX?-1:1),
    -StandingHeight/(PPU*2)+(step-metric.x)*scale,0);
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
