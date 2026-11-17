using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ArtificialNegligence {
 public partial class GameController {
  struct ClimbRoute {public float x,bottom,top;}
  readonly List<ClimbRoute> climbRoutes=new List<ClimbRoute>();
  float? qaClimbAxis;bool qaClimbJump;
  bool isClimbing;ClimbRoute climbRoute;float climbCooldown,climbPhase;
  readonly Dictionary<int,Sprite[]> climbAnimations=new Dictionary<int,Sprite[]>();
  public bool IsClimbing=>isClimbing;
  void EndClimb(){if(playerBody)playerBody.gravityScale=1;isClimbing=false;climbCooldown=.25f;}
  void BuildClimbRoutes(){
   climbRoutes.Clear();isClimbing=false;climbCooldown=climbPhase=0;
   foreach(var row in rows){if(row.kind!="ladder"&&row.kind!="climb")continue;
    var route=new ClimbRoute{x=(row.x+row.w*.5f)/PPU,bottom=-(row.y+row.h)/PPU,top=-row.y/PPU};climbRoutes.Add(route);
    for(int side=0;side<2;side++){var rail=SpriteObject("Climbable rail",white,new Vector2(route.x+(side==0?-.25f:.25f),(route.top+route.bottom)*.5f),3,false);rail.transform.localScale=new Vector3(2f/PPU,route.top-route.bottom,1);rail.color=Cream;}
    for(float y=route.bottom;y<=route.top;y+=6f/PPU){var rung=SpriteObject("Ladder rung",white,new Vector2(route.x,y),3,false);rung.transform.localScale=new Vector3(.6f,2f/PPU,1);rung.color=Cream;}
   }
  }
  float ClimbInput(){if(qaMode&&qaClimbAxis.HasValue)return qaClimbAxis.Value;float value=(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)||jumpHeld?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)||duckHeld?1:0);return value!=0?value:pad.connected?pad.y:0;}
  bool TickClimbing(){
   climbCooldown=Mathf.Max(0,climbCooldown-Time.fixedDeltaTime);float vertical=ClimbInput();
   if(!isClimbing&&climbCooldown<=0&&Mathf.Abs(vertical)>.3f&&knockback<=0){
    foreach(var route in climbRoutes){var pos=playerBody.position;if(Mathf.Abs(pos.x-route.x)>.55f||pos.y<route.bottom-.15f||pos.y>route.top+1)continue;
     if(vertical<0&&pos.y<=route.bottom+.7f||vertical>0&&pos.y>=route.top-.1f)continue;
     climbRoute=route;isClimbing=true;slideTime=dashTime=0;SetLowCollider(false,false);playerBody.gravityScale=0;jumpBuffer=0;break;
    }
   }
   if(!isClimbing)return false;
   if(knockback>0){EndClimb();return false;}
   if(qaMode&&qaClimbJump||Input.GetKey(KeyCode.Space)||PadHeld(PadButton.A)){EndClimb();playerBody.linearVelocity=new Vector2(PadMove*3,9);jumpBuffer=0;return true;}
   var position=playerBody.position;position.x=Mathf.MoveTowards(position.x,climbRoute.x,5*Time.fixedDeltaTime);playerBody.position=position;
   playerBody.linearVelocity=new Vector2(0,vertical*2.4f);jumpBuffer=coyote=0;grounded=false;
   climbPhase+=Mathf.Abs(vertical)*Time.fixedDeltaTime*6;
   if(vertical>0&&position.y>=climbRoute.top+.75f){EndClimb();playerBody.linearVelocity=Vector2.zero;}
   else if(vertical<0&&position.y<=climbRoute.bottom+.65f){EndClimb();}
   return true;
  }
  void ApplyClimbVisual(){
   if(!isClimbing||!playerVisual)return;
   if(!climbAnimations.TryGetValue(stats.character,out var frames)){
    string[] names={"looper","","","","ai-wrath","davey-goats","brad-jammin","mikey-redd","prison-mike"};
    var texture=Resources.Load<Texture2D>("Art/climbing-cast");
    var layout=Resources.Load<TextAsset>("Art/"+names[stats.character]+"-climb-layout");var mask=Resources.Load<TextAsset>("Art/climbing-cast-mask");
    frames=texture&&layout&&mask?SpriteAtlas.Fit(texture,4,1,64,64,32,true,JsonUtility.FromJson<AtlasLayout>(layout.text).frames.Select(f=>f.Rect).ToArray(),mask.bytes,4):null;
    climbAnimations[stats.character]=frames;
   }
   // Retain a safe fallback for incomplete art imports; the shared cast atlas supplies all six characters.
   playerVisual.sprite=frames!=null?frames[(int)climbPhase%frames.Length]:playerFrames[0];playerVisual.flipX=false;
  }
 }
}
