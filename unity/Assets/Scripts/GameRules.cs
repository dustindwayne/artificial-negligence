using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArtificialNegligence {
    [Serializable] public class RunStats {
        public int points, score, speed, vitality, health = 5, maxHealth = 5;
        public int[] gear = {0,1,1};
        public int cityUnlocked;public int[] stageStars=new int[30];
        public int xp, rank=1, talents, power, defense, weapon, suit, module, district;
        public static readonly string[] Weapons={"PULSE", "ARC", "SCATTER", "RAIL"};
        public static readonly string[] Suits={"BALANCED", "KINETIC", "INSULATED", "LIGHT"};
        public static readonly string[] Modules={"SPRINTER", "CAPACITOR", "STABILIZER"};
        public int XpNeeded {get{return 60+(rank-1)*35;}}
        public int WeaponCount {get{return Math.Min(4,1+(rank-1)/2);}}
        public int SuitCount {get{return Math.Min(4,1+(rank-1)/2);}}
        public int ModuleCount {get{return Math.Min(3,1+(rank-1)/2);}}
        public string GainXp(int amount){amount=Math.Max(1,Mathf.RoundToInt(amount*GameplayContent.Current.xp));xp+=amount;int before=rank;while(xp>=XpNeeded){xp-=XpNeeded;rank++;talents++;points+=25;}return rank>before?"LEVEL "+rank+" / +TALENT / LOADOUTS UNLOCKED":"+"+amount+" XP";}
        public string Train(bool offense){int value=offense?power:defense;if(value>=20)return "TRAINING MAXED";int price=50+value*30;if(talents<1||points<price)return "NEED 1 TALENT + "+price+" PTS";points-=price;talents--;if(offense)power++;else defense++;return offense?"WEAPON MASTERY +1":"DEFENSE TRAINING +1";}
        public int HitDamage(int family){int damage=gear[1]+power;bool shield=family==3||family==6,heavy=family==2||family==8;
            if(weapon==1)damage=shield?damage*2:heavy?Math.Max(1,damage/2):damage;
            else if(weapon==3)damage=heavy?damage*2:shield?Math.Max(1,damage/2):damage+1;
            else if(shield)damage=Math.Max(1,damage/2);
            if(module==1&&weapon==1)damage+=2;if(module==2&&weapon==3)damage+=2;return Math.Max(1,Mathf.RoundToInt(damage*GameplayContent.Current.weaponDamage));
        }
        public int Incoming(int raw,bool energy){if(suit==1)raw+=energy?1:-1;if(suit==2)raw+=energy?-1:1;if(suit==3)raw++;return Math.Max(1,raw-defense/3);}
        public string Matchup {get{return weapon==1?"ARC breaks shields; weak vs heavy armor":weapon==2?"SCATTER clears swarms; short range":weapon==3?"RAIL breaks armor; weak vs shields":"PULSE is balanced; shields resist it";}}

        public static readonly string[] Tiers = {"NONE","COMMON","RARE","EPIC","LEGEND","ULTRA"};
        public int Cost(int slot) { return slot==0?30+speed*20:slot==1?40+(gear[1]-1)*25:slot==2?35+(gear[2]-1)*25:35+vitality*25; }
        public int Level(int slot) { return slot==0?speed:slot==3?vitality:gear[slot]; }
        public int Cap(int slot) { return slot==0?5:slot==3?3:4; }
        public string Upgrade(int slot) {
            if(Level(slot)>=Cap(slot)) return slot==1||slot==2?"ULTRA IS FOUND IN BOXES ONLY":"UPGRADE MAXED";
            int cost=Cost(slot); if(points<cost)return "NEED "+cost+" POINTS";
            points-=cost;
            if(slot==0) { speed++; return "RUNNING SPEED / LEVEL "+speed; }
            if(slot==1) {gear[1]++;return Tiers[gear[1]]+" PULSE / TIER "+gear[1];}
            if(slot==2) {gear[2]++;maxHealth++;health++;return Tiers[gear[2]]+" ARMOR / TIER "+gear[2];}
            vitality++;maxHealth++;health++;return "VITALITY / LEVEL "+vitality;
        }
        public static int RollTier(int roll) { if(roll<0||roll>=200)throw new ArgumentOutOfRangeException();return roll==0?5:roll<10?4:roll<30?3:roll<80?2:1; }
        public string Equip(int slot,int tier) {
            string name=new[]{"BOOTS","PULSE","ARMOR"}[slot];
            if(tier<=gear[slot]) {points+=5;score+=5;return Tiers[tier]+" "+name+" SALVAGED / +5 PTS";}
            int gain=tier-gear[slot];gear[slot]=tier;
            if(slot==2) {maxHealth+=gain;health=Math.Min(maxHealth,health+gain);}
            return Tiers[tier]+" "+name+" / TIER "+tier;
        }
    }
    public struct LevelRow {
        public int level; public string kind; public float x,y,w,h;
        public LevelRow(string[] s) {level=int.Parse(s[0]);kind=s[1];x=float.Parse(s[2],System.Globalization.CultureInfo.InvariantCulture);y=float.Parse(s[3],System.Globalization.CultureInfo.InvariantCulture);w=float.Parse(s[4],System.Globalization.CultureInfo.InvariantCulture);h=float.Parse(s[5],System.Globalization.CultureInfo.InvariantCulture);}
        public static List<LevelRow> Parse(string csv,int level) {return csv.Split('\n').Skip(1).Where(s=>!string.IsNullOrWhiteSpace(s)).Select(s=>new LevelRow(s.Trim().Split(','))).Where(r=>r.level==level).ToList();}
    }
    public class LogicPuzzle {
        public string question, answer,explanation; public string[] options; public int correct,family,circuitTarget,circuitInputs; public bool circuit; public int[] values;
        public LogicPuzzle(string q,string a,string b,string c) {question=q;answer=a;options=new[]{a,b,c};}
    }
    public class PuzzleFactory {
        readonly System.Random rng;readonly HashSet<string> seen=new HashSet<string>();readonly int rotation;
        public PuzzleFactory(System.Random random) {rng=random;rotation=rng.Next(12);}
        public static string CircuitOutputs(int mask){int a=mask&1,b=(mask>>1)&1,c=(mask>>2)&1,d=(mask>>3)&1;return a+" "+(a^b)+" "+(b^c)+" "+(c^d);}
        public LogicPuzzle Create(int ordinal) {
            int family=(ordinal+rotation)%12;
            while(true) {
                LogicPuzzle p;
                if(family==0) {int a=rng.Next(1,19),d=rng.Next(2,10),v=a+4*d;p=new LogicPuzzle($"Sequence: {a}, {a+d}, {a+2*d}, {a+3*d}, ?",v.ToString(),(v-d).ToString(),(v+d).ToString());}
                else if(family==1){int a=rng.Next(1,9),d=rng.Next(2,5),v=a*d*d*d;p=new LogicPuzzle($"Sequence: {a}, {a*d}, {a*d*d}, ?",v.ToString(),(v+d).ToString(),(v-d).ToString());}
                else if(family==2){int v=rng.Next(4,64);p=new LogicPuzzle($"Binary {Convert.ToString(v,2)} equals which decimal number?",v.ToString(),(v+1).ToString(),(v-1).ToString());}
                else if(family==3){int a=rng.Next(2),b=rng.Next(2),g=rng.Next(3);string name=new[]{"AND","OR","XOR"}[g],rule=new[]{"1 if both bits are 1","1 if either bit is 1","1 if the bits differ"}[g];int v=g==0?a&b:g==1?a|b:a^b;p=new LogicPuzzle($"{name} returns {rule}. {a} {name} {b} = ?",v.ToString(),(1-v).ToString(),"2");}
                else if(family==4){int n=rng.Next(1,100);p=new LogicPuzzle($"Even IDs use port A; odd IDs use port B. Unit {n} uses?",n%2==0?"PORT A":"PORT B",n%2==0?"PORT B":"PORT A","BOTH PORTS");}
                else if(family==5){int n=rng.Next(4,34);p=new LogicPuzzle($"Need >= {n} GB. A: {n-1}; B: {n}; C: {n+2}. Which qualify?","B + C","A + B","A + C");}
                else if(family==6){int n=rng.Next(1,16);p=new LogicPuzzle("Restore the circuit. Match every output to the target. A feeds output 1; A XOR B feeds 2; B XOR C feeds 3; C XOR D feeds 4.",n.ToString(),"0","16");p.circuit=true;p.circuitTarget=n;p.explanation="XOR is 1 when its inputs differ. Start with output 1 and solve each next switch from left to right.";p.question+=" Target "+CircuitOutputs(n);}
                else if(family==7){int a=rng.Next(2,10),b=rng.Next(1,6),c=rng.Next(2,5),v=(a+b)*c;p=new LogicPuzzle($"Program: x = {a}; x = x + {b}; x = x * {c}. What is x?",v.ToString(),(a+b*c).ToString(),(v+1).ToString());p.explanation="Execute assignments in order: add first, then multiply.";}
                else if(family==8){int a=rng.Next(2,11),b=rng.Next(2,11),c=rng.Next(2,11),v=a+b+c;p=new LogicPuzzle($"Route costs: Gate A to B = {a}; B to C = {b}; C to exit = {c}. Total energy?",v.ToString(),(v+2).ToString(),(v-1).ToString());p.explanation="Add the cost of each leg of the route.";}
                else if(family==9){int n=rng.Next(3,13),v=n*(n+1)/2;p=new LogicPuzzle($"A queue has {n} jobs. Job 1 costs 1 cycle, job 2 costs 2, and so on. Total cycles?",v.ToString(),(v+n+1).ToString(),(n*n).ToString());p.explanation="The sum of 1 through n is n times (n + 1), divided by 2.";}
                else if(family==10){int n=rng.Next(2,9),a=rng.Next(0,3);string who=new[]{"A","B","C"}[a];p=new LogicPuzzle($"Only one battery is charged. {who} has {n} volts; the other two have 0. Which powers the terminal?",who,new[]{"A","B","C"}[(a+1)%3],new[]{"A","B","C"}[(a+2)%3]);p.explanation="The charged battery is the only one with a voltage greater than zero.";}
                else{int n=rng.Next(5,45),v=n/4;p=new LogicPuzzle($"Each drone requires 4 cores. You have {n} cores. How many complete drones can you power?",v.ToString(),(v+1).ToString(),(v+2).ToString());p.explanation="Divide by four and round down; incomplete groups cannot power a drone.";}
                p.family=family;if(p.explanation==null)p.explanation=family<2?"Find the consistent step or multiplier between consecutive values.":family==2?"Binary place values, right to left: 1, 2, 4, 8, 16, 32.":family==3?"Apply the stated truth-table rule to the two input bits.":family==4?"Even numbers divide by two without a remainder.":"Select every capacity that meets or exceeds the stated requirement.";
                if(!seen.Add(p.question))continue;
                for(int i=2;i>0;i--){int j=rng.Next(i+1);string v=p.options[i];p.options[i]=p.options[j];p.options[j]=v;}
                p.correct=Array.IndexOf(p.options,p.answer);return p;
            }
        }
    }
    public enum ScreenMode {Title,Play,Pause,Shop,Gear,Puzzle,Clear,Win,Dead,Settings,CityMap,PuzzleResult}
    public class TerminalData {
        public LogicPuzzle puzzle;public bool attempted,solved;
        public bool Answer(int choice) {if(attempted)return false;attempted=true;solved=choice==(puzzle.circuit?puzzle.circuitTarget:puzzle.correct);return solved;}
    }
    public class HoldControl : MonoBehaviour, UnityEngine.EventSystems.IPointerDownHandler, UnityEngine.EventSystems.IPointerUpHandler, UnityEngine.EventSystems.IPointerExitHandler {
        public Action<bool> changed;
        public void OnPointerDown(UnityEngine.EventSystems.PointerEventData e){changed?.Invoke(true);}
        public void OnPointerUp(UnityEngine.EventSystems.PointerEventData e){changed?.Invoke(false);}
        public void OnPointerExit(UnityEngine.EventSystems.PointerEventData e){changed?.Invoke(false);}
        void OnDisable(){changed?.Invoke(false);}
    }
}

