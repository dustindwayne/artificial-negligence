using UnityEngine;
using UnityEngine.EventSystems;
namespace ArtificialNegligence {
 public sealed class PixelChoiceFeedback:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,ISelectHandler,IDeselectHandler,IPointerEnterHandler {
  public UnityEngine.UI.Image focusMarker;bool focus;Vector2 home;RectTransform rect;bool held;float depth;
  void Awake(){rect=GetComponent<RectTransform>();home=rect.anchoredPosition;}
  void Update(){depth=Mathf.MoveTowards(depth,held?1:0,Time.unscaledDeltaTime*12);rect.anchoredPosition=home+Vector2.down*Mathf.Round(depth*2);if(focusMarker)focusMarker.color=focus?new Color(.92f,.72f,.32f,1):Color.clear;}
  public void OnPointerDown(PointerEventData e){var button=GetComponent<UnityEngine.UI.Button>();held=button&&button.interactable;}public void OnPointerUp(PointerEventData e){held=false;}public void OnPointerExit(PointerEventData e){held=focus=false;}public void OnPointerEnter(PointerEventData e){focus=true;}public void OnSelect(BaseEventData e){focus=true;}public void OnDeselect(BaseEventData e){held=focus=false;}
 }
}
