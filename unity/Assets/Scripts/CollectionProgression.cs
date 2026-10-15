using System;
using System.Linq;
using UnityEngine;
namespace ArtificialNegligence {
    public partial class RunStats {
        public int collectionVersion,character,outfit;
        public int[] weaponRarities,armorRarities;
        public static readonly string[] Characters={"LOOPER","VALE","MENDER","ROOK"};
        public static readonly string[] CharacterDescriptions={"A core runner with a blue pulse pack","A rooftop mechanic","A neighborhood medic","The original street survivor"};
        public static readonly string[] Outfits={"ORIGINAL","CYAN THREAD","GOLD THREAD","VIOLET THREAD"};
        public bool CharacterUnlocked(int id){return id==0||id==1&&GraphicsGpus>=24||id==2&&cityUnlocked>=6||id==3&&cityUnlocked>=12;}
        public bool OutfitUnlocked(int id){return id==0||id==1&&rank>=3||id==2&&GraphicsGpus>=75||id==3&&cityUnlocked>=12;}
        public void EnsureCollection(){
            if(gear==null||gear.Length!=3)gear=new[]{0,1,1};
            weapon=Mathf.Clamp(weapon,0,3);suit=Mathf.Clamp(suit,0,3);
            if(weaponRarities==null||weaponRarities.Length!=4)weaponRarities=new int[4];
            if(armorRarities==null||armorRarities.Length!=4)armorRarities=new int[4];
            if(collectionVersion==0){
                int open=Math.Min(4,1+(rank-1)/2);
                for(int i=0;i<open;i++){weaponRarities[i]=Math.Max(1,gear[1]);armorRarities[i]=Math.Max(1,gear[2]);}
                weaponRarities[weapon]=Math.Max(1,gear[1]);armorRarities[suit]=Math.Max(1,gear[2]);collectionVersion=1;
            }
            for(int i=0;i<Math.Min(4,1+(rank-1)/2);i++){weaponRarities[i]=Math.Max(1,weaponRarities[i]);armorRarities[i]=Math.Max(1,armorRarities[i]);}
            character=Mathf.Clamp(character,0,3);outfit=Mathf.Clamp(outfit,0,3);
            if(!CharacterUnlocked(character))character=0;if(!OutfitUnlocked(outfit))outfit=0;
        }
        public bool SelectEquipment(int slot,int family){
            EnsureCollection();if(slot!=1&&slot!=2||family<0||family>3)return false;
            int tier=(slot==1?weaponRarities:armorRarities)[family];if(tier==0)return false;
            if(slot==1){weapon=family;gear[1]=tier;}else{int gain=tier-gear[2];suit=family;gear[2]=tier;maxHealth=Math.Max(1,maxHealth+gain);health=Math.Min(health,maxHealth);}
            return true;
        }
        public string Discover(int slot,int family,int tier){
            EnsureCollection();if(slot!=1&&slot!=2||family<0||family>3||tier<1||tier>5)throw new ArgumentOutOfRangeException();
            int[] owned=slot==1?weaponRarities:armorRarities;string name=slot==1?Weapons[family]:Suits[family]+" ARMOR";
            if(tier<=owned[family]){points+=5;score+=5;return Tiers[tier]+" "+name+" SALVAGED / +5 PTS";}
            bool newItem=owned[family]==0;owned[family]=tier;
            if(slot==1&&weapon==family)gear[1]=tier;
            if(slot==2&&suit==family){int gain=tier-gear[2];gear[2]=tier;maxHealth+=gain;health=Math.Min(maxHealth,health+gain);}
            return Tiers[tier]+" "+name+(newItem?" UNLOCKED":" COLLECTED")+" / EQUIP IN COLLECTION";
        }
    }
}
