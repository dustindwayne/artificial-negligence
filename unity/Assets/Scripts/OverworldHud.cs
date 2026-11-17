using System;
using UnityEngine;
using UnityEngine.UI;
namespace ArtificialNegligence {
 public partial class GameController {
  float MapHudBottom=>UiWidth>=1120?56:74;
  GameObject mapAreaChoices;
  Button MapHudAction(Transform hud,string text,float x,float width,Color accent,Action action){
   float height=mode==ScreenMode.Play?PlayHudTop:MapHudBottom;float buttonHeight=Mathf.Min(42,height-20);var button=Button(hud,text,x,(height-buttonHeight)*.5f,width,buttonHeight,accent,action);
   // Tabs share the HUD chassis; outlines appear only on focus or press.
   button.transform.GetChild(0).GetComponent<Image>().color=Color.clear;
   button.transform.GetChild(1).GetComponent<Image>().color=Color.white;
   button.transform.GetChild(2).gameObject.SetActive(false);
   var label=button.GetComponentInChildren<Text>();label.fontSize=21;

   return button;
  }
  void BuildOverworldHud(){
   var hud=Rect(screenRoot,"Overworld pixel HUD",0,0,UiWidth,MapHudBottom);
   var skin=Image(hud,"Pixel artwork map console",0,0,UiWidth,MapHudBottom,Color.white,InfoFrame());skin.type=UnityEngine.UI.Image.Type.Sliced;
   bool wide=UiWidth>=1120;mapTravelLabel=Label(hud,"",28,wide?18:14,wide?200:478,20,wide?12:14,Cream);
   mapAreaChoices=Rect(hud,"District slider",wide?248:18,wide?12:31,488,32).gameObject;
   var viewport=Rect(mapAreaChoices.transform,"District viewport",35,0,418,32);viewport.gameObject.AddComponent<RectMask2D>();var hit=viewport.gameObject.AddComponent<Image>();hit.color=Color.clear;hit.raycastTarget=true;
   var content=Rect(viewport,"District choices",0,0,5*136,32);
   var scroll=mapAreaChoices.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=true;scroll.vertical=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.inertia=true;scroll.decelerationRate=.08f;scroll.scrollSensitivity=24;
   var slider=mapAreaChoices.AddComponent<PixelDistrictSlider>();slider.scroll=scroll;
   string[] names={"STREET","MARKET","TRANSIT","GARDENS","SPIRE"};
   for(int i=0;i<5;i++){int area=i;bool open=mapOffset/30*30+i*6<=stats.cityUnlocked;var cell=Image(content,"District / "+names[i],i*136,0,132,32,Color.clear);cell.raycastTarget=true;var choice=cell.gameObject.AddComponent<Button>();choice.targetGraphic=cell;choice.interactable=open;choice.onClick.AddListener(()=>{int cycle=mapOffset/30;mapDistrict=area;stats.mapEntrance=cycle*30+area*6;SaveProgress();SetMode(ScreenMode.CityMap);});Label(cell.transform,names[i],4,3,124,24,16,open?(i==mapDistrict?Gold:Cream):new Color(.35f,.33f,.28f),TextAnchor.MiddleCenter);if(i==mapDistrict)for(int mark=0;mark<8;mark++)Image(cell.transform,"Selected district pixels",34+mark*8,28,5,2,Gold);}
   Button(mapAreaChoices.transform,"<",0,0,30,32,Accent,()=>slider.Step(-1)).GetComponentInChildren<Text>().fontSize=14;
   Button(mapAreaChoices.transform,">",458,0,30,32,Accent,()=>slider.Step(1)).GetComponentInChildren<Text>().fontSize=14;
   Canvas.ForceUpdateCanvases();scroll.horizontalNormalizedPosition=Mathf.Clamp01((mapDistrict*136-143)/262f);
   MapHudAction(hud,"MENU",UiWidth-438,86,Accent,()=>SetMode(ScreenMode.Title));
   MapHudAction(hud,"CHARACTER",UiWidth-332,144,Accent,()=>{characterReturn=ScreenMode.CityMap;SetMode(ScreenMode.Characters);});
   mapEnter=MapHudAction(hud,"ENTER LEVEL",UiWidth-168,156,Gold,EnterMapStage);
  }
 }
}
