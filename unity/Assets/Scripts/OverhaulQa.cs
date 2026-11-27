using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace ArtificialNegligence {
 public partial class GameController {
  IEnumerator OverhaulQa(){
   yield return null;var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-qaOutput");string path=at>=0?args[at+1]:Application.persistentDataPath;Directory.CreateDirectory(path);
   stats=new RunStats();stats.EnsureCollection();if(!RequiresCampaignRestart(0)||RequiresCampaignRestart(1)||stats.inventory.Count!=0)throw new Exception("Campaign restart epoch or empty inventory invalid");for(int control=0;control<8;control++)if(!ControlArtwork(control))throw new Exception("Missing dedicated control art");LoadLevel(0);SetMode(ScreenMode.Play);if(climbRoutes.Count==0)throw new Exception("No teaching ladder");
   var duct=rows.First(row=>row.kind=="crawl");
   playerBody.position=PixelCenter(duct.x-16,218-StandingHeight*.5f);
   playerBody.linearVelocity=Vector2.zero;
   yield return new WaitForFixedUpdate();
   yield return new WaitForFixedUpdate();
   if(!CanStand()||IsDucking)throw new Exception("Standing space before duct was blocked");
   duckHeld=rightHeld=true;
   float ductDeadline=Time.time+4;
   while(playerBody.position.x<(duct.x+12)/PPU&&Time.time<ductDeadline)
    yield return new WaitForFixedUpdate();
   if(!IsDucking||CanStand()||playerBody.position.x<(duct.x+12)/PPU)
    throw new Exception("Player could not crouch into the duct");
   duckHeld=false;
   while(playerBody.position.x<(duct.x+duct.w+12)/PPU&&Time.time<ductDeadline)
    yield return new WaitForFixedUpdate();
   rightHeld=false;
   if(!CanStand()||IsDucking||playerBody.position.x<(duct.x+duct.w+12)/PPU)
    throw new Exception("Player could not stand again after the duct");
   LoadLevel(0);
   SetMode(ScreenMode.Play);
   var route=climbRoutes[0];playerBody.position=new Vector2(route.x,route.bottom+.7f);qaClimbAxis=1;yield return new WaitForSeconds(.45f);if(!isClimbing)throw new Exception("Cannot attach to ladder");
   qaClimbAxis=0;var held=playerBody.position;yield return new WaitForSeconds(.2f);if(Mathf.Abs(playerBody.position.y-held.y)>.04f)throw new Exception("Climb idle drifts");
   foreach(int id in RunStats.ActiveCharacters){stats.character=id;for(int pose=0;pose<4;pose++){climbPhase=pose;ApplyClimbVisual();if(!climbAnimations.TryGetValue(id,out var frames)||frames==null)throw new Exception("Climb art missing / "+id);var pixels=frames[pose].texture.GetPixels32();for(int y=0;y<64;y++)for(int x=0;x<64;x++)if((x<4||x>=60||y<4||y>=60)&&pixels[y*64+x].a!=0)throw new Exception("Climb art gutter bleed");}CaptureQa(Path.Combine(path,"climb-character-"+id+".png"));yield return null;}
   stats.character=0;qaClimbJump=true;yield return new WaitForFixedUpdate();qaClimbJump=false;if(isClimbing||playerBody.gravityScale!=1)throw new Exception("Cannot jump off ladder");
   qaClimbAxis=null;LoadLevel(0);SetMode(ScreenMode.Play);route=climbRoutes[0];playerBody.position=new Vector2(route.x,route.bottom+.7f);qaClimbAxis=1;float deadline=Time.time+5;yield return new WaitForSeconds(.45f);while(isClimbing&&Time.time<deadline)yield return null;qaClimbAxis=0;if(isClimbing||playerBody.position.y<route.top)throw new Exception("Top dismount blocked");
   qaClimbAxis=null;stats.cityUnlocked=3;stats.mapEntrance=0;SetMode(ScreenMode.CityMap);SelectMapDestination(1);if(mode!=ScreenMode.CityMap)throw new Exception("Map click auto-entered level");yield return new WaitForSecondsRealtime(4);if(mapStage!=1||mapDestination!=1)throw new Exception("Map walk failed");CaptureQa(Path.Combine(path,"walking-map.png"));EnterMapStage();if(mode!=ScreenMode.Play||level!=1)throw new Exception("Map confirm failed");
   stats.cityUnlocked=3;stats.mapEntrance=0;level=0;NextLevel();if(mapDestination!=1||mapStage!=0)throw new Exception("Clear does not start next-level walk");if(screenRoot.GetComponentsInChildren<UnityEngine.UI.Text>().Any(t=>t.text=="SKIP WALK"))throw new Exception("Walk skip remains");var mapRect=screenRoot.Find("Fullscreen district overworld") as RectTransform;if(!mapRect||mapRect.sizeDelta!=new Vector2(MapArtWidth,MapArtHeight))throw new Exception("Map is not fullscreen");yield return new WaitForSecondsRealtime(4);if(mapStage!=1)throw new Exception("Post-clear walk failed");
   stats.cityUnlocked=6;stats.mapEntrance=5;level=5;NextLevel();if(!mapArriving)throw new Exception("District crossing has no arrival walk");yield return new WaitForSecondsRealtime(4);if(mapArriving||mapStage!=6)throw new Exception("District arrival failed");
   stats.cityUnlocked=29;for(int area=0;area<5;area++){mapDistrict=area;stats.mapEntrance=area*6;SetMode(ScreenMode.CityMap);yield return new WaitForSecondsRealtime(.3f);var expected=Resources.Load<Texture2D>("Art/"+new[]{"fractured-city-overworld","market-overworld","transit-overworld","gardens-overworld","spire-overworld"}[area]);if(!expected||mapNodes.Length!=6)throw new Exception("District map import failed");foreach(var point in mapNodes)if(point.y+MapArtY-24<MapHudBottom)throw new Exception("HUD covers map character at entrance");foreach(var edge in districtLayout.edges)foreach(var point in edge.points)if(point.y+MapArtY-24<MapHudBottom)throw new Exception("HUD covers walking character");if((area==0||area==2||area==3)&&!mapWaterMaterial)throw new Exception("Visible water is not animated");float before=mapWaterMaterial?mapWaterMaterial.GetFloat("_FlowTime"):0;yield return new WaitForSecondsRealtime(.2f);if(mapWaterMaterial&&mapWaterMaterial.GetFloat("_FlowTime")<=before)throw new Exception("Water time is frozen");CaptureQa(Path.Combine(path,"district-map-"+area+".png"));}
   stats.health=3;SetMode(ScreenMode.Play);UpdateUi();if(healthCells.Length!=stats.maxHealth||healthCells[3].sprite!=HeartSprite(false))throw new Exception("Hearts do not match health");yield return new WaitForSecondsRealtime(.3f);if(worldDisplay.rectTransform.sizeDelta!=new Vector2(UiWidth-8,PlayViewHeight)||worldDisplay.rectTransform.anchoredPosition.y!=-PlayHudTop||Mathf.Abs(gameCamera.aspect-graphicsTexture.width/(float)graphicsTexture.height)>.001f)throw new Exception("Game viewport does not fit the HUD opening");ValidateHudGrid();CaptureQa(Path.Combine(path,"pixel-hearts.png"));
   SetMode(ScreenMode.Pause);CaptureQa(Path.Combine(path,"pause-menu.png"));settingsReturn=ScreenMode.Pause;SetMode(ScreenMode.Settings);CaptureQa(Path.Combine(path,"settings-menu.png"));SetMode(ScreenMode.Gear);CaptureQa(Path.Combine(path,"inventory-menu.png"));stats.Discover(1,0,2);stats.Discover(1,1,3);stats.Discover(1,2,4);stats.Discover(2,1,2);if(!stats.HasWeapon||!stats.HasArmor)throw new Exception("Found items failed to equip");stats.EquipItem(stats.inventory[0].id);stats.EquipItem(stats.inventory[3].id);SetMode(ScreenMode.Gear);CaptureQa(Path.Combine(path,"inventory-found.png"));SetMode(ScreenMode.Shop);CaptureQa(Path.Combine(path,"upgrades-menu.png"));characterReturn=ScreenMode.CityMap;SetMode(ScreenMode.Characters);CaptureQa(Path.Combine(path,"characters-menu.png"));
   for(int upgrade=0;upgrade<6;upgrade++){upgradeSelection=upgrade;SetMode(ScreenMode.Shop);CaptureQa(Path.Combine(path,"upgrade-"+upgrade+".png"));}
   characterReturn=ScreenMode.CityMap;SetMode(ScreenMode.Characters);OpenCosmeticDropdown(cosmeticTrigger.transform.parent as RectTransform);CaptureQa(Path.Combine(path,"cosmetics-menu.png"));CloseCosmeticDropdown();
   activeTerminal=terminals.FirstOrDefault();if(activeTerminal!=null){SetMode(ScreenMode.Puzzle);CaptureQa(Path.Combine(path,"puzzle-menu.png"));SetMode(ScreenMode.PuzzleResult);CaptureQa(Path.Combine(path,"puzzle-result.png"));}
   foreach(var end in new[]{ScreenMode.Clear,ScreenMode.Dead,ScreenMode.Win}){SetMode(end);CaptureQa(Path.Combine(path,"result-"+end+".png"));}
   for(int page=0;page<4;page++){storyPage=page;storyFractureStarted=Time.unscaledTime;SetMode(ScreenMode.Story);CaptureQa(Path.Combine(path,"opening-"+page+".png"));}
   mapDistrict=0;stats.mapEntrance=0;SetMode(ScreenMode.CityMap);var strip=mapAreaChoices.GetComponent<UnityEngine.UI.ScrollRect>();if(!strip||!strip.horizontal||strip.vertical)throw new Exception("District selector is not horizontal");float stripBefore=strip.horizontalNormalizedPosition;mapAreaChoices.GetComponent<PixelDistrictSlider>().Step(1);yield return new WaitForSecondsRealtime(.5f);if(strip.horizontalNormalizedPosition<=stripBefore)throw new Exception("District strip does not slide");CaptureQa(Path.Combine(path,"area-selector.png"));
   districtLayout.secrets=new[]{new SecretMapBranch{id="qa-hidden",label="QA SECRET",stage=0,node=new Vector2(400,240),points=new[]{new Vector2(300,240),new Vector2(400,240)}}};var secretRoot=Rect(screenRoot,"Secret visibility test",0,0,960,540);stats.discoveredSecretRoutes.Clear();BuildDiscoveredMapBranches(secretRoot);if(secretRoot.childCount!=0)throw new Exception("Hidden secret leaked onto map");stats.discoveredSecretRoutes.Add("qa-hidden");BuildDiscoveredMapBranches(secretRoot);if(secretRoot.childCount==0||!JsonUtility.FromJson<RunStats>(JsonUtility.ToJson(stats)).HasSecretRoute("qa-hidden"))throw new Exception("Secret discovery did not persist");Destroy(secretRoot.gameObject);stats.discoveredSecretRoutes.Clear();
   if(SceneLines.Length!=30||SceneTitles.Length!=30||SceneLocations.Length!=30)throw new Exception("Story scene coverage is incomplete");
   foreach(int stage in new[]{0,6,12,18,24,30,36,42,48,54,60,66,72,78,84})if(StageStoryScene(stage)<0)throw new Exception("District introduction missing");
   foreach(int stage in new[]{29,59,89})if(EndingScene(stage)!=11+StoryAct(stage))throw new Exception("Act finale missing");
   var storyStats=stats;stats=new RunStats();stats.EnsureCollection();
   for(int scene=0;scene<24;scene++){narrativeId=scene;foreach(var line in DirectedSceneLines()){var name=line.Split('|')[0];if(name=="DAVEY GOATS"||name=="MIKEY O'REDD"||name=="BRAD JAMMIN'"||name=="PRISON MIKE")throw new Exception("Unrecruited ally speaks in main story");}}
   stats=storyStats;stats.daveyGoatsUnlocked=stats.mikeyReddUnlocked=stats.bradJamminUnlocked=stats.prisonMikeUnlocked=true;
   for(int scene=0;scene<SceneLines.Length;scene++){for(int line=0;line<SceneLines[scene].Length;line++){narrativeId=scene;narrativeLine=line;narrativeShotStarted=Time.unscaledTime;SetMode(ScreenMode.Narrative);AuditUiLayout("dialogue-"+scene+"-"+line);}narrativeLine=0;narrativeShotStarted=Time.unscaledTime-3.5f;SetMode(ScreenMode.Narrative);yield return null;CaptureQa(Path.Combine(path,"cutscene-"+scene.ToString("00")+".png"));}
   stats.seenStoryScenes.Remove(5);BeginNarrative(5,ScreenMode.CityMap);var movingLayers=screenRoot.GetComponentsInChildren<CutsceneLayerMotion>();var actors=screenRoot.GetComponentsInChildren<CinematicActorMotion>();if(movingLayers.Length<3||actors.Length<2)throw new Exception("Staged cutscene depth or actors missing");var farLayer=movingLayers[0].GetComponent<RectTransform>();var actorLayer=actors[0].GetComponent<RectTransform>();var dialogue=screenRoot.Find("Story dialogue") as RectTransform;var dialogueBefore=dialogue.anchoredPosition;var farBefore=farLayer.anchoredPosition;var actorBefore=actorLayer.anchoredPosition;yield return new WaitForSecondsRealtime(1);if(farLayer.anchoredPosition==farBefore||actorLayer.anchoredPosition==actorBefore||dialogue.anchoredPosition!=dialogueBefore)throw new Exception("Actor staging or fixed dialogue failed");yield return new WaitForSecondsRealtime(2.5f);CaptureQa(Path.Combine(path,"layered-cutscene.png"));
   SetMode(ScreenMode.Title);yield return new WaitForSecondsRealtime(1);CaptureQa(Path.Combine(path,"parallax-title.png"));
   var titleDock=screenRoot.Find("Menu action dock").gameObject;
   var titleActions=screenRoot.GetComponentsInChildren<UnityEngine.UI.Button>();
   titleDock.SetActive(false);
   foreach(var action in titleActions)action.gameObject.SetActive(false);
   CaptureQa(Path.Combine(path,"title-art.png"));
   titleDock.SetActive(true);
   foreach(var action in titleActions)action.gameObject.SetActive(true);
if(screenRoot.GetComponentsInChildren<UnityEngine.UI.Text>().Any(t=>t.text=="STORY"))throw new Exception("Standalone Story menu remains");
   foreach(int wideWidth in new[]{1600,1680}){
    Screen.SetResolution(wideWidth,720,false);yield return new WaitForSecondsRealtime(.5f);qaSafeArea=new Rect(0,0,Screen.width,Screen.height);RefreshViewport();SetMode(ScreenMode.Play);RefreshGraphics(true);yield return new WaitForSecondsRealtime(.3f);
    if(Mathf.Abs(UiWidth-Screen.width*540f/Screen.height)>1||Mathf.Abs(gameCamera.aspect-graphicsTexture.width/(float)graphicsTexture.height)>.001f)throw new Exception("Widescreen viewport mismatch");ValidateHudGrid();CaptureQa(Path.Combine(path,"phone-"+wideWidth+"-play.png"));
    stats.graphicsCores=Enumerable.Range(0,150).Select(i=>"qa-wide-"+i).ToList();SetMode(ScreenMode.Play);RefreshGraphics(true);yield return new WaitForSecondsRealtime(.3f);CaptureQa(Path.Combine(path,"phone-"+wideWidth+"-restored.png"));int savedPoints=stats.points,savedCollected=collected,savedMomentum=stats.momentum,savedSupport=supportKind;stats.points=12345;collected=7;stats.momentum=75;supportKind=2;UpdateUi();yield return null;if(gpuLabel.text!="7/"+totalGpus||pointsLabel.text!=CompactHudCount(12345)||supportMeter.Count(m=>m.color==HudGold)!=3)throw new Exception("Live HUD counters or segmented meter do not update");CaptureQa(Path.Combine(path,"phone-"+wideWidth+"-live-hud.png"));stats.points=savedPoints;collected=savedCollected;stats.momentum=savedMomentum;supportKind=savedSupport;UpdateUi();stats.graphicsCores.Clear();
    mapDistrict=0;stats.mapEntrance=0;SetMode(ScreenMode.CityMap);foreach(var point in mapNodes)if(point.y+MapArtY-24<MapHudBottom)throw new Exception("Wide map HUD covers character");yield return new WaitForSecondsRealtime(.2f);CaptureQa(Path.Combine(path,"phone-"+wideWidth+"-map.png"));
    SetMode(ScreenMode.Title);yield return new WaitForSecondsRealtime(.2f);CaptureQa(Path.Combine(path,"phone-"+wideWidth+"-menu.png"));
   }
   qaSafeArea=new Rect(72,0,Screen.width-120,Screen.height);RefreshViewport();SetMode(ScreenMode.Play);RefreshGraphics(true);yield return new WaitForSecondsRealtime(.3f);ValidateHudGrid();CaptureQa(Path.Combine(path,"phone-notch-play.png"));SetMode(ScreenMode.CityMap);yield return new WaitForSecondsRealtime(.2f);CaptureQa(Path.Combine(path,"phone-notch-map.png"));SetMode(ScreenMode.Settings);CaptureQa(Path.Combine(path,"phone-notch-settings.png"));
   FinishUiLayoutAudit(path);Debug.Log("AN_OVERHAUL_QA_PASS / ladder attach, idle, jump-off and top exit; 24 climb frames and gutters; map travel then confirm; accurate empty hearts; distinct control sprites; district slider; secret visibility; staged actors and fixed dialogue; one-time epoch");Application.Quit(0);
  }
 }
}

