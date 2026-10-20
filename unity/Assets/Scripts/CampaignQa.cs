using System;
using System.Collections;
using System.Linq;
using System.IO;
using UnityEngine;

namespace ArtificialNegligence {
 public partial class GameController {
  // Scripted progression regression, separate from the physics route validator.
  // Positioning at pickups is intentional: this checks collection/gates, not human playability.
  IEnumerator CampaignQa(){
   yield return null;
   var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-qaOutput");string path=at>=0&&at+1<args.Length?args[at+1]:Application.persistentDataPath;Directory.CreateDirectory(path);
   stats=new RunStats();stats.EnsureCollection();buffs=new float[7];stats.storySeen=true;
   int bosses=0,wins=0;
   for(int stage=0;stage<90;stage++){
    if(stats.cityUnlocked!=stage)throw new Exception("Campaign unlock sequence / "+stage);
    StartStage(stage);yield return null;
    if(mode==ScreenMode.Narrative)FinishNarrative();
    playerBody.position=PixelCenter(worldLength-60,207);playerBody.linearVelocity=Vector2.zero;
    TryCompleteStage();if(mode!=ScreenMode.Play||stats.cityUnlocked!=stage)throw new Exception("Quota gate bypass / "+stage);
    // Find loot rather than grant starting equipment. First box must supply a weapon.
    var box=pickups.FirstOrDefault(p=>!p.gpu&&!p.taken);
    if(box==null)throw new Exception("No accessible equipment box / "+stage);
    playerBody.position=box.position;TickPickups(0);
    if(stats.WeaponCount==0)throw new Exception("First box did not supply a weapon");
    stats.EquipItem(stats.inventory.First(i=>i.slot==1).id);
    foreach(var pickup in pickups.Where(p=>p.gpu&&!p.taken).ToArray()){playerBody.position=pickup.position;TickPickups(0);}
    if(collected!=totalGpus||collected<RequiredGpus)throw new Exception("Pickup quota cannot be satisfied / "+stage);
    playerBody.position=PixelCenter(worldLength-60,207);
    var boss=enemies.FirstOrDefault(e=>e.IsBoss&&e.hp>0);
    if(boss){
     TryCompleteStage();if(mode!=ScreenMode.Play||stats.cityUnlocked!=stage)throw new Exception("Live boss gate bypass / "+stage);
     for(int hit=0;boss.hp>0&&hit<20000;hit++){boss.QaStepCombat(.25f);boss.Hit(Math.Max(1,stats.HitDamage(boss.type)),stats.weapon);}
     if(boss.hp>0)throw new Exception("Found weapon cannot defeat boss / "+stage);
     bosses++;if(mode==ScreenMode.Narrative)FinishNarrative();
    }
    // Walk the final stretch with the real movement controller, not a forced clear.
    var floor=rows.Last(r=>r.kind=="platform"&&r.y==218);
    playerBody.position=PixelCenter(floor.x+floor.w-140,207);playerBody.linearVelocity=Vector2.zero;
    rightHeld=true;duckHeld=true;float deadline=Time.realtimeSinceStartup+5;
    while(mode==ScreenMode.Play&&Time.realtimeSinceStartup<deadline)yield return null;
    ClearInput();
    var expected=(stage+1)%CityRoute.Stages==0?ScreenMode.Win:ScreenMode.Clear;
    if(mode!=expected||stats.cityUnlocked!=stage+1)throw new Exception("Exit/ending blocked / "+stage+" / "+mode);
    int points=stats.points;TryCompleteStage();if(stats.points!=points)throw new Exception("Repeated completion rewards");
    if(stage==12&&(!stats.wrathFreed||stats.wrathEncounters!=3))throw new Exception("Wrath unlock blocks route");
    if(mode==ScreenMode.Win){wins++;CaptureQa(Path.Combine(path,"campaign-victory-cycle-"+wins+".png"));}
    stats=JsonUtility.FromJson<RunStats>(JsonUtility.ToJson(stats));stats.EnsureCollection();
    if(stats.cityUnlocked!=stage+1)throw new Exception("Completion not persisted");
    NextLevel();if(mode!=ScreenMode.CityMap||level!=stage+1)throw new Exception("Next city cycle inaccessible");
    Debug.Log("AN_CAMPAIGN_STAGE_PASS / "+(stage+1));
   }
   if(wins!=3||bosses!=17||!stats.CharacterUnlocked(4))throw new Exception("Campaign totals incorrect / bosses "+bosses);
   foreach(int legacyStage in new[]{12,18,35}){
    stats=new RunStats{cityUnlocked=legacyStage,district=legacyStage};stats.EnsureCollection();
    if(stats.wrathEncounters!=2||stats.CharacterUnlocked(4))throw new Exception("Legacy save auto-unlocked Wrath");
    LoadLevel(legacyStage);SetMode(ScreenMode.Play);var final=enemies.SingleOrDefault(e=>e.type==12);
    if(!final)throw new Exception("Pending Wrath never appeared / "+legacyStage);
    for(int hit=0;final.hp>0&&hit<2000;hit++){final.QaStepCombat(.25f);final.Hit(2,3);}
    if(final.hp>0||!stats.CharacterUnlocked(4)||stats.wrathEncounters!=3)throw new Exception("Legacy final fight did not unlock Wrath");
    if(mode==ScreenMode.Narrative)FinishNarrative();
    stats=JsonUtility.FromJson<RunStats>(JsonUtility.ToJson(stats));stats.EnsureCollection();stats.character=4;ApplyCharacter();
    if(!stats.wrathFreed||stats.character!=4)throw new Exception("Recovered Wrath unlock lost after reload");
    LoadLevel(legacyStage);if(enemies.Any(e=>e.type==12))throw new Exception("Recovered final fight repeated after unlock");
   }
   Debug.Log("AN_WRATH_SAVE_RECOVERY_PASS / level 13, later stage, next city cycle, real final defeat required, persisted playable unlock");
   File.WriteAllText(Path.Combine(path,"campaign-result.json"),"{\"stages\":90,\"bosses\":17,\"victories\":3,\"wrathUnlocked\":true,\"scriptedProgression\":true,\"humanPlaythrough\":false,\"legacyWrathRecoveryCases\":3}");
   Debug.Log("AN_CAMPAIGN_QA_PASS / 90 stages, all quota/boss/exit gates, found weapon, three Wrath encounters, three act endings, saved progression, next cycles");Application.Quit(0);
  }
 }
}
