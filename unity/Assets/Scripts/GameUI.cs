using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ArtificialNegligence {
    public partial class GameController {
        Canvas canvas;RectTransform uiRoot,screenRoot;Font pixelFont;Sprite frameSprite,uiWhite,panelFill;
        Image xpFill,objectiveFill;Text xpHud;bool armorCollection;int inventoryPage,upgradeSelection;ScreenMode characterReturn=ScreenMode.CityMap,settingsReturn=ScreenMode.Title;
        Text gpuLabel,pointsLabel,healthLabel,nextLabel,buffLabel,toastLabel;GameObject toastPanel,solveDock,buffPanel;
        Text updateStatusLabel,updateButtonLabel,bossHealthHud;
        Image[] healthCells;Image miniPlayer;readonly List<Image> miniGpus=new List<Image>();
        readonly List<Text> gearLabels=new List<Text>();readonly List<Image> gearColors=new List<Image>();
        public static Color TierColor(int tier){return tier==5?Gold:tier==4?new Color(1,.57f,.15f):tier==3?new Color(.87f,.35f,1):tier==2?Accent:new Color(.57f,.84f,.65f);}
        void BuildCanvas(){
            pixelFont=Resources.Load<Font>("Fonts/Pixel");if(!pixelFont)pixelFont=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var go=new GameObject("Pixel Tech UI",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;canvas.pixelPerfect=true;
            go.GetComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
            uiRoot=new GameObject("16x9 Safe Area",typeof(RectTransform)).GetComponent<RectTransform>();uiRoot.SetParent(go.transform,false);uiRoot.anchorMin=uiRoot.anchorMax=new Vector2(.5f,.5f);uiRoot.pivot=new Vector2(.5f,.5f);uiRoot.sizeDelta=new Vector2(960,540);
            new GameObject("Event System",typeof(EventSystem),typeof(StandaloneInputModule));
            uiWhite=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),new Vector2(.5f,.5f));frameSprite=CreateFrame();InitializeGraphics();
        }
        Sprite CreateFrame(){panelFill=uiWhite;return InfoFrame();}
        RectTransform Rect(Transform parent,string name,float x,float y,float w,float h){AnchorPlayRect(parent,ref x,ref w);var rt=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rt.SetParent(parent,false);rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(x,-y);rt.sizeDelta=new Vector2(w,h);return rt;}
        Image Image(Transform parent,string name,float x,float y,float w,float h,Color color,Sprite sprite=null){var rt=Rect(parent,name,x,y,w,h);var img=sprite&&sprite.border!=Vector4.zero?(Image)rt.gameObject.AddComponent<PixelNinePatchImage>():rt.gameObject.AddComponent<Image>();img.sprite=sprite?sprite:uiWhite;if(sprite&&sprite.texture.name.StartsWith("pixel-tech-"))img.material=mode==ScreenMode.Play?PlayHudMaterial():WarmUiMaterial();img.color=color;img.raycastTarget=false;return img;}
        Text Label(Transform parent,string text,float x,float y,float w,float h,int size,Color color,TextAnchor align=TextAnchor.MiddleLeft){var rt=Rect(parent,"Text / "+text,x,y,w,h);var label=rt.gameObject.AddComponent<Text>();label.font=pixelFont;label.text=text;label.fontSize=size;label.color=color;label.alignment=align;label.raycastTarget=false;label.horizontalOverflow=HorizontalWrapMode.Wrap;label.verticalOverflow=VerticalWrapMode.Truncate;if((align==TextAnchor.MiddleCenter||align==TextAnchor.MiddleLeft))rt.gameObject.AddComponent<UiOpticalLabel>();return label;}
        RectTransform Panel(Transform parent,string name,float x,float y,float w,float h,Color accent,float alpha=.96f){
            var rt=Rect(parent,name,x,y,w,h);
            // Never put an opaque rectangle behind a sprite with stepped transparent corners.
            var fill=Image(rt,"Input surface",0,0,w,h,Color.clear,panelFill);fill.type=UnityEngine.UI.Image.Type.Sliced;
            bool action=name.StartsWith("Button / "),window=h>=300&&w>600,dialogue=name=="Story dialogue"||name=="Opening dialogue"||name=="Pickup notice"||name=="Looper chat bubble";
            var frame=Image(rt,"Pixel artwork",0,0,w,h,action||window||dialogue?Color.white:Color.clear,action?ArtworkFrame(true):window?WindowFrame():InfoFrame());frame.type=UnityEngine.UI.Image.Type.Sliced;foreach(var graphic in new[]{fill,frame}){var box=graphic.rectTransform;box.anchorMin=Vector2.zero;box.anchorMax=Vector2.one;box.offsetMin=box.offsetMax=Vector2.zero;}return rt;
        }
        Button MenuListChoice(Transform parent,string text,float x,float y,float w,float h,bool selected,Action action){
            var rt=Rect(parent,"Choice / "+text,x,y,w,h);var surface=Image(rt,"Choice hit area",0,0,w,h,Color.clear);surface.raycastTarget=true;var button=rt.gameObject.AddComponent<Button>();button.targetGraphic=surface;button.transition=UnityEngine.UI.Selectable.Transition.None;button.onClick.AddListener(()=>action());if(selected)Image(rt,"Selected pixel mark",0,10,4,h-20,Gold);Label(rt,text,14,6,w-28,h-12,18,selected?Gold:Cream);var feedback=rt.gameObject.AddComponent<PixelChoiceFeedback>();feedback.focusMarker=Image(rt,"Focus pixels",6,12,3,h-24,Color.clear);return button;
        }
        Button Button(Transform parent,string text,float x,float y,float w,float h,Color accent,Action clicked){var rt=Panel(parent,"Button / "+text,x,y,w,h,accent);var background=rt.GetChild(0).GetComponent<Image>();background.raycastTarget=true;var button=rt.gameObject.AddComponent<Button>();button.targetGraphic=background;
            var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1.4f,1.4f,1.4f,1);colors.pressedColor=new Color(2,2,2,1);colors.disabledColor=new Color(.14f,.13f,.11f,.8f);colors.fadeDuration=.12f;button.colors=colors;rt.gameObject.AddComponent<ButtonFeedback>();
            Image(rt,"Signal tab",0,h*.28f,0,0,accent);var caption=Label(rt,text,18,9,w-36,h-18,Mathf.Min(22,(int)h-18),accent,TextAnchor.MiddleCenter);var textBox=rt.gameObject.AddComponent<UiButtonText>();textBox.label=caption;if(text.Length==1){textBox.left=textBox.right=6;}textBox.Apply();if(clicked!=null)button.onClick.AddListener(()=>clicked());return button;
        }
        void Hold(Transform parent,string text,float x,float y,float w,float h,Color accent,Action<bool> changed){var b=Button(parent,text,x,y,w,h,accent,null);b.gameObject.AddComponent<HoldControl>().changed=changed;}
        void Pips(Transform parent,int value,int cap,float x,float y,Color color){for(int i=0;i<cap;i++)Image(parent,"Tier cell",x+i*24,y,18,8,i<value?color:new Color(.18f,.17f,.14f));}
        void BuildScreen(){
            controllerMenuHint=null;RefreshViewport(false);
            if(screenRoot){screenRoot.gameObject.SetActive(false);Destroy(screenRoot.gameObject);}float screenWidth=WideScene?UiWidth:960;screenRoot=Rect(uiRoot,"Screen / "+mode,(960-screenWidth)*.5f,0,screenWidth,540);
            gpuLabel=pointsLabel=healthLabel=nextLabel=buffLabel=toastLabel=null;toastPanel=solveDock=buffPanel=null;miniGpus.Clear();gearLabels.Clear();gearColors.Clear();
            updateStatusLabel=updateButtonLabel=bossHealthHud=graphicsLabel=null;
            if(mode==ScreenMode.Play){screenRoot.name="HUD_ROOT";BuildHud();return;}
            if(mode==ScreenMode.Narrative){BuildNarrative();return;}if(mode==ScreenMode.Story){BuildStory();return;}if(mode==ScreenMode.Title){BuildTitle();return;}if(mode==ScreenMode.CityMap){BuildCityMap();return;}
            Image(screenRoot,"Dim world",(960-ScreenUiWidth)*.5f-uiRoot.anchoredPosition.x,0,ScreenUiWidth,540,new Color(.015f,.014f,.012f,.8f));
            string title=mode==ScreenMode.PuzzleResult?"TERMINAL RESULT":mode==ScreenMode.Shop?"UPGRADE STATION":mode==ScreenMode.Characters?"CHARACTER / LOADOUT":mode==ScreenMode.Gear?"INVENTORY":mode==ScreenMode.Puzzle?"INTELLECT TERMINAL":mode==ScreenMode.Pause?"RUN PAUSED":mode==ScreenMode.Clear?"DISTRICT LIBERATED":mode==ScreenMode.Win?ActWinTitle:mode==ScreenMode.Dead?"RUN TERMINATED":"SETTINGS";
            Color accent=mode==ScreenMode.Dead?Pink:Accent;var window=Panel(screenRoot,title,42,30,876,480,accent,.91f);
            Label(window,title,70,21,650,22,22,Cream);
            if(mode!=ScreenMode.Win&&mode!=ScreenMode.Dead&&mode!=ScreenMode.Clear)Button(window,"X",768,15,44,36,accent,()=>SetMode(mode==ScreenMode.Characters?characterReturn:mode==ScreenMode.Settings?settingsReturn:ScreenMode.Play));
            if(mode==ScreenMode.Characters)BuildCharacters(window);else if(mode==ScreenMode.PuzzleResult)BuildPuzzleResult(window);else if(mode==ScreenMode.Shop)BuildShop(window);else if(mode==ScreenMode.Gear)BuildGear(window);else if(mode==ScreenMode.Puzzle)BuildPuzzle(window);else if(mode==ScreenMode.Pause)BuildPause(window);else if(mode==ScreenMode.Settings)BuildSettings(window);else BuildEnd(window);
        }
        void BuildTitle(){BuildLivingTitle();}
        void ActionControl(string name,Color accent,Action action,Action<bool> hold=null){
            var parent=name=="LEFT"||name=="RIGHT"||name=="DUCK"?hudLeftGrid:hudRightGrid;
            var b=Button(parent,"",0,0,HudButtonWidth,HudButtonHeight,accent,action);b.name="Touch / "+name;
            int cell=name=="LEFT"?0:name=="RIGHT"?1:name=="JUMP"?2:name=="DASH"?3:name=="DUCK"?4:5;ApplyHudControl(b,cell);if(hold!=null)b.gameObject.AddComponent<HoldControl>().changed=hold;
        }
        static string CompactHudCount(int value){return value>=1000000000?(value/1000000000f).ToString("0.#")+"B":value>=1000000?(value/1000000f).ToString("0.#")+"M":value>=10000?(value/1000f).ToString("0.#")+"K":value.ToString();}
        string fittedHudNotice;
        void BuildHud(){
            BuildHudFrame();
            BuildHudGrid();
            fittedHudNotice=null;
            healthLabel=null;
            xpHud=null;
            xpFill=null;

            healthCells=new Image[Mathf.Max(1,stats.maxHealth)];
            float gap=Mathf.Min(27,215f/healthCells.Length);
            float heartSize=Mathf.Min(22,gap-2);
            for(int i=0;i<healthCells.Length;i++)
                healthCells[i]=Image(hudHealthCell,"Health heart",i*gap,(24-heartSize)*.5f,heartSize,heartSize,HudHealth,HeartSprite(true));

            Label(hudResourceCell,"GPU",0,0,65,17,12,HudMuted);
            gpuLabel=Label(hudResourceCell,"",0,17,75,26,21,HudText);
            Image(hudResourceCell,"Counter divider",84,5,1,32,hudRailColor);
            Label(hudResourceCell,"POINTS",105,0,95,17,12,HudMuted);
            pointsLabel=Label(hudResourceCell,"",105,17,102,26,21,HudGold);
            nextLabel=null;
            objectiveFill=null;
            bossHealthHud=null;

            var gearButton=Button(hudTop,"GEAR",244,9,86,34,HudText,()=>SetMode(ScreenMode.Gear));
            SkinHudButton(gearButton);
            var upgradeButton=Button(hudTop,"UPGRADES",336,9,112,34,HudText,()=>SetMode(ScreenMode.Shop));
            SkinHudButton(upgradeButton);
            var pause=Button(hudPauseCell,"",0,0,56,34,HudText,Pause);
            SkinHudButton(pause);
            Image(pause.transform,"Pause bar left",21,9,4,16,HudText);
            Image(pause.transform,"Pause bar right",31,9,4,16,HudText);

            buffPanel=hudBoostCell.gameObject;
            buffLabel=Label(buffPanel.transform,"",0,0,102,32,12,HudMuted);
            float noticeWidth=Mathf.Min(320,UiWidth-700);
            toastPanel=Rect(hudTop,"Notification",(UiWidth-noticeWidth)*.5f,55,noticeWidth,34).gameObject;
            Image(toastPanel.transform,"Recessed face",0,0,noticeWidth,34,hudTrayFace);
            Image(toastPanel.transform,"Upper lip",6,0,noticeWidth-12,1,hudRailColor);
            Image(toastPanel.transform,"Lower lip",6,33,noticeWidth-12,1,hudRailColor);
            Image(toastPanel.transform,"Notification light",10,15,4,4,HudGold);
            toastLabel=Label(toastPanel.transform,"",22,0,noticeWidth-44,34,15,HudText,TextAnchor.MiddleCenter);
            toastPanel.AddComponent<CanvasGroup>().blocksRaycasts=false;

            ActionControl("LEFT",HudText,null,v=>leftHeld=v);
            ActionControl("RIGHT",HudText,null,v=>rightHeld=v);
            ActionControl("DUCK",HudText,null,v=>{
                duckHeld=v;
                if(v&&playerBody&&Mathf.Abs(playerBody.linearVelocity.x)>.8f)Slide();
            });
            ActionControl("DASH",HudText,Dash);
            ActionControl("FIRE",HudGold,null,v=>fireHeld=v);
            hudRightGrid.GetChild(1).GetComponent<Button>().interactable=stats.HasWeapon;
            ActionControl("JUMP",HudText,null,v=>jumpHeld=v);

            controlHint=null;
            var interact=Button(hudAiCell,"INTERACT",0,3,232,42,HudGold,OpenNearbyPuzzle);
            SkinHudButton(interact);
            solveDock=interact.gameObject;
            BuildSupportHud();
            UpdateUi();
        }

        void UpdateHudNotice(){
            hudRightGrid.GetChild(1).GetComponent<Button>().interactable=stats.HasWeapon;
            toastPanel.SetActive(toastTime>0);
            if(toastTime<=0)return;

            if(fittedHudNotice!=toast){
                fittedHudNotice=toast;
                toastLabel.text=toast;
                toastLabel.fontSize=15;
                while(toastLabel.fontSize>12&&toastLabel.preferredHeight>34)
                    toastLabel.fontSize--;
                if(toastLabel.preferredHeight>34){
                    const string ellipsis="...";
                    string shortened=toast;
                    do {
                        int end=shortened.LastIndexOf(' ');
                        shortened=(end>0?shortened.Substring(0,end):shortened.Substring(0,shortened.Length-1)).TrimEnd();
                        toastLabel.text=shortened+ellipsis;
                    } while(shortened.Length>0&&toastLabel.preferredHeight>34);
                }
            }
            toastPanel.GetComponent<CanvasGroup>().alpha=Mathf.Clamp01(toastTime/.35f);
        }

        void UpdateUi(){
            if(graphicsLabel)graphicsLabel.text=RendererProgress.Progress(stats);
            RefreshViewport();
            if(updateStatusLabel&&updater)updateStatusLabel.text=updater.Status;
            if(updateButtonLabel&&updater)updateButtonLabel.text=updater.ButtonText;
            if(mode!=ScreenMode.Play||!gpuLabel)return;UpdateSupportHud();
            if(xpHud)xpHud.text="LV "+stats.rank;if(xpFill)xpFill.rectTransform.sizeDelta=new Vector2(105*stats.xp/(float)stats.XpNeeded,4);
            gpuLabel.text=collected+"/"+totalGpus;var boss=enemies.FirstOrDefault(e=>(e.IsBoss||e.type==12)&&e.hp>0);if(bossHealthHud)bossHealthHud.text=boss!=null&&Mathf.Abs(boss.transform.position.x-playerBody.position.x)<16?EnemyDisplayName(boss.type)+"  "+boss.hp+" / "+boss.maxHp:"";pointsLabel.text=CompactHudCount(stats.points);
            for(int i=0;i<healthCells.Length;i++){bool filled=i<stats.health;healthCells[i].sprite=HeartSprite(filled);healthCells[i].color=filled?HudHealth:HudMuted;}
            if(miniPlayer)miniPlayer.rectTransform.anchoredPosition=new Vector2(60+playerBody.position.x/(worldLength/PPU)*572,-5);int count=0;
            foreach(var pickup in pickups.Where(p=>p.gpu)){if(count<miniGpus.Count)miniGpus[count].gameObject.SetActive(!pickup.taken);count++;}
            var nearest=pickups.Where(p=>p.gpu&&!p.taken).OrderBy(p=>Mathf.Abs(p.transform.position.x-playerBody.position.x)).FirstOrDefault();
            if(nextLabel)nextLabel.text=collected<RequiredGpus?"RECOVER "+Mathf.Max(0,RequiredGpus-collected)+" MORE":boss!=null?"DEFEAT UPLINK GUARD":"REACH THE UPLINK >";
            if(objectiveFill)objectiveFill.rectTransform.sizeDelta=new Vector2(306*Mathf.Clamp01(collected/(float)Mathf.Max(1,RequiredGpus)),4);
            string[] names={"SPEED+","SHIELD","MAGNET","RAPID","2X","PHASE","JUMP+"};var active=new List<string>();for(int i=0;i<buffs.Length;i++)if(buffs[i]>0)active.Add(names[i]+" "+(int)buffs[i]+"s");buffPanel.SetActive(active.Count>0);buffLabel.text=string.Join(" / ",Enumerable.Range(0,buffs.Length).Where(i=>buffs[i]>0).Take(2).Select(i=>names[i]))+(active.Count>2?" +"+(active.Count-2):"");
            UpdateHudNotice();
            bool interacting=NearbyTerminal()!=null;solveDock.SetActive(interacting);if(supportAction)supportAction.gameObject.SetActive(!interacting);if(supportLabel)supportLabel.gameObject.SetActive(!interacting);if(supportFill)supportFill.gameObject.SetActive(!interacting);if(supportKindLabel)supportKindLabel.gameObject.SetActive(!interacting);
            for(int i=0;i<gearLabels.Count;i++){gearLabels[i].text="T"+stats.gear[i];gearLabels[i].color=TierColor(stats.gear[i]);}
        }
        void EquippedSummary(Transform parent,float x,float y,float width){
            for(int n=0;n<2;n++){int slot=n+1;var item=stats.EquippedItem(slot);float rowY=y+n*55;Image(parent,"Loadout item",x,rowY,34,34,item!=null?Color.white:new Color(.3f,.28f,.24f),EquipmentIcon(slot,slot==1?stats.weapon:stats.suit)).preserveAspect=true;var name=Label(parent,item!=null?item.Name:slot==1?"UNARMED":"NO ARMOR",x+42,rowY,width-42,22,16,Cream);var detail=Label(parent,item!=null?RunStats.Tiers[item.tier]+" / LV "+item.level:"FIND EQUIPMENT IN THE CITY",x+42,rowY+23,width-42,19,12,item!=null?TierColor(item.tier):Accent);}
        }
        void BuildShop(RectTransform window){
            stats.EnsureCollection();upgradeSelection=Mathf.Clamp(upgradeSelection,0,5);
            Label(window,stats.points+" PTS    "+stats.salvage+" SALVAGE    "+stats.talents+" TALENTS",44,70,786,26,17,Gold);
            string[] names={"RUN SPEED","WEAPON","ARMOR","VITALITY","WEAPON MASTERY","DEFENSE TRAINING"};
            for(int i=0;i<6;i++){int select=i;var tab=MenuListChoice(window,names[i],40,108+i*48,240,42,i==upgradeSelection,()=>{upgradeSelection=select;BuildScreen();});tab.GetComponentInChildren<Text>().fontSize=17;}
            int slot=upgradeSelection,value=slot<4?stats.Level(slot):slot==4?stats.power:stats.defense,cap=slot<4?stats.Cap(slot):20,cost=slot<4?stats.Cost(slot):50+value*30;
            var details=Panel(window,"Upgrade details",300,108,536,284,Accent);Image(details,"Upgrade symbol",20,18,64,64,Color.white,slot==1||slot==2?EquipmentIcon(slot,slot==1?stats.weapon:stats.suit):ControlArtwork(7)).preserveAspect=true;
            Label(details,names[slot],100,20,410,30,25,Cream);Label(details,"LEVEL "+value+" / "+cap,100,55,410,24,17,Gold);
            string[] effects={"Run faster and cross the city with more control.","Improve the equipped weapon. Its found rarity stays fixed.","Improve the equipped armor. Its found rarity stays fixed.","Increase your maximum health.","Improve damage through training.","Improve your resistance through training."};
            Label(details,effects[slot],22,103,490,56,20,Cream);
            var equipped=slot==1||slot==2?stats.EquippedItem(slot):null;
            Label(details,slot==1||slot==2?(equipped!=null?equipped.Name+" / "+RunStats.Tiers[equipped.tier]:"FIND AND EQUIP AN ITEM FIRST"):"XP "+stats.xp+" / "+stats.XpNeeded,22,169,490,36,16,Accent);
            string price=cost+" PTS"+(slot==1||slot==2?" + "+stats.UpgradeSalvageCost(slot)+" SALVAGE":slot>=4?" + 1 TALENT":"");
            bool missing=(slot==1||slot==2)&&equipped==null;var buy=Button(details,missing?"ITEM REQUIRED":value>=cap?"MAX LEVEL":price,20,225,496,42,Gold,()=>{Toast(slot<4?stats.Upgrade(slot):stats.Train(slot==4));SaveProgress();BuildScreen();});buy.GetComponentInChildren<Text>().fontSize=18;buy.interactable=!missing&&value<cap&&stats.points>=cost&&(slot<4||stats.talents>0)&&(slot!=1&&slot!=2||stats.CanUpgradeItem(slot));
            Label(window,toastTime>0?toast:"RARITY IS FOUND. EACH ITEM KEEPS ITS OWN LEVEL.",42,400,550,42,14,Accent);Button(window,"BACK",642,410,194,40,Accent,()=>SetMode(ScreenMode.Play));
        }
        void BuildGear(RectTransform window){
            stats.EnsureCollection();int slot=armorCollection?2:1;var found=stats.inventory.Where(i=>i.slot==slot).OrderByDescending(i=>i.tier).ThenBy(i=>i.id).ToList();int pages=Math.Max(1,(found.Count+1)/2);inventoryPage=Mathf.Clamp(inventoryPage,0,pages-1);
            var preview=Panel(window,"Equipped runner",36,100,246,304,Accent);Image(preview,"Active character",40,22,166,157,Color.white,CharacterFrame(stats.character,0)).preserveAspect=true;EquippedSummary(preview,18,188,210);
            Button(window,"CHARACTER",36,410,246,40,Accent,()=>{characterReturn=ScreenMode.Gear;SetMode(ScreenMode.Characters);}).GetComponentInChildren<Text>().fontSize=18;
            Button(window,"WEAPONS / "+stats.WeaponCount,302,73,254,40,armorCollection?Accent:Gold,()=>{armorCollection=false;inventoryPage=0;BuildScreen();}).GetComponentInChildren<Text>().fontSize=18;Button(window,"ARMOR / "+stats.SuitCount,572,73,264,40,armorCollection?Gold:Accent,()=>{armorCollection=true;inventoryPage=0;BuildScreen();}).GetComponentInChildren<Text>().fontSize=18;
            if(found.Count==0){var empty=Panel(window,"Empty collection",302,129,534,264,Accent);Label(empty,armorCollection?"NO ARMOR FOUND":"NO WEAPONS FOUND",24,67,486,40,25,Cream,TextAnchor.MiddleCenter);Label(empty,"Explore the city and open supply boxes.\nEquip what you find.",24,125,486,65,19,Accent,TextAnchor.MiddleCenter);}
            for(int cell=0;cell<2;cell++){int at=inventoryPage*2+cell;if(at>=found.Count)break;var item=found[at];var card=Panel(window,item.Name,302,126+cell*132,534,120,TierColor(item.tier));Image(card,"Found equipment",14,15,48,48,Color.white,EquipmentIcon(slot,item.family)).preserveAspect=true;Label(card,item.Name,76,10,290,25,20,Cream);Label(card,RunStats.Tiers[item.tier]+" / LV "+item.level+" OF "+item.MaxLevel,76,39,280,22,14,TierColor(item.tier));bool active=(slot==1?stats.weaponItemId:stats.armorItemId)==item.id;var equip=Button(card,active?"EQUIPPED":"EQUIP",375,15,146,40,active?Gold:Accent,()=>{stats.EquipItem(item.id);SaveProgress();BuildScreen();});equip.GetComponentInChildren<Text>().fontSize=16;equip.interactable=!active;if(!active){var salvage=Button(card,"SALVAGE +"+stats.SalvageValue(item),375,65,146,40,Accent,()=>{stats.SalvageItem(item.id);SaveProgress();BuildScreen();});salvage.GetComponentInChildren<Text>().fontSize=14;}else Label(card,"PROTECTED",375,67,146,30,12,Gold,TextAnchor.MiddleCenter);}
            if(pages>1){Button(window,"<",306,410,44,38,Accent,()=>{inventoryPage--;BuildScreen();}).interactable=inventoryPage>0;Label(window,(inventoryPage+1)+" / "+pages,362,410,98,38,16,Cream);Button(window,">",468,410,44,38,Accent,()=>{inventoryPage++;BuildScreen();}).interactable=inventoryPage<pages-1;}
            Button(window,"BACK",642,410,194,40,Accent,()=>SetMode(ScreenMode.Play));
        }
        void BuildCharacters(RectTransform window){
            stats.EnsureCollection();cosmeticPopup=null;
            for(int row=0;row<RunStats.ActiveCharacters.Length;row++){int choice=RunStats.ActiveCharacters[row];bool unlocked=stats.CharacterUnlocked(choice),active=stats.character==choice;var select=MenuListChoice(window,"",36,92+row*51,224,46,active,()=>{stats.character=choice;ApplyCharacter();SaveProgress();BuildScreen();});Image(select.transform,"Recruit silhouette",12,5,35,35,unlocked?Color.white:Color.black,CharacterFrame(choice,0)).preserveAspect=true;Label(select.transform,RunStats.Characters[choice],54,11,156,26,18,unlocked?(active?Gold:Cream):Accent);select.interactable=unlocked&&!active;}
            var preview=Panel(window,"Active character preview",278,91,296,319,Gold);Label(preview,RunStats.Characters[stats.character],24,12,248,28,23,Cream,TextAnchor.MiddleCenter);Image(preview,"Selected character",47,46,202,206,Color.white,CharacterFrame(stats.character,0)).preserveAspect=true;
            cosmeticTrigger=Button(preview,"COSMETICS / "+RunStats.Outfits[stats.outfit]+" v",22,264,252,38,Gold,()=>OpenCosmeticDropdown(preview));cosmeticTrigger.GetComponentInChildren<Text>().fontSize=14;
            var gear=Panel(window,"Character equipment",598,91,238,319,Accent);Label(gear,"EQUIPPED",18,16,202,26,19,Cream);EquippedSummary(gear,16,65,206);Label(gear,"Items are shared\nacross your allies.",18,195,202,48,14,Accent);Button(gear,"GEAR",18,260,202,38,Accent,()=>SetMode(ScreenMode.Gear)).GetComponentInChildren<Text>().fontSize=17;
            Label(window,"UNLOCK ALLIES THROUGH PLAY.",38,416,550,26,15,Accent);Button(window,"BACK",642,410,194,40,Accent,()=>SetMode(characterReturn));
        }
        void BuildPuzzle(RectTransform window){
            if(activeTerminal==null)return;if(activeTerminal.data.puzzle.circuit){BuildCircuit(window);return;}
            var question=Panel(window,"Logic question",30,102,568,110,Accent);var text=Label(question,activeTerminal.data.puzzle.question,20,10,528,90,24,Cream);
            for(int i=0;i<3;i++){int choice=i;Button(window,(i+1)+".  "+activeTerminal.data.puzzle.options[i],40,231+i*62,548,52,i==0?Gold:Accent,()=>AnswerPuzzle(choice));}
            var art=Panel(window,"Terminal console",621,108,225,288,Accent,.7f);Image(art,"Terminal art",34,49,158,158,Color.white,itemFrames[2]);Label(art,"ONE ATTEMPT",15,213,195,36,26,Gold,TextAnchor.MiddleCenter);Label(art,"NO REROLLS",15,248,195,26,20,Accent,TextAnchor.MiddleCenter);
            Label(window,"CORRECT = POWERUP / WRONG = NO REWARD",32,414,799,30,20,Accent);
        }
        void BuildPause(RectTransform window){
            Label(window,CityRoute.Name(level),40,95,795,38,29,Accent,TextAnchor.MiddleCenter);
            var summary=Panel(window,"Runner summary",46,145,783,82,Accent,.75f);
            Label(summary,RunStats.Characters[stats.character]+" / LEVEL "+stats.rank,18,8,745,28,22,Cream);
            Label(summary,"XP "+stats.xp+" / "+stats.XpNeeded+"     "+RendererProgress.Progress(stats),18,42,745,24,16,Gold);
            string guide=controllerInUse?"STICK MOVE / A JUMP / X FIRE / B SLIDE / RB DASH\nY INTERACT / LB AI HELP / VIEW INVENTORY":Application.isMobilePlatform||qaMode?"ARROWS MOVE / HOLD DOWN TO DUCK / MOVING DOWN SLIDES\nJUMP / FIRE WITH FOUND WEAPON / DASH / EARN AI HELP":"A/D MOVE / SPACE JUMP / S DUCK / CTRL SLIDE / J FIRE / SHIFT DASH\nE INTERACT / R AI HELP / I INVENTORY / C CHARACTER / U UPGRADES";
            Label(window,guide,44,242,788,38,14,Accent,TextAnchor.MiddleCenter);
            Button(window,"RESUME",263,287,350,48,Gold,()=>SetMode(ScreenMode.Play));Button(window,"INVENTORY",46,346,247,48,Accent,()=>SetMode(ScreenMode.Gear));Button(window,"CHARACTER",314,346,247,48,Accent,()=>{characterReturn=ScreenMode.Pause;SetMode(ScreenMode.Characters);});Button(window,"UPGRADES",582,346,247,48,Accent,()=>SetMode(ScreenMode.Shop));
            Button(window,"AI TYPE / "+SupportNames[supportKind]+" >",46,406,247,42,Accent,()=>{CycleSupport();BuildScreen();}).GetComponentInChildren<Text>().fontSize=17;
            Button(window,"CITY MAP",314,406,247,42,Gold,()=>SetMode(ScreenMode.CityMap));Button(window,"SETTINGS",582,406,247,42,Accent,()=>{settingsReturn=ScreenMode.Pause;SetMode(ScreenMode.Settings);});
        }
        void BuildSettings(RectTransform window){
            Button(window,"SOUND / "+(soundOn?"ON":"OFF"),44,100,340,50,Accent,()=>{soundOn=!soundOn;PlayerPrefs.SetInt("sound",soundOn?1:0);BuildScreen();});
            Button(window,"HAPTICS / "+(haptics?"ON":"OFF"),44,170,340,50,Accent,()=>{haptics=!haptics;PlayerPrefs.SetInt("haptics",haptics?1:0);BuildScreen();});
            Button(window,"DIFFICULTY / "+DifficultyName,44,240,340,50,Accent,CycleDifficulty).GetComponentInChildren<Text>().fontSize=18;
            Label(window,"Adaptive difficulty adjusts the next stage to your performance.",58,310,312,70,18,Accent);
#if UNITY_WEBGL && !UNITY_EDITOR
            Label(window,"WEB EDITION / "+Application.version+"\nProgress saves in this browser. Reload for new releases.",440,108,366,200,20,Cream);
#else
            var update=Button(window,updater.ButtonText,440,100,366,50,Gold,()=>updater.Begin());updateButtonLabel=update.GetComponentInChildren<Text>();updateButtonLabel.fontSize=18;
            updateStatusLabel=Label(window,updater.Status,440,180,366,150,20,Cream);
#endif
            Button(window,"BACK",642,410,194,40,Accent,()=>SetMode(settingsReturn));
        }
        void BuildEnd(RectTransform window){
            Color accent=mode==ScreenMode.Dead?Pink:Gold;Label(window,mode==ScreenMode.Clear?CityRoute.Name(level)+" / COMPLETE":mode==ScreenMode.Win?ActWinMessage:"THE CITY STILL NEEDS YOU.",38,102,800,55,28,Accent,TextAnchor.MiddleCenter);
            string[] labels={"GPUs","SCORE","DISTRICT"},values={collected+"/"+totalGpus,stats.score.ToString("N0"),(level+1).ToString()};
            if(mode==ScreenMode.Win&&StoryAct(level)==2){labels[2]="RESTORED";values[2]=stats.restoredOverlords.Count+" / 5";}
            for(int i=0;i<3;i++){var card=Panel(window,labels[i],56+i*263,181,235,116,accent);Image(card,"Stat icon",13,25,56,56,Color.white,itemFrames[i==0?0:i==1?7:3]);Label(card,labels[i],79,14,139,34,22,Accent);Label(card,values[i],79,50,139,47,34,Cream);}
            Label(window,mode==ScreenMode.Clear?(string.IsNullOrEmpty(clearBonus)?"ROUTE CLEARED / UPGRADES SAVED / +2 HEALTH":clearBonus):mode==ScreenMode.Win?ActWinDetail:"PULSE TIER "+stats.gear[1]+" / ARMOR TIER "+stats.gear[2],35,328,806,46,24,accent,TextAnchor.MiddleCenter);
            Button(window,mode==ScreenMode.Clear?"CITY MAP":mode==ScreenMode.Win?ActContinueLabel:"RETRY / CITY MAP",231,392,414,54,accent,()=>{if(mode==ScreenMode.Clear||mode==ScreenMode.Win)NextLevel();else StartRun();});
        }
    }
}

