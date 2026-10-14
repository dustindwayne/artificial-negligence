using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Linq;
namespace ArtificialNegligence {
 public class EnemyAgent:MonoBehaviour {
  public static readonly string[] Names={"SCOUT","GUNNER","CHARGER","AEGIS","SWARM","SNIPER","AUDITOR","HARVESTER","EXECUTOR"};
  public int hp,maxHp,type;public int AttackCount {get{return volley;}}public bool IsBoss {get{return type>=6;}}
  GameController game;SpriteRenderer visual,bar;Light2D warning;
  float left,right,direction=1,cooldown,windup,charge,stun,baseY;int volley;Vector2 aim;
  public void Initialize(GameController controller,int enemyType,float origin){
   game=controller;type=enemyType;maxHp=hp=IsBoss?20+game.level*3:(type==2?6:type==3?5:3)+game.level/2;
   maxHp=hp=Mathf.Max(1,Mathf.RoundToInt(hp*(IsBoss?GameplayContent.Current.bossHealth:GameplayContent.Current.enemyHealth)));
   visual=GetComponent<SpriteRenderer>();baseY=transform.position.y;warning=game.PointLight(transform,IsBoss?GameController.Gold:GameController.Pink,IsBoss?4:2,.1f);
   var floor=game.rows.First(r=>r.kind=="platform"&&r.y==218&&r.x/GameController.PPU<=origin&&(r.x+r.w)/GameController.PPU>=origin);
   float home=transform.position.x;left=Mathf.Max((floor.x+14)/GameController.PPU,home-(IsBoss?10:3));right=Mathf.Min((floor.x+floor.w-14)/GameController.PPU,home+(IsBoss?10:3));
   if(IsBoss)transform.localScale=Vector3.one*1.6f;{bar=game.SpriteObject("Boss health",game.enemyFrames[0],transform.position+Vector3.up*1.9f,20,false,transform);bar.sprite=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),Vector2.one*.5f,1);}
  }
  void Update(){
   if(!game||game.mode!=ScreenMode.Play||hp<=0)return;float dt=Time.deltaTime;cooldown=Mathf.Max(0,cooldown-dt);stun=Mathf.Max(0,stun-dt);warning.enabled=Mathf.Abs(transform.position.x-game.gameCamera.transform.position.x)<17;
   Vector2 delta=game.playerBody.position-(Vector2)transform.position;bool sight=Mathf.Abs(delta.x)<(IsBoss?15:11)&&Mathf.Abs(delta.y)<4&&!Physics2D.Linecast(transform.position,game.playerBody.position,1<<8).collider;
   bool enraged=IsBoss&&hp<=maxHp/2;float movement=0;Color tint=type==3||type==6?GameController.Cyan:type==4?new Color(.6f,1,.3f):type==5?new Color(.8f,.4f,1):IsBoss?GameController.Gold:Color.white;
   warning.color=tint;warning.intensity=windup>0?1:.1f;visual.color=stun>0?GameController.Cream:windup>0?GameController.Pink:Color.white;
   if(bar){bar.color=enraged?GameController.Pink:GameController.Gold;bar.transform.localScale=new Vector3((IsBoss?2.4f:1.1f)*Mathf.Max(0,hp)/maxHp,.08f,1);bar.enabled=warning.enabled;}
   if(stun>0&&!IsBoss)return;
   if(windup>0){windup-=dt;if(windup<=0){
    if(type==2||type==8&&volley%2==0){charge=IsBoss?.65f:.45f;cooldown=1.7f;}
    else{int count=type==7?(enraged?7:5):type==6?3:1;float speed=type==5?14:8+Mathf.Min(4,game.level*.2f);
     for(int i=0;i<count;i++){float angle=(i-(count-1)/2f)*(type==7?12:16)*Mathf.Deg2Rad;var v=new Vector2(aim.x*Mathf.Cos(angle)-aim.y*Mathf.Sin(angle),aim.x*Mathf.Sin(angle)+aim.y*Mathf.Cos(angle));game.SpawnBullet((Vector2)transform.position+v*.7f,v*speed,true,IsBoss?3:2);}
     cooldown=(type==5?2.4f:1.8f)/(enraged?1.35f:1);volley++;
    }
   }}else if(charge>0){charge-=dt;movement=direction*(IsBoss?11:8);if(charge<=0)volley++;}
   else if(sight){direction=delta.x>=0?1:-1;
    if(type==1||type==3||type==5||IsBoss){if(Mathf.Abs(delta.x)>6)movement=direction*1.3f;else if(type==5&&Mathf.Abs(delta.x)<4)movement=-direction*2;if(cooldown<=0){aim=delta.normalized;windup=type==5?1.1f:IsBoss?.85f:.7f;}}
    else if(type==2&&Mathf.Abs(delta.x)<6&&cooldown<=0)windup=.6f;else movement=direction*(type==4?3.6f:2.3f);
   }else{if(transform.position.x<=left)direction=1;if(transform.position.x>=right)direction=-1;movement=direction*1.15f;}
   var pos=transform.position;pos.x=Mathf.Clamp(pos.x+movement*dt*(1+Mathf.Min(.5f,game.level*.025f))*GameplayContent.Current.enemySpeed,left,right);pos.y=baseY+(type==4?Mathf.Sin(Time.time*5)*.25f:0);transform.position=pos;
   visual.flipX=direction>0;visual.sprite=game.enemyFrames[type*4+(windup>0||charge>0?3:Mathf.Abs(movement)>.1f?1+(int)(Time.time*8)%2:0)];transform.rotation=Quaternion.Euler(0,0,windup>0?Mathf.Sin(Time.time*24)*2:movement!=0?Mathf.Sin(Time.time*8)*1.2f:0);
   if(Mathf.Abs(delta.x)<(IsBoss?1.2f:.8f)&&Mathf.Abs(delta.y)<1){if(game.buffs[1]>0||game.buffs[5]>0){if(!IsBoss)Hit(99);}else game.Damage(IsBoss?4:type==2?3:2,Mathf.Sign(delta.x));}
  }
  public void Hit(int damage){if(hp<=0)return;hp-=damage;stun=.15f;if(hp<=0){game.EnemyDown(this);game.Burst(transform.position,GameController.Gold,IsBoss?32:10);visual.enabled=false;warning.enabled=false;if(bar)bar.enabled=false;enabled=false;}else game.Burst(transform.position,GameController.Cyan,4);}
 }
}
