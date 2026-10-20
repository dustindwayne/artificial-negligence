using UnityEngine;
namespace ArtificialNegligence {
 public static class CombatBalance {
  public static int Health(int type,int level,int mastery,int tier,int encounter){
   if(type==13||type==14)return 150+level*7+Mathf.Clamp(mastery,0,20)*4;
   if(type==12)return Mathf.RoundToInt((160+level*9+Mathf.Clamp(mastery,0,20)*7+tier*8)*(1+Mathf.Clamp(encounter,0,2)*.2f));
   if(type>=6&&type<=8)return 80+level*7+Mathf.Clamp(mastery,0,20)*4;
   if(type>=9)return new[]{24,28,22}[type-9]+level*2+Mathf.Clamp(mastery,0,20);
   return new[]{8,10,16,14,7,11}[type]+level;
  }
  public static int WrathSlot(int stage){return stage==5?0:stage==11?1:stage==12?2:-1;}
 }
 public partial class EnemyAgent {
  int wrathPhase=1,wrathPattern,wrathBurst;float damageGate,staggerGate,phaseLock,wrathDash,wrathRecovery,wrathBurstClock;Vector2 wrathHome;float arenaLeft,arenaRight;
  public int CombatPhase=>type==12?wrathPhase:hp<=maxHp/2?2:1;
  public string CombatTell=>type!=12?"":phaseLock>0?"REBOOT":humanShield>0?"GUARD / ARC":humanWindup>0?(wrathPattern%3==2?"RUSH / JUMP":"TARGET LOCK"):wrathDash>0?"RUSH":wrathRecovery>0?"EXPOSED":"PHASE "+wrathPhase;
  void InitializeCombat(LevelRow floor){if(type<12)return;wrathHome=humanBody.position;arenaLeft=(floor.x+12)/GameController.PPU;arenaRight=(floor.x+floor.w-12)/GameController.PPU;humanAttack=1;humanAbility=4;}
  void TickCombatTimers(float dt){damageGate=Mathf.Max(0,damageGate-dt);staggerGate=Mathf.Max(0,staggerGate-dt);}
  int MitigateHit(int damage,int kind){
   if(damage<=0||damageGate>0||type==12&&phaseLock>0)return 0;
   if(type==12){
    if(humanShield>0){if(kind==1){humanShield=0;wrathRecovery=.65f;game.Toast("ARC DISRUPTION / WRATH'S GUARD BROKEN");}else damage=Mathf.Max(1,Mathf.RoundToInt(damage*.45f));}
    else if(kind!=3&&wrathRecovery<=0)damage=Mathf.Max(1,Mathf.RoundToInt(damage*.8f));
    damage=Mathf.Min(damage,Mathf.Max(1,Mathf.CeilToInt(maxHp*.065f)));
    if(wrathPhase<3){int floor=Mathf.CeilToInt(maxHp*(wrathPhase==1?.66f:.33f));damage=Mathf.Min(damage,Mathf.Max(0,hp-floor));}
    damageGate=.14f;
   }else if(type>=13){if(type==14&&humanShield>0&&kind!=1)damage=Mathf.Max(1,damage/2);damage=Mathf.Min(damage,Mathf.Max(1,Mathf.CeilToInt(maxHp*.08f)));damageGate=.13f;}else if(IsBoss){damage=Mathf.Min(damage,Mathf.Max(1,Mathf.CeilToInt(maxHp*.12f)));damageGate=.1f;}
   else if(IsHuman){if(humanShield>0&&kind!=1)damage=Mathf.Max(1,damage-3);damage=Mathf.Min(damage,Mathf.Max(1,Mathf.CeilToInt(maxHp*.4f)));damageGate=.08f;}
   return damage;
  }
  void CheckWrathPhase(){
   if(type!=12||wrathPhase>=3)return;int floor=Mathf.CeilToInt(maxHp*(wrathPhase==1?.66f:.33f));if(hp>floor)return;
   wrathPhase++;phaseLock=.85f;humanWindup=wrathDash=0;wrathBurst=0;humanShield=0;humanAttack=.35f;humanAbility=.8f;wrathPattern=wrathPhase==2?2:0;
   game.Burst(transform.position,GameController.Gold,18);game.Toast("AI WRATH / PHASE "+wrathPhase+" / PROGRAM ESCALATING");
  }
  void TickWrath(float dt,bool sight){
   phaseLock=Mathf.Max(0,phaseLock-dt);wrathRecovery=Mathf.Max(0,wrathRecovery-dt);wrathDash=Mathf.Max(0,wrathDash-dt);
   if(phaseLock>0){humanWindup=0;return;}
   if(humanAbility<=0&&humanAlert>0){humanAbility=wrathPhase==3?6:8;humanShield=wrathPhase==1?1.8f:2.4f;game.Burst(transform.position,GameController.Pink,10);}
   if(wrathBurst>0){wrathBurstClock-=dt;if(wrathBurstClock<=0){wrathBurstClock=.19f;wrathBurst--;FireWrathShot(humanAim,12,2);}}
   if(wrathRecovery>0||wrathDash>0)return;
   if(humanWindup>0){humanWindup-=dt;if(humanWindup<=0){
    int pattern=wrathPattern%3;
    if(pattern==0){int count=3+wrathPhase*2;for(int i=0;i<count;i++)FireWrathShot(Quaternion.Euler(0,0,(i-(count-1)*.5f)*10)*humanAim,10.5f,2);wrathRecovery=.32f;}
    else if(pattern==1){FireWrathShot(humanAim,12,2);wrathBurst=wrathPhase+1;wrathBurstClock=.19f;wrathRecovery=.6f;}
    else{direction=humanAim.x>=0?1:-1;wrathDash=.42f;wrathRecovery=1.05f;if(wrathPhase>=2){var origin=humanBody.position+Vector2.down*.5f;game.SpawnBullet(origin+Vector2.left*.6f,Vector2.left*8.5f,true,2);game.SpawnBullet(origin+Vector2.right*.6f,Vector2.right*8.5f,true,2);}}
    volley++;wrathPattern++;humanAttack=wrathPhase==3?1.05f:1.5f;
   }}else if(sight&&humanAttack<=0&&wrathBurst==0){humanAim=(game.PlayerHitCenter+game.playerBody.linearVelocity*.18f-humanBody.position).normalized;if(wrathPattern%3==2)humanAim=new Vector2(humanAim.x>=0?1:-1,0);humanWindup=wrathPattern%3==2?1.05f:.85f;}
  }
  void FireWrathShot(Vector2 aim,float speed,int damage){game.SpawnBullet(humanBody.position+aim*.65f,aim*speed,true,damage);}
  bool RecoverWrath(){if(type<12||humanBody.position.y>=-18)return false;humanBody.position=wrathHome;humanBody.linearVelocity=Vector2.zero;phaseLock=.9f;humanWindup=wrathDash=0;wrathBurst=0;game.Burst(wrathHome,GameController.Gold,16);return true;}
  public void QaVerifyFallRecovery(){int before=hp;humanBody.position=new Vector2(wrathHome.x,-25);if(!RecoverWrath()||hp!=before||humanBody.position.y<-18)throw new System.Exception("Wrath pit recovery failed");}
  // QA uses normal damage/phase gates, not an instant-kill route.
  public void QaStepCombat(float dt){TickCombatTimers(dt);if(type==12)TickWrath(dt,false);}
 }
}
