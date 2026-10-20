using System;
using System.Linq;
namespace ArtificialNegligence {
 public partial class RunStats {
  public int salvage;
  public System.Collections.Generic.List<int> clearRewardStages=new System.Collections.Generic.List<int>(),restoredOverlords=new System.Collections.Generic.List<int>();
  public EquipmentItem EquippedItem(int slot){return inventory?.FirstOrDefault(i=>i.id==(slot==1?weaponItemId:armorItemId)&&i.slot==slot);}
  public int EquipmentLevel(int slot){return EquippedItem(slot)?.level??0;}
  public int EquipmentCap(int slot){return EquippedItem(slot)?.MaxLevel??0;}
  public int UpgradeSalvageCost(int slot){var item=EquippedItem(slot);return item==null?0:2+item.tier+item.level/2;}
  public bool CanUpgradeItem(int slot){return EquippedItem(slot)!=null&&EquipmentLevel(slot)<EquipmentCap(slot)&&points>=Cost(slot)&&salvage>=UpgradeSalvageCost(slot);}
  public int SalvageValue(EquipmentItem item){return item.tier*3+item.level;}
  public bool SalvageItem(int id){EnsureCollection();var item=inventory.FirstOrDefault(i=>i.id==id);if(item==null||id==weaponItemId||id==armorItemId)return false;salvage+=SalvageValue(item);inventory.Remove(item);EnsureCollection();return true;}
  public static int ExcellentClearTier(int roll){if(roll<0||roll>=100)throw new ArgumentOutOfRangeException();return roll<5?5:roll<15?4:2;}
  public static bool ExcellentClear(int collected,int total,int hits){return total>0&&collected>=Math.Ceiling(total*.95)&&hits==0;}
 }
 public partial class GameController {
  string clearBonus="";
  void AwardExcellentClear(){
   clearBonus="";if(!RunStats.ExcellentClear(collected,totalGpus,stageHits)||stats.clearRewardStages.Contains(level))return;
   stats.clearRewardStages.Add(level);int tier=RunStats.ExcellentClearTier(rng.Next(100));int slot=rng.Next(1,3);stats.Discover(slot,rng.Next(4),tier);
   clearBonus="EXCELLENT CLEAR / "+RunStats.Tiers[tier]+" EQUIPMENT FOUND";
  }
  void RecordHumanRestoration(EnemyAgent enemy){if(level<60||level>=90||!enemy.IsBoss)return;int id=CityRoute.Area(level);if(!stats.restoredOverlords.Contains(id))stats.restoredOverlords.Add(id);}
 }
}
