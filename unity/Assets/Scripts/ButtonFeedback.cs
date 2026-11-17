using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace ArtificialNegligence {
 public class ButtonFeedback:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler,ISelectHandler,IDeselectHandler,ISubmitHandler {
  public bool preserveArtwork;
  Image fill,border;Button button;Text label;Color baseFill,baseBorder;Vector2 labelHome;Vector3 homeScale;bool hover,pressed,selected;float value,depth,pulse;
  void Start(){button=GetComponent<Button>();fill=transform.GetChild(0).GetComponent<Image>();border=transform.GetChild(1).GetComponent<Image>();label=GetComponentInChildren<Text>();baseFill=fill.color;baseBorder=border.color;homeScale=transform.localScale;if(label)labelHome=label.rectTransform.anchoredPosition;button.onClick.AddListener(()=>pulse=.12f);}
  void Update(){if(!fill)return;pulse=Mathf.Max(0,pulse-Time.unscaledDeltaTime);float target=button.interactable?(pressed||pulse>0?1:hover||selected?.35f:0):0;value=Mathf.MoveTowards(value,target,Time.unscaledDeltaTime*10);depth=Mathf.MoveTowards(depth,pressed||pulse>0?1:0,Time.unscaledDeltaTime*14);fill.color=Color.Lerp(baseFill,new Color(.2f,.17f,.11f,baseFill.a),value);if(!preserveArtwork){border.color=Color.Lerp(baseBorder,new Color(1,.9f,.7f,1),value*.25f);if(!button.interactable)border.color=new Color(.35f,.33f,.28f,.65f);}transform.localScale=homeScale*(1-depth*.025f);}
  public void OnPointerEnter(PointerEventData e){hover=true;}public void OnPointerExit(PointerEventData e){hover=pressed=false;}public void OnPointerDown(PointerEventData e){if(button&&button.interactable)pressed=true;}public void OnPointerUp(PointerEventData e){pressed=false;}public void OnSelect(BaseEventData e){selected=true;}public void OnDeselect(BaseEventData e){selected=pressed=false;}public void OnSubmit(BaseEventData e){pulse=.12f;}
  void OnDisable(){hover=pressed=selected=false;pulse=depth=0;if(homeScale!=Vector3.zero)transform.localScale=homeScale;if(label)label.rectTransform.anchoredPosition=labelHome;}
 }
}
