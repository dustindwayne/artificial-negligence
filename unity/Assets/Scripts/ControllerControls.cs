using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace ArtificialNegligence {
 public partial class GameController {
  PadFrame pad,previousPad;bool controllerInUse,padGate;float menuRepeat;Vector2 menuDirection;Text controlHint,controllerMenuHint;
  const string PadHelp="STICK / D-PAD MOVE + DUCK / A JUMP / X OR RT ATTACK / B SLIDE / RB DASH\nY TERMINAL / LB CHAT / LT SUPPORT TYPE / VIEW PACK / MENU PAUSE";
  bool PadDown(PadButton b){return pad.Held(b)&&!previousPad.Held(b);}
  bool PadHeld(PadButton b){return pad.connected&&!padGate&&pad.Held(b);}
  float PadMove=>pad.connected?pad.x:0;
  bool PadDuck=>pad.connected&&pad.y<-.5f;
  void VerifyControllerRouting(){
   if(ControllerDevice.Axis(.15f)!=0||ControllerDevice.Axis(-.15f)!=0||ControllerDevice.Axis(1)!=1||ControllerDevice.Axis(-1)!=-1)throw new System.Exception("Controller deadzone failed");
   SetMode(ScreenMode.Pause);HandleController(new PadFrame{connected=true});controllerInUse=true;
   var resume=screenRoot.GetComponentsInChildren<Button>().First(b=>b.GetComponentInChildren<Text>().text=="RESUME");EventSystem.current.SetSelectedGameObject(resume.gameObject);
   HandleController(new PadFrame{connected=true,buttons=PadButton.A});if(mode!=ScreenMode.Play||PadHeld(PadButton.A))throw new System.Exception("Controller menu submit or held-button gate failed");
   HandleController(new PadFrame{connected=true,buttons=PadButton.A});if(PadHeld(PadButton.A))throw new System.Exception("Controller menu button leaked into play");
   HandleController(new PadFrame{connected=true});HandleController(new PadFrame{connected=true,x=.75f,y=-1,buttons=PadButton.A|PadButton.X});
   if(!PadHeld(PadButton.A)||!PadHeld(PadButton.X)||!PadDuck||PadMove!=.75f)throw new System.Exception("Controller gameplay routing failed");
   HandleController(default);if(mode!=ScreenMode.Pause||PadMove!=0||PadHeld(PadButton.A))throw new System.Exception("Controller disconnect safety failed");
   stats.seenStoryScenes.Remove(0);BeginNarrative(0,ScreenMode.Play);HandleController(new PadFrame{connected=true});
   HandleController(new PadFrame{connected=true,buttons=PadButton.A});HandleController(new PadFrame{connected=true,buttons=PadButton.A});if(narrativeLine!=1)throw new System.Exception("Controller dialogue repeated on hold");
   HandleController(new PadFrame{connected=true});HandleController(new PadFrame{connected=true,buttons=PadButton.B});if(mode!=ScreenMode.Play)throw new System.Exception("Controller story skip failed");
   HandleController(default);Debug.Log("AN_CONTROLLER_QA_PASS / deadzone, menu confirm, no held-button leak, movement, duck/jump/attack routing, disconnect pause, dialogue confirm and skip");
  }
  bool TickController(){return HandleController(ControllerDevice.Poll());}
  bool HandleController(PadFrame next){
   previousPad=pad;pad=next;
   if(!pad.connected){bool lost=previousPad.connected;controllerInUse=false;padGate=true;if(EventSystem.current)EventSystem.current.sendNavigationEvents=true;if(lost&&mode==ScreenMode.Play){Pause();Toast("CONTROLLER DISCONNECTED / RECONNECT OR USE KEYBOARD");return true;}return false;}
   if(!previousPad.connected)padGate=true;
   if(pad.buttons==0)padGate=false;
   bool activity=pad.buttons!=previousPad.buttons||Mathf.Abs(pad.x)>.3f||Mathf.Abs(pad.y)>.3f;
   if(activity)controllerInUse=true;
   if(Input.GetMouseButtonDown(0)||Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.A)||Input.GetKeyDown(KeyCode.D))controllerInUse=false;
   if(EventSystem.current)EventSystem.current.sendNavigationEvents=!controllerInUse;
   if(controlHint)controlHint.text=controllerInUse?PadHelp:"A/D MOVE / SPACE JUMP / S DUCK / CTRL SLIDE / J ATTACK / SHIFT DASH\nR CHAT / Q SUPPORT TYPE / I INVENTORY / C CHARACTER / ESC PAUSE";
   if(controllerInUse&&mode!=ScreenMode.Play){if(!controllerMenuHint)controllerMenuHint=Label(screenRoot,"STICK / D-PAD SELECT     A CONFIRM     B BACK / SKIP",20,517,920,20,12,Cyan,TextAnchor.MiddleCenter);controllerMenuHint.gameObject.SetActive(true);}else if(controllerMenuHint)controllerMenuHint.gameObject.SetActive(false);
   if(padGate)return false;
   if(mode==ScreenMode.Story||mode==ScreenMode.Narrative){
    if(PadDown(PadButton.B)){if(mode==ScreenMode.Story)FinishStory();else FinishNarrative();return true;}
    if(PadDown(PadButton.A)){if(mode==ScreenMode.Story)AdvanceStory();else AdvanceNarrative();return true;}return false;
   }
   if(mode==ScreenMode.Play){
    if(PadDown(PadButton.Start)){Pause();return true;}
    if(PadDown(PadButton.Back)){SetMode(ScreenMode.Gear);return true;}
    if(PadDown(PadButton.Y)){OpenNearbyPuzzle();if(mode!=ScreenMode.Play)return true;}
    if(PadDown(PadButton.B))Slide();if(PadDown(PadButton.RB))Dash();if(PadDown(PadButton.LB))AskChat();if(PadDown(PadButton.LT))CycleSupport();return false;
   }
   if(PadDown(PadButton.B)||PadDown(PadButton.Start)&&mode==ScreenMode.Pause){
    if(mode==ScreenMode.Characters)SetMode(characterReturn);
    else if(mode==ScreenMode.Settings||mode==ScreenMode.CityMap)SetMode(ScreenMode.Title);
    else if(mode==ScreenMode.Pause||mode==ScreenMode.Gear||mode==ScreenMode.Shop)SetMode(ScreenMode.Play);
    return true;
   }
   var buttons=screenRoot.GetComponentsInChildren<Button>().Where(b=>b.IsActive()&&b.IsInteractable()).ToArray();if(buttons.Length==0)return false;
   var selected=EventSystem.current?EventSystem.current.currentSelectedGameObject:null;var current=buttons.FirstOrDefault(b=>b.gameObject==selected);
   if(controllerInUse&&!current){current=buttons.FirstOrDefault(b=>b.GetComponentInChildren<Text>()?.text!="X")??buttons[0];EventSystem.current.SetSelectedGameObject(current.gameObject);}
   var direction=Mathf.Abs(pad.x)>Mathf.Abs(pad.y)?new Vector2(Mathf.Abs(pad.x)>.5f?Mathf.Sign(pad.x):0,0):new Vector2(0,Mathf.Abs(pad.y)>.5f?Mathf.Sign(pad.y):0);
   if(direction!=menuDirection){menuDirection=direction;menuRepeat=0;}
   if(direction!=Vector2.zero&&Time.unscaledTime>=menuRepeat&&current){
    Vector2 origin=current.transform.position;var nextButton=buttons.Where(b=>b!=current).Select(b=>new{b,delta=(Vector2)b.transform.position-origin}).Where(v=>Vector2.Dot(v.delta,direction)>1).OrderBy(v=>v.delta.magnitude*(1+3*(1-Vector2.Dot(v.delta.normalized,direction)))).FirstOrDefault();
    if(nextButton!=null){current=nextButton.b;EventSystem.current.SetSelectedGameObject(current.gameObject);}menuRepeat=Time.unscaledTime+.22f;
   }
   if(current&&PadDown(PadButton.A)){current.onClick.Invoke();return true;}return false;
  }
 }
}
