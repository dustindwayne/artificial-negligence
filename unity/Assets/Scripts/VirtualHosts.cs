using UnityEngine;
namespace ArtificialNegligence {
 public partial class EnemyAgent {
  bool virtualSpecial;
  void TickVirtualHost(float dt,bool sight){
   phaseLock=Mathf.Max(0,phaseLock-dt);if(phaseLock>0){humanWindup=0;return;}
   if(humanAbility<=0&&humanAlert>0&&humanWindup<=0){humanAbility=hp<=maxHp/2?5:7;virtualSpecial=true;humanAim=(game.PlayerHitCenter-humanBody.position).normalized;humanWindup=1.1f;game.Burst(transform.position,type==13?GameController.Gold:GameController.Cyan,8);}
   if(humanWindup>0){humanWindup-=dt;if(humanWindup<=0){
    int count=virtualSpecial?(type==13?7:5):(type==13?3:2);float spacing=virtualSpecial?12:7;
    for(int i=0;i<count;i++){Vector2 shot=Quaternion.Euler(0,0,(i-(count-1)*.5f)*spacing)*humanAim;game.SpawnBullet(humanBody.position+shot*.7f,shot*(type==13?9:11),true,2);}
    if(virtualSpecial&&type==14){humanShield=1.5f;var pos=humanBody.position;pos.x=Mathf.Clamp(pos.x-direction*2,arenaLeft,arenaRight);game.Burst(humanBody.position,GameController.Cyan,12);humanBody.position=pos;}
    virtualSpecial=false;volley++;humanAttack=type==13?1.5f:.95f;
   }}else if(sight&&humanAttack<=0&&stun<=0){humanAim=(game.PlayerHitCenter+game.playerBody.linearVelocity*.12f-humanBody.position).normalized;humanWindup=.65f;}
  }
 }
}
