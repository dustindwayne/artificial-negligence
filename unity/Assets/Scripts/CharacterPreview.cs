using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace ArtificialNegligence {
 public partial class GameController {
  GameObject cosmeticPopup;Button cosmeticTrigger;
  void CloseCosmeticDropdown(){if(cosmeticPopup){cosmeticPopup.SetActive(false);Destroy(cosmeticPopup);cosmeticPopup=null;}if(cosmeticTrigger&&EventSystem.current)EventSystem.current.SetSelectedGameObject(cosmeticTrigger.gameObject);}
  void OpenCosmeticDropdown(RectTransform preview){
   if(cosmeticPopup){CloseCosmeticDropdown();return;}
   var overlay=Rect(screenRoot,"Cosmetic dropdown",0,0,960,540);cosmeticPopup=overlay.gameObject;
   var blocker=Image(overlay,"Close outside",0,0,960,540,new Color(0,0,0,.22f));blocker.raycastTarget=true;var dismiss=blocker.gameObject.AddComponent<Button>();dismiss.targetGraphic=blocker;dismiss.navigation=new Navigation{mode=Navigation.Mode.None};dismiss.onClick.AddListener(CloseCosmeticDropdown);
   bool secret=stats.character==4&&wonderWrathUnlocked;int count=secret?5:4;float height=count*42+12;
   var anchor=screenRoot.InverseTransformPoint(preview.TransformPoint(new Vector3(18,-264,0)));
   var list=Panel(overlay,"Outfit options",anchor.x,-anchor.y-height-4,264,height,Gold,1);
   string[] conditions={"STARTER","LEVEL 3","75 GPUS","12 STAGES"};var selectable=new List<Button>();
   for(int i=0;i<count;i++){
    int choice=i;bool open=i==4||stats.OutfitUnlocked(i),active=i==4?wonderWrathEquipped:stats.outfit==i&&!(stats.character==4&&wonderWrathEquipped&&wonderWrathUnlocked);
    string title=i==4?"WONDER WRATH":RunStats.Outfits[i];string detail=active?"EQUIPPED":open?"":conditions[i];
    var button=Button(list,title+(detail.Length>0?" / "+detail:""),6,6+i*42,252,38,active?Gold:Cyan,()=>{CloseCosmeticDropdown();if(choice==4){if(!wonderWrathEquipped)ToggleWonderWrath();}else SelectStandardOutfit(choice);});
    button.GetComponentInChildren<Text>().fontSize=15;button.interactable=open;if(open)selectable.Add(button);
   }
   // Keep keyboard navigation inside the open list as well as controller focus.
   for(int i=0;i<selectable.Count;i++)selectable[i].navigation=new Navigation{mode=Navigation.Mode.Explicit,selectOnUp=selectable[(i+selectable.Count-1)%selectable.Count],selectOnDown=selectable[(i+1)%selectable.Count]};
   if(EventSystem.current)EventSystem.current.SetSelectedGameObject((selectable.FirstOrDefault(b=>b.GetComponentInChildren<Text>().text.Contains("EQUIPPED"))??selectable[0]).gameObject);
  }
 }
}
