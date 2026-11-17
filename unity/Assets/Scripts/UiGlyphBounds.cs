using UnityEngine;
using UnityEngine.UI;
namespace ArtificialNegligence {
 public static class UiGlyphBounds {
  public static bool Measure(Text label,out Rect bounds){var gen=label.cachedTextGenerator;gen.Populate(label.text,label.GetGenerationSettings(label.rectTransform.rect.size));var verts=gen.verts;float left=float.MaxValue,right=float.MinValue,bottom=float.MaxValue,top=float.MinValue;bool any=false;
   for(int i=0;i+3<verts.Count-4;i+=4){float x0=float.MaxValue,x1=float.MinValue,y0=float.MaxValue,y1=float.MinValue;for(int j=0;j<4;j++){var p=verts[i+j].position/label.pixelsPerUnit;x0=Mathf.Min(x0,p.x);x1=Mathf.Max(x1,p.x);y0=Mathf.Min(y0,p.y);y1=Mathf.Max(y1,p.y);}if(x1-x0<.01f||y1-y0<.01f)continue;left=Mathf.Min(left,x0);right=Mathf.Max(right,x1);bottom=Mathf.Min(bottom,y0);top=Mathf.Max(top,y1);any=true;}
   bounds=any?Rect.MinMaxRect(left,bottom,right,top):new Rect();return any;
  }
 }
 public sealed class UiOpticalLabel:MonoBehaviour {
  Text label;Vector2 home;void Awake(){label=GetComponent<Text>();home=label.rectTransform.anchoredPosition;}
  public void Apply(){if(!label||GetComponentInParent<UiButtonText>())return;label.rectTransform.anchoredPosition=home;if(UiGlyphBounds.Measure(label,out var bounds))label.rectTransform.anchoredPosition=home+new Vector2(label.alignment==TextAnchor.MiddleLeft?0:Mathf.Round(label.rectTransform.rect.center.x-bounds.center.x),Mathf.Round(label.rectTransform.rect.center.y-bounds.center.y));}
  void LateUpdate(){Apply();}
 }
}
