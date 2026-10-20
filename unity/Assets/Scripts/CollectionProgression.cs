using System;
using System.Linq;
using UnityEngine;
namespace ArtificialNegligence {
    public partial class RunStats {
        public int collectionVersion=2,character,outfit,recruitStoryVersion;public bool mikeyVirtualFreed,daveyVirtualFreed;public bool wrathFreed,daveyGoatsUnlocked,bradJamminUnlocked,mikeyReddUnlocked,prisonMikeUnlocked;public int wrathEncounters,wrathProgressVersion;public System.Collections.Generic.List<int> seenStoryScenes=new System.Collections.Generic.List<int>();
        public int[] weaponRarities,armorRarities;
        public static readonly string[] Characters={"LOOPER","","","","AI WRATH","DAVEY GOATS","BRAD JAMMIN'","MIKEY O'REDD","PRISON MIKE"};
        public static readonly string[] CharacterDescriptions={"A core runner with a blue pulse pack","","","","A freed enforcer of the true overlords","A goat outside the program","A boombox-carrying resistance ally","A red-bearded rebel with a bucket of gold","A bandana-wearing island-shirt rebel"};
        // Retain retired IDs 1-3 so old saves cannot select a different character by accident.
        public static readonly int[] ActiveCharacters={0,4,5,6,7,8};
        public static readonly string[] Outfits={"ORIGINAL","CYAN THREAD","GOLD THREAD","VIOLET THREAD"};
        public bool CharacterUnlocked(int id){return id==0||id==4&&wrathFreed||id==5&&(daveyGoatsUnlocked||daveyVirtualFreed)||id==6&&(bradJamminUnlocked||cityUnlocked>=6)||id==7&&(mikeyReddUnlocked||mikeyVirtualFreed)||id==8&&(prisonMikeUnlocked||cityUnlocked>=12);}
        public bool OutfitUnlocked(int id){return id==0||id==1&&rank>=3||id==2&&GraphicsGpus>=75||id==3&&cityUnlocked>=12||id==4&&wrathFreed;}
        [Serializable] public class EquipmentItem {public int id,slot,family,tier,level=1;public int MaxLevel=>tier==5?10:4+tier;public string Name {get{return (slot==1?Weapons[family]:Suits[family]+" ARMOR")+" #"+id.ToString("000");}}}
        public System.Collections.Generic.List<EquipmentItem> inventory=new System.Collections.Generic.List<EquipmentItem>();
        public int weaponItemId,armorItemId,nextEquipmentId=1;
        public bool HasWeapon {get{return weaponItemId>0&&gear[1]>0;}}
        public bool HasArmor {get{return armorItemId>0&&gear[2]>0;}}
        public void EnsureCollection(){
            if(recruitStoryVersion<1){if(GraphicsGpus>=24)daveyGoatsUnlocked=true;recruitStoryVersion=1;}
            if(seenStoryScenes==null)seenStoryScenes=new System.Collections.Generic.List<int>();
            // Older builds could finish these boss chapters without recording Wrath encounters.
            // Restore only the two prior chapters; the final fight must still be won.
            if(wrathProgressVersion<1){
                int cleared=Math.Max(cityUnlocked,district),previous=cleared>=12?2:cleared>=6?1:0;
                if(stageStars!=null&&stageStars.Length>5&&stageStars[5]>0)previous=Math.Max(previous,1);
                if(stageStars!=null&&stageStars.Length>11&&stageStars[11]>0)previous=2;
                if(seenStoryScenes.Contains(7))previous=Math.Max(previous,1);
                if(seenStoryScenes.Contains(8))previous=2;
                wrathEncounters=Math.Max(wrathEncounters,previous);wrathProgressVersion=1;
            }
            wrathEncounters=wrathFreed?3:Mathf.Clamp(wrathEncounters,0,2);
            compute=Mathf.Clamp(compute,0,ComputeCapacity);momentum=Mathf.Clamp(momentum,0,100);
            if(seenStoryScenes==null)seenStoryScenes=new System.Collections.Generic.List<int>();
            if(gear==null||gear.Length!=3)gear=new[]{0,0,0};
            if(inventory==null)inventory=new System.Collections.Generic.List<EquipmentItem>();
            if(collectionVersion<2){inventory.Clear();weaponItemId=armorItemId=0;collectionVersion=2;}
            inventory.RemoveAll(i=>i==null||(i.slot!=1&&i.slot!=2)||i.family<0||i.family>3||i.tier<1||i.tier>5);
            salvage=Math.Max(0,salvage);if(clearRewardStages==null)clearRewardStages=new System.Collections.Generic.List<int>();if(restoredOverlords==null)restoredOverlords=new System.Collections.Generic.List<int>();foreach(var item in inventory)item.level=Mathf.Clamp(item.level,1,item.MaxLevel);
            nextEquipmentId=Math.Max(nextEquipmentId,inventory.Count==0?1:inventory.Max(i=>i.id)+1);
            weaponRarities=new int[4];armorRarities=new int[4];foreach(var i in inventory){var rarities=i.slot==1?weaponRarities:armorRarities;rarities[i.family]=Math.Max(rarities[i.family],i.tier);}
            var w=inventory.FirstOrDefault(i=>i.id==weaponItemId&&i.slot==1);gear[1]=w==null?0:w.tier;weapon=w==null?0:w.family;if(w==null)weaponItemId=0;
            var a=inventory.FirstOrDefault(i=>i.id==armorItemId&&i.slot==2);int armorTier=a==null?0:a.tier;maxHealth=Math.Max(1,maxHealth+armorTier-gear[2]);health=Math.Min(health,maxHealth);gear[2]=armorTier;suit=a==null?0:a.family;if(a==null)armorItemId=0;
            character=Mathf.Clamp(character,0,Characters.Length-1);outfit=Mathf.Clamp(outfit,0,3);if(!CharacterUnlocked(character))character=0;if(!OutfitUnlocked(outfit))outfit=0;
        }
        public bool EquipItem(int id){EnsureCollection();var item=inventory.FirstOrDefault(i=>i.id==id);if(item==null)return false;if(item.slot==1)weaponItemId=id;else armorItemId=id;EnsureCollection();return true;}
        public bool SelectEquipment(int slot,int family){EnsureCollection();var item=inventory.Where(i=>i.slot==slot&&i.family==family).OrderByDescending(i=>i.tier).FirstOrDefault();return item!=null&&EquipItem(item.id);}
        public void UpgradeEquippedItem(int slot){var item=inventory.FirstOrDefault(i=>i.id==(slot==1?weaponItemId:armorItemId));if(item!=null)item.level=Math.Min(item.MaxLevel,item.level+1);EnsureCollection();}
        public string Discover(int slot,int family,int tier){
            EnsureCollection();if(slot!=1&&slot!=2||family<0||family>3||tier<1||tier>5)throw new ArgumentOutOfRangeException();
            var item=new EquipmentItem{id=nextEquipmentId++,slot=slot,family=family,tier=tier};inventory.Add(item);salvage+=1;EnsureCollection();
            return Tiers[tier]+" "+item.Name+" FOUND / OPEN INVENTORY TO EQUIP";
        }
    }
}
