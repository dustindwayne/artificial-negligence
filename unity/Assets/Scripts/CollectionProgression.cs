using System;
using System.Linq;
using UnityEngine;
namespace ArtificialNegligence {
    public partial class RunStats {
        public int collectionVersion=2,character,outfit;
        public int[] weaponRarities,armorRarities;
        public static readonly string[] Characters={"LOOPER","VALE","MENDER","ROOK"};
        public static readonly string[] CharacterDescriptions={"A core runner with a blue pulse pack","A rooftop mechanic","A neighborhood medic","The original street survivor"};
        public static readonly string[] Outfits={"ORIGINAL","CYAN THREAD","GOLD THREAD","VIOLET THREAD"};
        public bool CharacterUnlocked(int id){return id==0||id==1&&GraphicsGpus>=24||id==2&&cityUnlocked>=6||id==3&&cityUnlocked>=12;}
        public bool OutfitUnlocked(int id){return id==0||id==1&&rank>=3||id==2&&GraphicsGpus>=75||id==3&&cityUnlocked>=12;}
        [Serializable] public class EquipmentItem {public int id,slot,family,tier;public string Name {get{return (slot==1?Weapons[family]:Suits[family]+" ARMOR")+" #"+id.ToString("000");}}}
        public System.Collections.Generic.List<EquipmentItem> inventory=new System.Collections.Generic.List<EquipmentItem>();
        public int weaponItemId,armorItemId,nextEquipmentId=1;
        public bool HasWeapon {get{return weaponItemId>0&&gear[1]>0;}}
        public bool HasArmor {get{return armorItemId>0&&gear[2]>0;}}
        public void EnsureCollection(){
            if(gear==null||gear.Length!=3)gear=new[]{0,0,0};
            if(inventory==null)inventory=new System.Collections.Generic.List<EquipmentItem>();
            if(collectionVersion<2){inventory.Clear();weaponItemId=armorItemId=0;collectionVersion=2;}
            inventory.RemoveAll(i=>i==null||(i.slot!=1&&i.slot!=2)||i.family<0||i.family>3||i.tier<1||i.tier>5);
            nextEquipmentId=Math.Max(nextEquipmentId,inventory.Count==0?1:inventory.Max(i=>i.id)+1);
            weaponRarities=new int[4];armorRarities=new int[4];foreach(var i in inventory){var rarities=i.slot==1?weaponRarities:armorRarities;rarities[i.family]=Math.Max(rarities[i.family],i.tier);}
            var w=inventory.FirstOrDefault(i=>i.id==weaponItemId&&i.slot==1);gear[1]=w==null?0:w.tier;weapon=w==null?0:w.family;if(w==null)weaponItemId=0;
            var a=inventory.FirstOrDefault(i=>i.id==armorItemId&&i.slot==2);int armorTier=a==null?0:a.tier;maxHealth=Math.Max(1,maxHealth+armorTier-gear[2]);health=Math.Min(health,maxHealth);gear[2]=armorTier;suit=a==null?0:a.family;if(a==null)armorItemId=0;
            character=Mathf.Clamp(character,0,3);outfit=Mathf.Clamp(outfit,0,3);if(!CharacterUnlocked(character))character=0;if(!OutfitUnlocked(outfit))outfit=0;
        }
        public bool EquipItem(int id){EnsureCollection();var item=inventory.FirstOrDefault(i=>i.id==id);if(item==null)return false;if(item.slot==1)weaponItemId=id;else armorItemId=id;EnsureCollection();return true;}
        public bool SelectEquipment(int slot,int family){EnsureCollection();var item=inventory.Where(i=>i.slot==slot&&i.family==family).OrderByDescending(i=>i.tier).FirstOrDefault();return item!=null&&EquipItem(item.id);}
        public void UpgradeEquippedItem(int slot){var item=inventory.FirstOrDefault(i=>i.id==(slot==1?weaponItemId:armorItemId));if(item!=null)item.tier++;EnsureCollection();}
        public string Discover(int slot,int family,int tier){
            EnsureCollection();if(slot!=1&&slot!=2||family<0||family>3||tier<1||tier>5)throw new ArgumentOutOfRangeException();
            var item=new EquipmentItem{id=nextEquipmentId++,slot=slot,family=family,tier=tier};inventory.Add(item);EnsureCollection();
            return Tiers[tier]+" "+item.Name+" FOUND / OPEN INVENTORY TO EQUIP";
        }
    }
}
