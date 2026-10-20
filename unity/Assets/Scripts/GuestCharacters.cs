using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace ArtificialNegligence {
 public partial class GameController {
  Sprite[] daveyGoatsFrames,bradJamminFrames;
  bool daveyAccountUnlocked,bradAccountUnlocked;
  void InitializeGuests(){
   daveyGoatsFrames=SpriteAtlas.Load("davey-goats",4,2,64,64,32,true,4);
   bradJamminFrames=SpriteAtlas.Load("brad-jammin",4,2,64,64,32,true,4);
   bool testing=Array.Exists(Environment.GetCommandLineArgs(),a=>a.StartsWith("-qa",StringComparison.Ordinal));
   daveyAccountUnlocked=!testing&&PlayerPrefs.GetInt("davey-goats-unlocked",0)==1;
   bradAccountUnlocked=!testing&&PlayerPrefs.GetInt("brad-jammin-unlocked",0)==1;
  }
  void ApplyGuestUnlocks(){stats.daveyGoatsUnlocked|=daveyAccountUnlocked;stats.bradJamminUnlocked|=bradAccountUnlocked;}
  bool RedeemCharacterCode(string code,out string message){
   string value=code?.Trim();int guest=value=="DaveyGoats"?5:value=="BradJammin"?6:0;
   if(guest>0){
    if(guest==5)daveyAccountUnlocked=stats.daveyGoatsUnlocked=true;else bradAccountUnlocked=stats.bradJamminUnlocked=true;
    if(!qaMode){PlayerPrefs.SetInt(guest==5?"davey-goats-unlocked":"brad-jammin-unlocked",1);PlayerPrefs.Save();}
    ApplyCharacter();message=RunStats.Characters[guest]+" UNLOCKED / CHARACTER MENU";return true;
   }
   bool ok=SubmitWonderCode(value);message=ok?(stats.CharacterUnlocked(4)?"WONDER WRATH UNLOCKED / CHARACTER LOADOUT":"OUTFIT UNLOCKED / FREE AI WRATH TO WEAR IT"):"SIGNAL NOT RECOGNIZED";return ok;
  }
  IEnumerator GuestsQa(){
   yield return null;var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-qaOutput");string path=at>=0?args[at+1]:Application.persistentDataPath;Directory.CreateDirectory(path);
   stats=new RunStats();ApplyCharacter();string message;
   if(stats.CharacterUnlocked(5)||stats.CharacterUnlocked(6)||RedeemCharacterCode("bradjammin",out message)||RedeemCharacterCode("DaveyGoat",out message))throw new Exception("Guest code gate failed");
   if(!RedeemCharacterCode("DaveyGoats",out message)||!RedeemCharacterCode("BradJammin",out message))throw new Exception("Guest unlock failed");
   if(stats.WeaponCount!=0||stats.SuitCount!=0||stats.wrathFreed||stats.CharacterUnlocked(1))throw new Exception("Guest code granted unrelated progress");
   var saved=JsonUtility.FromJson<RunStats>(JsonUtility.ToJson(stats));saved.EnsureCollection();if(!saved.CharacterUnlocked(5)||!saved.CharacterUnlocked(6))throw new Exception("Guest save lost unlocks");
   stats=new RunStats();ApplyCharacter();if(!stats.CharacterUnlocked(5)||!stats.CharacterUnlocked(6))throw new Exception("Account unlock lost on profile load");
   for(int id=5;id<=6;id++){
    stats.character=id;ApplyCharacter();if(stats.character!=id)throw new Exception("Cannot select guest");
    var sheet=new Texture2D(256,128,TextureFormat.RGBA32,false);sheet.SetPixels32(new Color32[256*128]);
    for(int i=0;i<8;i++){var pixels=playerFrames[i].texture.GetPixels32();if(!Array.Exists(pixels,c=>c.a>24))throw new Exception("Empty guest pose");for(int y=0;y<64;y++)for(int x=0;x<64;x++)if((x<4||y<4||x>=60||y>=60)&&pixels[y*64+x].a!=0)throw new Exception("Guest pose edge bleed");sheet.SetPixels32(i%4*64,(1-i/4)*64,64,64,pixels);}
    sheet.Apply();File.WriteAllBytes(Path.Combine(path,id==5?"davey-goats-sheet.png":"brad-jammin-sheet.png"),sheet.EncodeToPNG());
    characterReturn=ScreenMode.CityMap;SetMode(ScreenMode.Characters);yield return new WaitForSecondsRealtime(.2f);CaptureQa(Path.Combine(path,id==5?"davey-goats-preview.png":"brad-jammin-preview.png"));
   }
   Debug.Log("AN_GUESTS_PASS / exact codes, wrong-code rejection, profile and account unlocks, both selections, no free gear or story bypass, 16 isolated poses with 4px gutters");Application.Quit(0);
  }
 }
}
