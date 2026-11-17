using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace ArtificialNegligence {
 public partial class GameController {
  [Serializable] class UiAuditEntry {public string screen,text,problem;public float available,required;}
  [Serializable] class UiAuditReport {public int screens,labels;public List<UiAuditEntry> issues=new List<UiAuditEntry>();}
  readonly UiAuditReport uiAudit=new UiAuditReport();
  void AuditUiLayout(string screen){
   foreach(var fit in screenRoot.GetComponentsInChildren<UiButtonText>())fit.Apply();foreach(var fit in screenRoot.GetComponentsInChildren<UiOpticalLabel>())fit.Apply();Canvas.ForceUpdateCanvases();uiAudit.screens++;
   foreach(var image in screenRoot.GetComponentsInChildren<PixelNinePatchImage>()){if(image.name!="Pixel artwork"||image.color.a<.1f)continue;var parent=image.transform.parent as RectTransform;if(parent&&Vector2.Distance(image.rectTransform.rect.size,parent.rect.size)>.5f)uiAudit.issues.Add(new UiAuditEntry{screen=screen,text=image.transform.parent.name,problem="frame does not fit resized panel"});}
   var generator=new TextGenerator();var corners=new Vector3[4];
   foreach(var text in screenRoot.GetComponentsInChildren<Text>()){
    if(!text.isActiveAndEnabled||string.IsNullOrWhiteSpace(text.text))continue;uiAudit.labels++;var rect=text.rectTransform.rect;var settings=text.GetGenerationSettings(new Vector2(rect.width,rect.height));if(text.resizeTextForBestFit){int used=text.cachedTextGenerator.fontSizeUsedForBestFit;if(used>0)settings.fontSize=used;}settings.resizeTextForBestFit=false;
    float height=generator.GetPreferredHeight(text.text,settings)/text.pixelsPerUnit;if(height>rect.height+.5f)uiAudit.issues.Add(new UiAuditEntry{screen=screen,text=text.text,problem="vertical overflow",available=rect.height,required=height});
    if(text.horizontalOverflow==HorizontalWrapMode.Overflow){float width=generator.GetPreferredWidth(text.text,settings)/text.pixelsPerUnit;if(width>rect.width+.5f)uiAudit.issues.Add(new UiAuditEntry{screen=screen,text=text.text,problem="horizontal overflow",available=rect.width,required=width});}
    var button=text.GetComponentInParent<UiButtonText>();
    if(button&&button.label==text&&UiGlyphBounds.Measure(text,out var visible)){var parent=button.GetComponent<RectTransform>();var center=parent.InverseTransformPoint(text.transform.TransformPoint(visible.center));var offset=center-(Vector3)parent.rect.center;if(Mathf.Abs(offset.x)>.6f||Mathf.Abs(offset.y)>.6f)uiAudit.issues.Add(new UiAuditEntry{screen=screen,text=text.text,problem="visible button label not centered",available=.6f,required=Mathf.Max(Mathf.Abs(offset.x),Mathf.Abs(offset.y))});var lo=parent.InverseTransformPoint(text.transform.TransformPoint(visible.min));var hi=parent.InverseTransformPoint(text.transform.TransformPoint(visible.max));if(lo.x<parent.rect.xMin+button.left-1||hi.x>parent.rect.xMax-button.right+1||lo.y<parent.rect.yMin+button.vertical-1||hi.y>parent.rect.yMax-button.vertical+1)uiAudit.issues.Add(new UiAuditEntry{screen=screen,text=text.text,problem="visible button glyph padding"});}
    // Inspect actual glyph bounds against neighboring item artwork, not broad text boxes.
    var verts=text.cachedTextGenerator.verts;if(verts.Count<=4)continue;float minX=float.MaxValue,minY=float.MaxValue,maxX=float.MinValue,maxY=float.MinValue;
    for(int v=0;v<verts.Count-4;v++){var p=text.transform.TransformPoint(verts[v].position/text.pixelsPerUnit);var local=screenRoot.InverseTransformPoint(p);minX=Mathf.Min(minX,local.x);maxX=Mathf.Max(maxX,local.x);minY=Mathf.Min(minY,local.y);maxY=Mathf.Max(maxY,local.y);}
    var glyphs=UnityEngine.Rect.MinMaxRect(minX,minY,maxX,maxY);
    foreach(var image in text.transform.parent.GetComponentsInChildren<Image>()){
     string name=image.name.ToLowerInvariant();if(!image.isActiveAndEnabled||image.color.a<.1f||image is PixelNinePatchImage||!(name.Contains("item")||name.Contains("icon")||name.Contains("portrait")||name=="gpu"||name.Contains("symbol")))continue;
     image.rectTransform.GetWorldCorners(corners);var a=screenRoot.InverseTransformPoint(corners[0]);var b=screenRoot.InverseTransformPoint(corners[2]);var art=UnityEngine.Rect.MinMaxRect(Mathf.Min(a.x,b.x),Mathf.Min(a.y,b.y),Mathf.Max(a.x,b.x),Mathf.Max(a.y,b.y));art.xMin+=2;art.xMax-=2;art.yMin+=2;art.yMax-=2;if(glyphs.Overlaps(art))uiAudit.issues.Add(new UiAuditEntry{screen=screen,text=text.text,problem="glyphs overlap artwork: "+image.name});
    }
   }
  }
  void FinishUiLayoutAudit(string path){File.WriteAllText(Path.Combine(path,"ui-layout-audit.json"),JsonUtility.ToJson(uiAudit,true));if(uiAudit.issues.Count>0)throw new Exception("UI layout audit: "+uiAudit.issues.Count+" issues; see ui-layout-audit.json");Debug.Log("AN_UI_LAYOUT_PASS / "+uiAudit.labels+" labels across "+uiAudit.screens+" captures");}
 }
}
