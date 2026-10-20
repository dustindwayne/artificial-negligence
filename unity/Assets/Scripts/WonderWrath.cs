using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace ArtificialNegligence {
 public partial class GameController {
  bool wonderWrathUnlocked,wonderWrathEquipped;Sprite[] wonderWrathFrames;GameObject secretCodePanel;
  bool SecretCodeOpen=>secretCodePanel;
  void InitializeWonderWrath(){
   if(!Resources.Load<Texture2D>("Art/wonder-wrath"))return;
   wonderWrathFrames=SpriteAtlas.Load("wonder-wrath",4,2,64,64,32,true,4);
   bool testing=Array.Exists(Environment.GetCommandLineArgs(),a=>a.StartsWith("-qa",StringComparison.Ordinal));
   wonderWrathUnlocked=!testing&&PlayerPrefs.GetInt("wonder-wrath-unlocked",0)==1;
   wonderWrathEquipped=wonderWrathUnlocked&&PlayerPrefs.GetInt("wonder-wrath-equipped",0)==1;
  }
  void AddSecretSpot(){
   if(wonderWrathFrames==null)return;
   var spot=Image(screenRoot,"Decorative signal rivet",921,32,22,22,new Color(Gold.r,Gold.g,Gold.b,.35f),itemFrames[7]);spot.raycastTarget=true;
   var button=spot.gameObject.AddComponent<Button>();button.targetGraphic=spot;button.navigation=new Navigation{mode=Navigation.Mode.None};button.onClick.AddListener(OpenSecretCode);
  }
  void OpenSecretCode(){
   if(secretCodePanel)return;
   var blocker=Image(screenRoot,"Secret code overlay",0,0,960,540,new Color(0,0,0,.88f));blocker.raycastTarget=true;secretCodePanel=blocker.gameObject;
   var panel=Panel(blocker.transform,"Signal decoder",230,152,500,238,Gold);
   Label(panel,"PRIVATE CHANNEL",24,15,452,40,29,Gold);
   var inputRoot=Panel(panel,"Code input",24,64,452,48,Cyan);var background=inputRoot.GetChild(0).GetComponent<Image>();background.raycastTarget=true;
   var input=inputRoot.gameObject.AddComponent<InputField>();input.targetGraphic=background;input.characterLimit=32;input.contentType=InputField.ContentType.Alphanumeric;
   input.textComponent=Label(inputRoot,"",12,2,428,44,24,Cream);input.placeholder=Label(inputRoot,"ENTER CODE",12,2,428,44,23,new Color(.4f,.6f,.6f));
   var result=Label(panel,"",24,120,452,43,17,Cyan,TextAnchor.MiddleCenter);
   Action submit=()=>{bool ok=SubmitWonderCode(input.text);result.text=ok?(stats.CharacterUnlocked(4)?"WONDER WRATH UNLOCKED / CHARACTER LOADOUT":"OUTFIT UNLOCKED / FREE AI WRATH TO WEAR IT"):"SIGNAL NOT RECOGNIZED";result.color=ok?Gold:Pink;};
   Button(panel,"DECODE",24,180,215,39,Gold,submit);Button(panel,"CLOSE",261,180,215,39,Cyan,()=>{Destroy(secretCodePanel);secretCodePanel=null;});
   input.onEndEdit.AddListener(value=>{if(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.KeypadEnter))submit();});
   EventSystem.current.SetSelectedGameObject(input.gameObject);input.ActivateInputField();
  }
  bool SubmitWonderCode(string code){
   if(!string.Equals(code?.Trim(),"WonderWrath",StringComparison.Ordinal))return false;
   wonderWrathUnlocked=wonderWrathEquipped=true;
   if(!qaMode){PlayerPrefs.SetInt("wonder-wrath-unlocked",1);PlayerPrefs.SetInt("wonder-wrath-equipped",1);PlayerPrefs.Save();}
   ApplyCharacter();return true;
  }
  void SelectStandardOutfit(int choice){stats.outfit=choice;if(stats.character==4){wonderWrathEquipped=false;if(!qaMode){PlayerPrefs.SetInt("wonder-wrath-equipped",0);PlayerPrefs.Save();}}ApplyCharacter();SaveProgress();BuildScreen();}
  void ToggleWonderWrath(){wonderWrathEquipped=!wonderWrathEquipped;if(!qaMode){PlayerPrefs.SetInt("wonder-wrath-equipped",wonderWrathEquipped?1:0);PlayerPrefs.Save();}ApplyCharacter();BuildScreen();}
  void AddWonderWrathChoice(RectTransform window){
   if(!wonderWrathUnlocked||stats.character!=4)return;
   var b=Button(window,"WONDER WRATH / "+(wonderWrathEquipped?"EQUIPPED":"EQUIP"),470,328,376,27,Gold,ToggleWonderWrath);b.GetComponentInChildren<Text>().fontSize=16;
  }
  System.Collections.IEnumerator WonderWrathQa(){
   yield return null;
   var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-qaOutput");string path=at>=0&&at+1<args.Length?args[at+1]:Application.persistentDataPath;System.IO.Directory.CreateDirectory(path);
   stats=new RunStats();stats.EnsureCollection();
   if(SubmitWonderCode("wrong")||wonderWrathUnlocked)throw new Exception("Incorrect code unlocked costume");
   SetMode(ScreenMode.Title);var hotspot=screenRoot.Find("Decorative signal rivet").GetComponent<Button>();hotspot.onClick.Invoke();
   if(!SecretCodeOpen||secretCodePanel.GetComponentsInChildren<InputField>().Length!=1)throw new Exception("Single tap code input failed");
   if(!SubmitWonderCode("WonderWrath")||!wonderWrathUnlocked||stats.wrathFreed||stats.CharacterUnlocked(4)||stats.WeaponCount!=0)throw new Exception("Code bypassed gameplay unlocks");
   Destroy(secretCodePanel);secretCodePanel=null;
   stats.wrathFreed=true;stats.character=4;ApplyCharacter();if(playerFrames!=wonderWrathFrames)throw new Exception("Costume not applied");
   var sheet=new Texture2D(256,128,TextureFormat.RGBA32,false);sheet.SetPixels32(new Color32[256*128]);
   for(int i=0;i<8;i++){
    var pixels=wonderWrathFrames[i].texture.GetPixels32();if(!Array.Exists(pixels,c=>c.a>24))throw new Exception("Empty costume frame");
    for(int y=0;y<64;y++)for(int x=0;x<64;x++)if((x<4||y<4||x>=60||y>=60)&&pixels[y*64+x].a!=0)throw new Exception("Costume edge bleed / "+i);
    sheet.SetPixels32(i%4*64,(1-i/4)*64,64,64,pixels);
   }
   sheet.Apply();System.IO.File.WriteAllBytes(System.IO.Path.Combine(path,"Wonder-Wrath-game-sprite-sheet.png"),sheet.EncodeToPNG());
   var source=Resources.Load<Texture2D>("Art/wonder-wrath");var layout=JsonUtility.FromJson<AtlasLayout>(Resources.Load<TextAsset>("Art/wonder-wrath-layout").text);
   var large=SpriteAtlas.Fit(source,4,2,320,320,32,true,Array.ConvertAll(layout.frames,f=>f.Rect),Resources.Load<TextAsset>("Art/wonder-wrath-mask").bytes,16);
   var export=new Texture2D(1280,640,TextureFormat.RGBA32,false);export.SetPixels32(new Color32[1280*640]);for(int i=0;i<8;i++)export.SetPixels32(i%4*320,(1-i/4)*320,320,320,large[i].texture.GetPixels32());export.Apply();
   System.IO.File.WriteAllBytes(System.IO.Path.Combine(path,"Wonder-Wrath-clean-sprite-sheet.png"),export.EncodeToPNG());
   characterReturn=ScreenMode.CityMap;SetMode(ScreenMode.Characters);yield return new WaitForSecondsRealtime(.1f);CaptureQa(System.IO.Path.Combine(path,"wonder-wrath-loadout.png"));
   ToggleWonderWrath();if(playerFrames[0]!=wrathFrames[0])throw new Exception("Cannot restore original outfit");ToggleWonderWrath();if(playerFrames!=wonderWrathFrames)throw new Exception("Cannot re-equip costume");
   Debug.Log("AN_WONDER_WRATH_PASS / 8 isolated frames, 4px gutters, shared scale, single tap input, exact code, wrong-code rejection, story lock retained, cosmetic toggle");Application.Quit(0);
  }

 }
}
