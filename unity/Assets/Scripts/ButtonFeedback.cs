using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace ArtificialNegligence {
 public class ButtonFeedback:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler,ISelectHandler,IDeselectHandler {
  Image fill,border;Button button;Text label;Color baseFill,baseBorder;Vector2 labelHome;bool hover,pressed,selected;float value;
  void Start(){button=GetComponent<Button>();fill=transform.GetChild(0).GetComponent<Image>();border=transform.GetChild(1).GetComponent<Image>();label=GetComponentInChildren<Text>();baseFill=fill.color;baseBorder=border.color;if(label)labelHome=label.rectTransform.anchoredPosition;}
  void Update(){if(!fill)return;float target=button.interactable?(pressed?1:hover||selected?.42f:0):0;value=Mathf.MoveTowards(value,target,Time.unscaledDeltaTime*7);fill.color=Color.Lerp(baseFill,new Color(.08f,.24f,.28f,baseFill.a),value);border.color=new Color(baseBorder.r,baseBorder.g,baseBorder.b,Mathf.Lerp(baseBorder.a,1,value));if(label)label.rectTransform.anchoredPosition=labelHome+Vector2.down*(pressed?1:0);}
  public void OnPointerEnter(PointerEventData e){hover=true;}public void OnPointerExit(PointerEventData e){hover=pressed=false;}public void OnPointerDown(PointerEventData e){pressed=true;}public void OnPointerUp(PointerEventData e){pressed=false;}public void OnSelect(BaseEventData e){selected=true;}public void OnDeselect(BaseEventData e){selected=pressed=false;}
  void OnDisable(){hover=pressed=selected=false;}
 }
}
