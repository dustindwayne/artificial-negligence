using System;
using System.IO;
using UnityEngine;
namespace ArtificialNegligence {
    [Serializable] public class GameplayContent {
        public int version=1;
        public float enemyHealth=1,bossHealth=1,enemySpeed=1,enemyDamage=1,weaponDamage=1,xp=1;
        public string notes="Initial gameplay balance.";
        public static GameplayContent Current=new GameplayContent();
        static string PathName {get{return Path.Combine(Application.persistentDataPath,"gameplay-content.json");}}
        static bool Factor(float v){return !float.IsNaN(v)&&v>=.25f&&v<=4;}
        public static GameplayContent Parse(string json){
            try{var c=JsonUtility.FromJson<GameplayContent>(json);return c!=null&&c.version>0&&Factor(c.enemyHealth)&&Factor(c.bossHealth)&&Factor(c.enemySpeed)&&Factor(c.enemyDamage)&&Factor(c.weaponDamage)&&Factor(c.xp)?c:null;}catch{return null;}
        }
        public static void Load(){try{if(File.Exists(PathName)){var c=Parse(File.ReadAllText(PathName));if(c!=null&&c.version>Current.version)Current=c;}}catch{}}
        public static bool Save(GameplayContent c){
            if(c==null||c.version<=Current.version)return false;
            try{string temp=PathName+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(c));File.Copy(temp,PathName,true);File.Delete(temp);Current=c;return true;}catch{return false;}
        }
    }
}
