using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace ArtificialNegligence {
 public sealed class PixelDistrictSlider:MonoBehaviour,IBeginDragHandler {
  public ScrollRect scroll;float target;bool sliding;
  public void Step(int direction){float range=scroll.content.rect.width-scroll.viewport.rect.width;if(range<=0)return;target=Mathf.Clamp01((sliding?target:scroll.horizontalNormalizedPosition)+direction*136/range);sliding=true;scroll.StopMovement();}
  public void OnBeginDrag(PointerEventData e){sliding=false;}
  void Update(){if(!sliding)return;scroll.horizontalNormalizedPosition=Mathf.MoveTowards(scroll.horizontalNormalizedPosition,target,Time.unscaledDeltaTime*2.7f);if(Mathf.Abs(scroll.horizontalNormalizedPosition-target)<.001f)sliding=false;}
 }
}
