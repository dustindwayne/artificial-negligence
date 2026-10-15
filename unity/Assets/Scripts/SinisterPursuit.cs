using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ArtificialNegligence {
 public partial class EnemyAgent {
  Rigidbody2D humanBody;BoxCollider2D humanBox;float humanAlert,humanNavTimer,humanAttack,humanWindup,humanShield,humanAbility;bool humanGrounded,humanLow;Vector2 humanAim;int nextNode=-1;SpriteRenderer humanLaser,humanOrb;
  readonly List<LevelRow> navigation=new List<LevelRow>();
  public bool IsHuman {get{return type>=9;}}public bool IsPursuing {get{return IsHuman&&humanAlert>0;}}public SpriteRenderer VisualRenderer {get{return visual;}}
  void InitializeHuman(LevelRow floor){
   var original=visual;visual=game.SpriteObject("Sinister human animation",original.sprite,transform.position+Vector3.up*.1875f,8,false,transform);original.enabled=false;
   humanBody=gameObject.AddComponent<Rigidbody2D>();humanBody.constraints=RigidbodyConstraints2D.FreezeRotation;humanBody.interpolation=RigidbodyInterpolation2D.Interpolate;humanBody.collisionDetectionMode=CollisionDetectionMode2D.Continuous;humanBox=gameObject.AddComponent<BoxCollider2D>();humanBox.size=new Vector2(.75f,1.375f);humanBox.sharedMaterial=new PhysicsMaterial2D("Sinister boots"){friction=0};
   gameObject.layer=9;Physics2D.IgnoreLayerCollision(9,0,true);Physics2D.IgnoreLayerCollision(9,9,true);humanBody.position=new Vector2(transform.position.x,-floor.y/GameController.PPU+.6875f);
   navigation.AddRange(game.rows.Where(r=>r.kind=="platform"||r.kind=="crawl"));humanAbility=3;
   humanLaser=game.SpriteObject("AI targeting tell",game.itemFrames[0],transform.position,7,false,transform);humanLaser.sprite=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),Vector2.one*.5f,1);humanLaser.color=new Color(1,.2f,.4f,.55f);humanLaser.enabled=false;
   if(type==10||type==12){humanOrb=game.SpriteObject("Captured AI node",game.itemFrames[7],transform.position+Vector3.up*1.3f,9,false,transform);humanOrb.transform.localPosition=new Vector3(-.7f,1.3f,0);humanOrb.transform.localScale=Vector3.one*.25f;humanOrb.color=GameController.Pink;}
  }
  public void SetPresentationVisible(bool show){foreach(var sr in GetComponentsInChildren<SpriteRenderer>())sr.enabled=show&&hp>0;if(IsHuman)GetComponent<SpriteRenderer>().enabled=false;}
  int NearestNode(Vector2 position){
   float foot=-(position.y-.6875f)*GameController.PPU;float best=float.MaxValue;int at=0;
   for(int i=0;i<navigation.Count;i++){var r=navigation[i];float dx=Mathf.Max(r.x-position.x*GameController.PPU,0,position.x*GameController.PPU-r.x-r.w);float score=dx*3+Mathf.Abs(r.y-foot);if(score<best){best=score;at=i;}}return at;
  }
  void FindPursuitRoute(){
   if(navigation.Count==0)return;int start=NearestNode(humanBody.position),goal=NearestNode(game.playerBody.position);nextNode=-1;if(start==goal)return;
   var parent=new int[navigation.Count];for(int i=0;i<parent.Length;i++)parent[i]=-2;parent[start]=-1;var queue=new Queue<int>();queue.Enqueue(start);
   while(queue.Count>0){int from=queue.Dequeue();if(from==goal)break;var a=navigation[from];for(int to=0;to<navigation.Count;to++){if(parent[to]!=-2)continue;var b=navigation[to];float rise=a.y-b.y,gap=Mathf.Max(0,Mathf.Max(a.x,b.x)-Mathf.Min(a.x+a.w,b.x+b.w));if(rise>46||rise<-120||gap>(rise>28?22:60))continue;parent[to]=from;queue.Enqueue(to);}}
   if(parent[goal]==-2)return;int node=goal;while(parent[node]!=start&&parent[node]>=0)node=parent[node];nextNode=node;
  }
  void TickHuman(float dt){
   stun=Mathf.Max(0,stun-dt);humanAttack=Mathf.Max(0,humanAttack-dt);humanShield=Mathf.Max(0,humanShield-dt);humanAbility-=dt;humanNavTimer-=dt;
   Vector2 delta=game.PlayerHitCenter-humanBody.position;bool close=delta.magnitude<14;bool sight=close&&!Physics2D.Linecast(humanBody.position,game.PlayerHitCenter,1<<8).collider;if(sight||stun>0)humanAlert=9;else humanAlert=Mathf.Max(0,humanAlert-dt);
   if(humanGrounded&&humanNavTimer<=0&&humanAlert>0){humanNavTimer=.3f;FindPursuitRoute();}
   warning.enabled=Mathf.Abs(transform.position.x-game.gameCamera.transform.position.x)<17;warning.color=GameController.Pink;warning.intensity=humanWindup>0?.9f:.12f;
   if(humanAbility<=0&&(type==10||type==12)&&humanAlert>0){humanAbility=8;humanShield=2.5f;game.Burst(transform.position,GameController.Pink,8);}
   if(humanOrb){humanOrb.transform.localPosition=new Vector3(-direction*.65f,1.2f+Mathf.Sin(Time.time*4)*.12f,0);humanOrb.color=humanShield>0?GameController.Gold:GameController.Pink;}
   if(humanWindup>0){humanWindup-=dt;if(humanWindup<=0){
     int shots=type==9?3:type==12?5:1;for(int i=0;i<shots;i++){float angle=(i-(shots-1)*.5f)*(type==12?10:6);Vector2 shot=Quaternion.Euler(0,0,angle)*humanAim;game.SpawnBullet(humanBody.position+shot*.65f,shot*(type==11?12:10),true,type==12?3:2);}volley++;humanAttack=type==11?1.1f:type==12?1.8f:1.5f;
    }}else if(sight&&humanAttack<=0&&stun<=0){humanAim=(game.PlayerHitCenter+game.playerBody.linearVelocity*.15f-humanBody.position).normalized;humanWindup=type==12?1:type==11?.48f:.7f;}
   humanLaser.enabled=humanWindup>0;if(humanLaser.enabled){humanLaser.transform.localPosition=new Vector3(humanAim.x*2,humanAim.y*2,0);humanLaser.transform.localScale=new Vector3(4,.035f,1);humanLaser.transform.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(humanAim.y,humanAim.x)*Mathf.Rad2Deg);}
   visual.sprite=game.enemyFrames[type*4+(humanWindup>0?3:Mathf.Abs(humanBody.linearVelocity.x)>.2f?1+(int)(Time.time*8)%2:0)];visual.flipX=direction<0;visual.color=stun>0?GameController.Cream:humanShield>0?new Color(1,.6f,.8f):Color.white;
   float scale=humanLow?.34f:1;visual.transform.localScale=new Vector3(type==12?1.12f:1,scale,1);visual.transform.localPosition=Vector3.up*(-.6875f+.875f*scale);
   if(bar){bar.enabled=warning.enabled;bar.transform.localScale=new Vector3(1.2f*Mathf.Max(0,hp)/maxHp,.07f,1);bar.color=GameController.Pink;}
   var hit=game.PlayerHitCenter-(Vector2)humanBox.bounds.center;if(Mathf.Abs(hit.x)<.8f&&Mathf.Abs(hit.y)<.6f)game.Damage(type==12?3:2,Mathf.Sign(hit.x));
  }
  void FixedUpdate(){
   if(!IsHuman||!game||game.mode!=ScreenMode.Play||hp<=0||!humanBody)return;
   humanGrounded=Physics2D.BoxCast(humanBody.position+Vector2.down*.59f,new Vector2(.62f,.12f),0,Vector2.down,.16f,1<<8).collider;
   float target=game.playerBody.position.x;var velocity=humanBody.linearVelocity;
   if(humanAlert<=0){velocity.x=0;humanBody.linearVelocity=velocity;return;}
   if(nextNode>=0){var next=navigation[nextNode];target=(next.x+next.w*.5f)/GameController.PPU;}
   float move=Mathf.Abs(target-humanBody.position.x)>.15f?Mathf.Sign(target-humanBody.position.x):0;if(nextNode<0&&Mathf.Abs(game.playerBody.position.x-humanBody.position.x)<(type==11?1.1f:2.2f))move=0;
   if(move!=0)direction=move;
   humanLow=false;foreach(var hit in Physics2D.OverlapBoxAll(humanBody.position+new Vector2(direction*.65f,.34f),new Vector2(.7f,.68f),0,1<<8))if(!hit.usedByEffector)humanLow=true;
   humanBox.size=new Vector2(.75f,humanLow?.625f:1.375f);humanBox.offset=humanLow?Vector2.down*.375f:Vector2.zero;
   float speed=(type==11?5.7f:type==12?4.7f:5.1f)*Mathf.Lerp(1,game.EncounterDifficulty,.4f);if(humanLow)speed=2.4f;
   if(stun>0||humanWindup>0&&humanGrounded)move=0;
   velocity.x=Mathf.MoveTowards(velocity.x,move*speed,50*Time.fixedDeltaTime);
   bool climb=nextNode>=0&&Mathf.Abs(target-humanBody.position.x)<2.1f&&navigation[nextNode].y<-((humanBody.position.y-.6875f)*GameController.PPU)-8;
   if(humanGrounded&&(move!=0||climb)&&!humanLow){bool edge=!Physics2D.Raycast(humanBody.position+new Vector2(move*.8f,-.6f),Vector2.down,.8f,1<<8).collider;bool wall=Physics2D.Raycast(humanBody.position,Vector2.right*move,.65f,1<<8).collider;if(edge||climb||wall)velocity.y=235f/GameController.PPU;}
   velocity.y=Mathf.Max(-28,velocity.y);humanBody.linearVelocity=velocity;
   if(humanBody.position.y<-20)Hit(maxHp+1);
  }
 }
}
