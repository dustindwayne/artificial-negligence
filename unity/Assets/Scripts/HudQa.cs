using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace ArtificialNegligence {
 public partial class GameController {
  IEnumerator HudQa(){
   yield return null;
   var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-qaOutput");string path=at>=0&&at+1<args.Length?args[at+1]:Application.persistentDataPath;Directory.CreateDirectory(path);
   stats=new RunStats();stats.EnsureCollection();LoadLevel(0);SetMode(ScreenMode.Play);
   var touch=screenRoot.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Touch / ")).ToArray();
   if(touch.Length!=5||touch.Any(b=>b.name.Contains("FIRE")||b.name.Contains("SLIDE")))throw new Exception("Unarmed touch controls still cluttered");
   if(screenRoot.GetComponentsInChildren<Text>().Any(t=>t.text=="PACK"||t.text=="STRIKE")||supportAction.interactable)throw new Exception("Unused HUD controls shown");
   yield return new WaitForSecondsRealtime(.15f);CaptureQa(Path.Combine(path,"hud-unarmed.png"));
   var down=touch.Single(b=>b.name=="Touch / DUCK").GetComponent<HoldControl>();playerBody.position=PixelCenter(60,207);playerBody.linearVelocity=Vector2.zero;grounded=true;
   down.changed(true);yield return new WaitForFixedUpdate();if(!IsDucking)throw new Exception("Combined down control cannot duck");down.changed(false);ResetTraversal();
   grounded=true;playerBody.linearVelocity=new Vector2(4,0);down.changed(true);if(!IsSliding||!duckHeld)throw new Exception("Moving down control cannot slide");down.changed(false);ResetTraversal();
   stats.Discover(1,0,1);stats.SelectEquipment(1,0);SetMode(ScreenMode.Play);
   if(screenRoot.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Touch / "))!=6)throw new Exception("Found weapon did not expose fire control");
   stats.momentum=100;stats.compute=3;UpdateSupportHud();if(!supportAction.interactable)throw new Exception("Earned AI control unavailable");
   yield return new WaitForSecondsRealtime(.15f);CaptureQa(Path.Combine(path,"hud-armed-ready.png"));supportAction.onClick.Invoke();UpdateSupportHud();if(stats.momentum!=0||stats.compute!=0||supportAction.interactable)throw new Exception("AI readiness gate failed");
   Pause();yield return new WaitForSecondsRealtime(.15f);CaptureQa(Path.Combine(path,"controls-in-menu.png"));
   screenRoot.GetComponentsInChildren<Button>().Single(b=>b.GetComponentInChildren<Text>()?.text=="SETTINGS").onClick.Invoke();if(mode!=ScreenMode.Settings||settingsReturn!=ScreenMode.Pause)throw new Exception("Settings route failed");
   screenRoot.GetComponentsInChildren<Button>().Single(b=>b.GetComponentInChildren<Text>()?.text=="X").onClick.Invoke();if(mode!=ScreenMode.Pause)throw new Exception("Settings returned to title instead of pause");
   VerifyControllerRouting();Debug.Log("AN_HUD_QA_PASS / 5 unarmed and 6 armed touch controls, combined duck/slide, earned AI gate, pause hub, contextual settings return, controller routes");Application.Quit(0);
  }
 }
}
