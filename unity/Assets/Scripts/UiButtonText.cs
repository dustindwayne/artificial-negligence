using UnityEngine;
using UnityEngine.UI;
namespace ArtificialNegligence {
 public sealed class UiButtonText:MonoBehaviour {
  public Text label;public float left=18,right=18,vertical=9;
  public void Apply(){if(!label)return;var parent=GetComponent<RectTransform>();var rect=label.rectTransform;rect.anchoredPosition=new Vector2(left,-vertical);rect.sizeDelta=new Vector2(Mathf.Max(1,parent.rect.width-left-right),Mathf.Max(1,parent.rect.height-vertical*2));label.horizontalOverflow=HorizontalWrapMode.Overflow;label.fontSize=Mathf.Min(label.fontSize,Mathf.FloorToInt(rect.rect.height));if(UiGlyphBounds.Measure(label,out var glyphs)){var offset=rect.rect.center-glyphs.center;rect.anchoredPosition+=new Vector2(Mathf.Round(offset.x),Mathf.Round(offset.y));}}
  void LateUpdate(){Apply();}
 }
}
