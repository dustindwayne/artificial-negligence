using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace ArtificialNegligence {
 public partial class GameController {
  Sprite[] daveyGoatsFrames,bradJamminFrames,mikeyReddFrames,prisonMikeFrames;
  bool daveyAccountUnlocked,bradAccountUnlocked,mikeyAccountUnlocked,prisonMikeAccountUnlocked;
  void InitializeGuests(){
   daveyGoatsFrames=SpriteAtlas.Load("davey-goats",4,2,64,64,32,true,4);
   bradJamminFrames=SpriteAtlas.Load("brad-jammin",4,2,64,64,32,true,4);
   mikeyReddFrames=SpriteAtlas.Load("mikey-redd",4,2,64,64,32,true,4);
   prisonMikeFrames=SpriteAtlas.Load("prison-mike",4,2,64,64,32,true,4);
   bool testing=Array.Exists(Environment.GetCommandLineArgs(),a=>a.StartsWith("-qa",StringComparison.Ordinal));
   daveyAccountUnlocked=!testing&&PlayerPrefs.GetInt("davey-goats-unlocked",0)==1;
   bradAccountUnlocked=!testing&&PlayerPrefs.GetInt("brad-jammin-unlocked",0)==1;
   mikeyAccountUnlocked=!testing&&PlayerPrefs.GetInt("mikey-redd-unlocked",0)==1;
   prisonMikeAccountUnlocked=!testing&&PlayerPrefs.GetInt("prison-mike-unlocked",0)==1;
  }
  void ApplyGuestUnlocks(){stats.daveyGoatsUnlocked|=daveyAccountUnlocked;stats.bradJamminUnlocked|=bradAccountUnlocked;stats.mikeyReddUnlocked|=mikeyAccountUnlocked;stats.prisonMikeUnlocked|=prisonMikeAccountUnlocked;}
  bool RedeemCharacterCode(string code,out string message){
   bool ok=SubmitWonderCode(code);message=ok?(stats.CharacterUnlocked(4)?"WONDER WRATH UNLOCKED / CHARACTER LOADOUT":"OUTFIT UNLOCKED / FREE AI WRATH TO WEAR IT"):"SIGNAL NOT RECOGNIZED";return ok;
  }
  IEnumerator GuestsQa(){
   yield return null;var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-qaOutput");string path=at>=0?args[at+1]:Application.persistentDataPath;Directory.CreateDirectory(path);
   stats=new RunStats();ApplyCharacter();string message;
   foreach(int id in new[]{5,6,7,8})if(stats.CharacterUnlocked(id))throw new Exception("Fresh recruit unlocked");
   foreach(string rejected in new[]{"DaveyGoats","BradJammin","PrisonMike","mikeyredd","wrong"})if(RedeemCharacterCode(rejected,out message))throw new Exception("Recruit code accepted");
   stats.graphicsCores=new System.Collections.Generic.List<string>();for(int i=0;i<24;i++)stats.graphicsCores.Add("guest:"+i);
   if(stats.CharacterUnlocked(5))throw new Exception("Davey unlocked by GPU count without virtual defeat");
   stats.cityUnlocked=5;if(stats.CharacterUnlocked(6))throw new Exception("Brad unlocked too early");stats.cityUnlocked=6;if(!stats.CharacterUnlocked(6)||stats.CharacterUnlocked(8))throw new Exception("Brad stage gate failed");
   stats.cityUnlocked=11;if(stats.CharacterUnlocked(8))throw new Exception("Prison Mike unlocked too early");stats.cityUnlocked=12;if(!stats.CharacterUnlocked(8))throw new Exception("Prison Mike stage gate failed");
   stats.mikeyVirtualFreed=stats.daveyVirtualFreed=true;
   if(stats.WeaponCount!=0||stats.SuitCount!=0||stats.wrathFreed)throw new Exception("Recruit unlock granted unrelated progress");
   var saved=JsonUtility.FromJson<RunStats>(JsonUtility.ToJson(stats));saved.EnsureCollection();foreach(int id in new[]{5,6,7,8})if(!saved.CharacterUnlocked(id))throw new Exception("Recruit save lost unlock");
   var legacy=new RunStats{daveyGoatsUnlocked=true,bradJamminUnlocked=true,mikeyReddUnlocked=true};legacy.EnsureCollection();if(!legacy.CharacterUnlocked(5)||!legacy.CharacterUnlocked(6)||!legacy.CharacterUnlocked(7))throw new Exception("Prior recruit unlock removed");
   foreach(int id in new[]{1,2,3}){var retired=new RunStats{character=id,cityUnlocked=30};retired.EnsureCollection();if(retired.character!=0||retired.CharacterUnlocked(id))throw new Exception("Retired character still selectable");}
   stats=saved;ApplyCharacter();
   for(int id=5;id<=8;id++){
    stats.character=id;ApplyCharacter();if(stats.character!=id)throw new Exception("Cannot select guest");
    var sheet=new Texture2D(256,128,TextureFormat.RGBA32,false);sheet.SetPixels32(new Color32[256*128]);
    for(int i=0;i<8;i++){var pixels=playerFrames[i].texture.GetPixels32();if(!Array.Exists(pixels,c=>c.a>24))throw new Exception("Empty guest pose");for(int y=0;y<64;y++)for(int x=0;x<64;x++)if((x<4||y<4||x>=60||y>=60)&&pixels[y*64+x].a!=0)throw new Exception("Guest pose edge bleed");sheet.SetPixels32(i%4*64,(1-i/4)*64,64,64,pixels);}
    sheet.Apply();File.WriteAllBytes(Path.Combine(path,id==5?"davey-goats-sheet.png":id==6?"brad-jammin-sheet.png":id==7?"mikey-redd-sheet.png":"prison-mike-sheet.png"),sheet.EncodeToPNG());
    characterReturn=ScreenMode.CityMap;SetMode(ScreenMode.Characters);yield return new WaitForSecondsRealtime(.2f);CaptureQa(Path.Combine(path,id==5?"davey-goats-preview.png":id==6?"brad-jammin-preview.png":id==7?"mikey-redd-preview.png":"prison-mike-preview.png"));
   }
   Debug.Log("AN_GUESTS_PASS / virtual-host unlock flags and stage 6/12 gates, all old recruit codes rejected, save/legacy unlocks, retired-character migration, four selections, no free gear/story bypass, 32 isolated poses with 4px gutters");Application.Quit(0);
  }
 }
}
