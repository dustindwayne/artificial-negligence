using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace ArtificialNegligence {
 public partial class GameController {
  IEnumerator ThreeActQa(){
   yield return null;var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-qaOutput");string path=at>=0?args[at+1]:Application.persistentDataPath;Directory.CreateDirectory(path);
   stats=new RunStats();stats.EnsureCollection();
   int[] counts=new int[6];for(int roll=0;roll<100;roll++)counts[RunStats.ExcellentClearTier(roll)]++;
   if(counts[5]!=5||counts[4]!=10||counts[2]!=85||!RunStats.ExcellentClear(19,20,0)||RunStats.ExcellentClear(18,20,0)||RunStats.ExcellentClear(20,20,1))throw new Exception("Excellent-clear distribution/qualification");
   level=0;collected=totalGpus=20;stageHits=0;AwardExcellentClear();int items=stats.inventory.Count;AwardExcellentClear();if(items!=1||stats.inventory.Count!=items||stats.HasWeapon||stats.HasArmor)throw new Exception("Reward repeats or auto equips");
   stats.Discover(1,0,2);var weapon=stats.inventory.Last();stats.EquipItem(weapon.id);stats.points=10000;stats.salvage=100;int rarity=weapon.tier;stats.Upgrade(1);
   if(weapon.level!=2||weapon.tier!=rarity||stats.SalvageItem(weapon.id))throw new Exception("Item level/rarity/equipped salvage");
   stats.Discover(1,0,2);var spare=stats.inventory.Last();if(spare.level!=1||!stats.SalvageItem(spare.id)||weapon.level!=2)throw new Exception("Independent item/salvage");
   stats=JsonUtility.FromJson<RunStats>(JsonUtility.ToJson(stats));stats.EnsureCollection();if(stats.EquipmentLevel(1)!=2||!stats.clearRewardStages.Contains(0))throw new Exception("Equipment persistence");
   Debug.Log("AN_EQUIPMENT_QA_PASS / 5 Ultra, 10 Legend, 85 Rare; 95 percent/no-hit gate; one reward per stage; manual equip; independent levels; fixed rarity; salvage and save");
   foreach(int stage in new[]{8,17}){
    stats=new RunStats();stats.EnsureCollection();stats.cityUnlocked=stage;StartStage(stage);yield return null;
    int id=stage==8?7:5;if(stats.CharacterUnlocked(id))throw new Exception("Virtual recruit unlocked before fight");
    var boss=enemies.FirstOrDefault(e=>e.type==(stage==8?13:14));if(!boss)throw new Exception("Virtual host missing");
    int hp=boss.hp;boss.Hit(99999,0);if(boss.hp<=0||boss.hp==hp)throw new Exception("Virtual host single-hit gate");
    CaptureQa(Path.Combine(path,stage==8?"mikey-virtual-world.png":"davey-virtual-world.png"));
    for(int hit=0;boss.hp>0&&hit<2000;hit++){boss.QaStepCombat(.25f);boss.Hit(9999,1);}
    if(boss.hp>0||!stats.CharacterUnlocked(id))throw new Exception("Virtual boss defeat does not unlock recruit");
    if(mode==ScreenMode.Narrative)FinishNarrative();
    stats=JsonUtility.FromJson<RunStats>(JsonUtility.ToJson(stats));stats.EnsureCollection();if(!stats.CharacterUnlocked(id))throw new Exception("Virtual unlock not persisted");
   }
   stats=new RunStats();stats.EnsureCollection();LoadLevel(0);BeginStory(ScreenMode.Title);yield return null;
   if(activeGraphics!=4||stats.GraphicsGpus!=0)throw new Exception("Prologue clear world");CaptureQa(Path.Combine(path,"reality-before-break.png"));AdvanceStory();yield return new WaitForSecondsRealtime(1.3f);
   if(activeGraphics!=0||stats.GraphicsGpus!=0)throw new Exception("Prologue fracture changed permanent progress");CaptureQa(Path.Combine(path,"reality-fracture.png"));FinishStory();
   stats.cityUnlocked=6;SetMode(ScreenMode.Title);yield return null;if(!titleRoster.Contains(6)||titleRoster.Contains(4)||titleRoster.Contains(5)||titleRoster.Contains(7)||titleRoster.Contains(8))throw new Exception("Title shows locked characters");titleStarted-=7;yield return null;CaptureQa(Path.Combine(path,"living-title.png"));
   Debug.Log("AN_PROLOGUE_TITLE_QA_PASS / clear world fractures without GPU changes; title roster excludes locked recruits");
   foreach(int scene in new[]{11,12,13}){BeginNarrative(scene,ScreenMode.CityMap);yield return null;AdvanceNarrative();yield return null;CaptureQa(Path.Combine(path,"story-act-"+(scene-10)+".png"));FinishNarrative();}
   stats.points=1200;stats.salvage=20;SetMode(ScreenMode.Shop);yield return null;CaptureQa(Path.Combine(path,"equipment-upgrades.png"));
   Debug.Log("AN_STORY_QA_PASS / virtual hosts resist single hits; defeat unlocks saved recruits; three act scenes render");Application.Quit(0);
  }
 }
}
