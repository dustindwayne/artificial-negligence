using UnityEngine;
using UnityEngine.UI;

namespace ArtificialNegligence {
 public partial class GameController {
  const float HudButtonWidth=72, HudControlGap=2, HudButtonHeight=52, HudControlGroupWidth=222, HudSideInset=24;
  static readonly Color HudText=new Color32(241,230,209,255);
  static readonly Color HudMuted=new Color32(185,180,169,255);
  static readonly Color HudGold=new Color32(239,188,103,255);
  static readonly Color HudHealth=new Color32(235,102,95,255);
  Color hudRailColor,hudButtonFace,hudTrayFace;
  RectTransform hudTop,hudBottom,hudHealthCell,hudResourceCell,hudPauseCell,hudLeftGrid,hudRightGrid,hudAiCell,hudBoostCell;

  void BuildHudGrid(){
   hudHealthCell=Rect(hudTop,"Health",HudSideInset,16,215,24);
   hudResourceCell=Rect(hudTop,"GPU and score",UiWidth-337,5,220,43);
   hudPauseCell=Rect(hudTop,"Pause",UiWidth-HudSideInset-56,9,56,34);
   hudLeftGrid=BuildControlGroup("Movement",HudSideInset);
   hudRightGrid=BuildControlGroup("Actions",UiWidth-HudSideInset-HudControlGroupWidth);
   hudAiCell=Rect(hudBottom,"AI help",(UiWidth-232)*.5f,14,232,48);
   hudBoostCell=Rect(hudBottom,"Active boosts",252,17,102,42);
  }

  RectTransform BuildControlGroup(string group,float x){
   var tray=Rect(hudBottom,group,x,13,HudControlGroupWidth,HudButtonHeight);
   Image(tray,"Recessed face",4,0,HudControlGroupWidth-8,HudButtonHeight,hudTrayFace);
   Image(tray,"Recess side",0,4,HudControlGroupWidth,HudButtonHeight-8,hudTrayFace);
   Image(tray,"Upper lip",6,0,HudControlGroupWidth-12,2,hudRailColor);
   Image(tray,"Lower lip",6,HudButtonHeight-2,HudControlGroupWidth-12,2,hudButtonFace);
   for(int i=1;i<3;i++)Image(tray,"Control divider",i*(HudButtonWidth+HudControlGap)-1,10,2,HudButtonHeight-20,hudRailColor);
   var root=Rect(tray,"Touch targets",1,0,HudControlGroupWidth-2,HudButtonHeight);
   var grid=root.gameObject.AddComponent<GridLayoutGroup>();
   grid.cellSize=new Vector2(HudButtonWidth,HudButtonHeight);
   grid.spacing=new Vector2(HudControlGap,0);
   grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;
   grid.constraintCount=3;
   return root;
  }

  void SkinHudButton(Button button){
   var art=button.transform.GetChild(1).GetComponent<Image>();
   art.material=null;
   art.sprite=uiWhite;
   art.type=UnityEngine.UI.Image.Type.Simple;
   art.color=hudRailColor;
   var face=Image(art.transform,"Key surface",0,0,0,0,hudButtonFace).rectTransform;
   face.anchorMin=Vector2.zero;
   face.anchorMax=Vector2.one;
   face.offsetMin=new Vector2(2,3);
   face.offsetMax=new Vector2(-2,-2);
  }

  void ValidateHudGrid(){
   Canvas.ForceUpdateCanvases();
   LayoutRebuilder.ForceRebuildLayoutImmediate(hudLeftGrid);
   LayoutRebuilder.ForceRebuildLayoutImmediate(hudRightGrid);
   Canvas.ForceUpdateCanvases();
   if(Mathf.Abs(hudAiCell.anchoredPosition.x+hudAiCell.rect.width*.5f-UiWidth*.5f)>1)
    throw new System.Exception("AI controls are not centered.");
   var controls=screenRoot.GetComponentsInChildren<Button>();
   var corners=new Vector3[4];
   var bounds=new System.Collections.Generic.List<Rect>();
   foreach(var control in controls){
    var rt=control.GetComponent<RectTransform>();
    rt.GetWorldCorners(corners);
    Vector2 low=screenRoot.InverseTransformPoint(corners[0]),high=screenRoot.InverseTransformPoint(corners[2]);
    var box=UnityEngine.Rect.MinMaxRect(low.x,-high.y,high.x,-low.y);
    bool top=box.yMin>=0&&box.yMax<=PlayHudTop+.1f;
    bool bottom=box.yMin>=PlayHudBottom-.1f&&box.yMax<=540;
    if((!top&&!bottom)||box.xMin<0||box.xMax>UiWidth+.1f)
     throw new System.Exception("Control outside HUD band: "+control.name);
    foreach(var other in bounds)if(box.Overlaps(other))
     throw new System.Exception("Overlapping controls: "+control.name);
    bounds.Add(box);
   }
   foreach(string name in new[]{"HUD upper backing","HUD lower backing"}){
    var band=screenRoot.Find(name).GetComponent<Image>();
    if(band.color.a!=1||Mathf.Abs(band.rectTransform.rect.width-UiWidth)>.1f)
     throw new System.Exception("HUD band must be opaque and full width.");
   }
  }
 }
}
