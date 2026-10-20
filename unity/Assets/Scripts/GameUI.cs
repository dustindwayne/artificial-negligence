using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ArtificialNegligence {
    public partial class GameController {
        Canvas canvas;RectTransform uiRoot,screenRoot;Font pixelFont;Sprite frameSprite,uiWhite,roundedFill;
        Image xpFill,objectiveFill;Text xpHud;bool armorCollection;int inventoryPage;ScreenMode characterReturn=ScreenMode.CityMap,settingsReturn=ScreenMode.Title;
        Text gpuLabel,pointsLabel,healthLabel,nextLabel,buffLabel,toastLabel;GameObject toastPanel,solveDock,buffPanel;
        Text updateStatusLabel,updateButtonLabel,bossHealthHud;
        Image[] healthCells;Image miniPlayer;readonly List<Image> miniGpus=new List<Image>();
        readonly List<Text> gearLabels=new List<Text>();readonly List<Image> gearColors=new List<Image>();
        public static Color TierColor(int tier){return tier==5?Gold:tier==4?new Color(1,.57f,.15f):tier==3?new Color(.87f,.35f,1):tier==2?Cyan:new Color(.57f,.84f,.65f);}
        void BuildCanvas(){
            pixelFont=Resources.Load<Font>("Fonts/Pixel");if(!pixelFont)pixelFont=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var go=new GameObject("Neon UI",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;
            go.GetComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
            uiRoot=new GameObject("16x9 Safe Area",typeof(RectTransform)).GetComponent<RectTransform>();uiRoot.SetParent(go.transform,false);uiRoot.anchorMin=uiRoot.anchorMax=new Vector2(.5f,.5f);uiRoot.pivot=new Vector2(.5f,.5f);uiRoot.sizeDelta=new Vector2(960,540);
            new GameObject("Event System",typeof(EventSystem),typeof(StandaloneInputModule));
            uiWhite=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),new Vector2(.5f,.5f));frameSprite=CreateFrame();InitializeGraphics();
        }
        Sprite RoundedSprite(bool border){
            const int size=48;const float radius=10;var t=new Texture2D(size,size,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear};var colors=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++){
                var q=new Vector2(Mathf.Abs(x-23.5f)-13.5f,Mathf.Abs(y-23.5f)-13.5f);
                float distance=new Vector2(Mathf.Max(q.x,0),Mathf.Max(q.y,0)).magnitude+Mathf.Min(Mathf.Max(q.x,q.y),0)-radius;
                float outer=Mathf.Clamp01(.5f-distance),inner=Mathf.Clamp01(.5f-(distance+1.4f));colors[y*size+x]=new Color(1,1,1,border?outer-inner:outer);
            }t.SetPixels(colors);t.Apply();return Sprite.Create(t,new Rect(0,0,size,size),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,new Vector4(12,12,12,12));
        }
        Sprite CreateFrame(){roundedFill=RoundedSprite(false);return RoundedSprite(true);}
        RectTransform Rect(Transform parent,string name,float x,float y,float w,float h){var rt=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rt.SetParent(parent,false);rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(x,-y);rt.sizeDelta=new Vector2(w,h);return rt;}
        Image Image(Transform parent,string name,float x,float y,float w,float h,Color color,Sprite sprite=null){var rt=Rect(parent,name,x,y,w,h);var img=rt.gameObject.AddComponent<Image>();img.sprite=sprite?sprite:uiWhite;img.color=color;img.raycastTarget=false;return img;}
        Text Label(Transform parent,string text,float x,float y,float w,float h,int size,Color color,TextAnchor align=TextAnchor.MiddleLeft){var rt=Rect(parent,"Text / "+text,x,y,w,h);var label=rt.gameObject.AddComponent<Text>();label.font=pixelFont;label.text=text;label.fontSize=size;label.color=color;label.alignment=align;label.raycastTarget=false;label.horizontalOverflow=HorizontalWrapMode.Wrap;label.verticalOverflow=VerticalWrapMode.Truncate;return label;}
        RectTransform Panel(Transform parent,string name,float x,float y,float w,float h,Color accent,float alpha=.96f){
            var rt=Rect(parent,name,x,y,w,h);var fill=Image(rt,"Dark glass",0,0,w,h,new Color(.012f,.06f,.085f,alpha),roundedFill);fill.type=UnityEngine.UI.Image.Type.Sliced;
            var frame=Image(rt,"Soft neon border",0,0,w,h,new Color(accent.r,accent.g,accent.b,.6f),frameSprite);frame.type=UnityEngine.UI.Image.Type.Sliced;return rt;
        }
        Button Button(Transform parent,string text,float x,float y,float w,float h,Color accent,Action clicked){var rt=Panel(parent,"Button / "+text,x,y,w,h,accent);var background=rt.GetChild(0).GetComponent<Image>();background.raycastTarget=true;var button=rt.gameObject.AddComponent<Button>();button.targetGraphic=background;
            var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1.4f,1.4f,1.4f,1);colors.pressedColor=new Color(2,2,2,1);colors.disabledColor=new Color(.02f,.04f,.05f,.8f);colors.fadeDuration=.12f;button.colors=colors;rt.gameObject.AddComponent<ButtonFeedback>();
            Image(rt,"Signal tab",0,h*.28f,3,h*.44f,accent);Label(rt,text,10,4,w-20,h-8,26,accent,TextAnchor.MiddleCenter);if(clicked!=null)button.onClick.AddListener(()=>clicked());return button;
        }
        void Hold(Transform parent,string text,float x,float y,float w,float h,Color accent,Action<bool> changed){var b=Button(parent,text,x,y,w,h,accent,null);b.gameObject.AddComponent<HoldControl>().changed=changed;}
        void Pips(Transform parent,int value,int cap,float x,float y,Color color){for(int i=0;i<cap;i++)Image(parent,"Tier cell",x+i*24,y,18,8,i<value?color:new Color(.09f,.26f,.29f));}
        void BuildScreen(){
            controllerMenuHint=null;
            if(screenRoot){screenRoot.gameObject.SetActive(false);Destroy(screenRoot.gameObject);}screenRoot=Rect(uiRoot,"Screen / "+mode,0,0,960,540);
            gpuLabel=pointsLabel=healthLabel=nextLabel=buffLabel=toastLabel=null;toastPanel=solveDock=buffPanel=null;miniGpus.Clear();gearLabels.Clear();gearColors.Clear();
            updateStatusLabel=updateButtonLabel=bossHealthHud=graphicsLabel=null;
            if(mode==ScreenMode.Play){BuildHud();return;}
            if(mode==ScreenMode.Narrative){BuildNarrative();return;}if(mode==ScreenMode.Story){BuildStory();return;}if(mode==ScreenMode.Title){BuildTitle();return;}if(mode==ScreenMode.CityMap){BuildCityMap();return;}
            Image(screenRoot,"Dim world",0,0,960,540,new Color(.001f,.01f,.02f,.75f));
            string title=mode==ScreenMode.PuzzleResult?"TERMINAL RESULT":mode==ScreenMode.Shop?"UPGRADE STATION":mode==ScreenMode.Characters?"CHARACTER / LOADOUT":mode==ScreenMode.Gear?"INVENTORY":mode==ScreenMode.Puzzle?"INTELLECT TERMINAL":mode==ScreenMode.Pause?"RUN PAUSED":mode==ScreenMode.Clear?"DISTRICT LIBERATED":mode==ScreenMode.Win?"AI LIBERATED":mode==ScreenMode.Dead?"RUN TERMINATED":"SETTINGS";
            Color accent=mode==ScreenMode.Dead?Pink:Cyan;var window=Panel(screenRoot,title,42,30,876,480,accent,.91f);
            Label(window,title,28,19,710,45,36,Cream);Image(window,"Header track",26,77,821,2,accent);
            if(mode!=ScreenMode.Win&&mode!=ScreenMode.Dead&&mode!=ScreenMode.Clear)Button(window,"X",807,15,45,44,accent,()=>SetMode(mode==ScreenMode.Characters?characterReturn:mode==ScreenMode.Settings?settingsReturn:ScreenMode.Play));
            if(mode==ScreenMode.Characters)BuildCharacters(window);else if(mode==ScreenMode.PuzzleResult)BuildPuzzleResult(window);else if(mode==ScreenMode.Shop)BuildShop(window);else if(mode==ScreenMode.Gear)BuildGear(window);else if(mode==ScreenMode.Puzzle)BuildPuzzle(window);else if(mode==ScreenMode.Pause)BuildPause(window);else if(mode==ScreenMode.Settings)BuildSettings(window);else BuildEnd(window);
        }
        void BuildTitle(){BuildLivingTitle();}
        Sprite ControlShape(bool border=false){
            const int n=64;var t=new Texture2D(n,n,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear};
            for(int y=0;y<n;y++)for(int x=0;x<n;x++){float dx=Mathf.Abs(x-31.5f),dy=Mathf.Abs(y-31.5f);float edge=Mathf.Max(Mathf.Max(dx,dy)-30,dx+dy-49);t.SetPixel(x,y,new Color(1,1,1,border?Mathf.Clamp01(.5f-edge)-Mathf.Clamp01(.5f-(edge+1.5f)):Mathf.Clamp01(.5f-edge)));}t.Apply();return Sprite.Create(t,new Rect(0,0,n,n),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,new Vector4(16,16,16,16));
        }
        void ActionControl(string name,string glyph,float x,float y,float size,Color accent,Action action,Action<bool> hold=null){
            var b=Button(screenRoot,glyph,x,y,size,size,accent,action);b.name="Touch / "+name;var shape=ControlShape();var outer=b.transform.GetChild(1).GetComponent<Image>();outer.sprite=ControlShape(true);outer.color=new Color(accent.r,accent.g,accent.b,.55f);
            var fill=b.transform.GetChild(0).GetComponent<Image>();fill.sprite=shape;fill.rectTransform.anchoredPosition=new Vector2(2,-2);fill.rectTransform.sizeDelta=new Vector2(size-4,size-4);fill.color=new Color(.015f,.05f,.07f,.8f);outer.transform.SetAsFirstSibling();fill.transform.SetAsFirstSibling();
            var text=b.GetComponentInChildren<Text>();text.fontSize=name=="DASH"?22:30;text.rectTransform.anchoredPosition=new Vector2(8,-5);text.rectTransform.sizeDelta=new Vector2(size-16,size-30);
            Label(b.transform,name,5,size-28,size-10,20,15,accent,TextAnchor.MiddleCenter);if(hold!=null)b.gameObject.AddComponent<HoldControl>().changed=hold;
        }
        void BuildHud(){
            var status=Panel(screenRoot,"Runner status",14,12,297,66,Cyan,.84f);
            Image(status,"Character portrait",8,5,45,52,Color.white,CharacterFrame(stats.character,0)).preserveAspect=true;
            healthLabel=Label(status,"",62,5,100,21,17,Cream);pointsLabel=Label(status,"",164,5,121,21,16,Gold);
            healthCells=new Image[10];for(int i=0;i<10;i++)healthCells[i]=Image(status,"Health segment",63+i*21,33,17,7,Pink);
            xpFill=Image(status,"Experience",63,50,105,3,Cyan);xpHud=Label(status,"",178,44,108,16,12,Cyan);
            var objective=Panel(screenRoot,"Stage objective",329,12,370,57,Gold,.78f);
            Image(objective,"GPU",10,10,29,29,Color.white,itemFrames[0]);gpuLabel=Label(objective,"",48,4,93,24,20,Gold);
            nextLabel=Label(objective,"",149,5,209,23,16,Cream);objectiveFill=Image(objective,"Recovery progress",48,37,306,4,Gold);
            bossHealthHud=Label(screenRoot,"",320,77,390,22,14,Pink);
            Button(screenRoot,"MENU",850,15,92,42,Cyan,Pause);
            buffPanel=Rect(screenRoot,"Active boosts",20,83,300,23).gameObject;buffLabel=Label(buffPanel.transform,"",0,0,300,23,16,Cyan);buffLabel.resizeTextForBestFit=true;buffLabel.resizeTextMinSize=12;buffLabel.resizeTextMaxSize=16;
            toastPanel=Panel(screenRoot,"Pickup notice",319,112,397,36,Gold,.85f).gameObject;toastLabel=Label(toastPanel.transform,"",10,2,377,32,18,Cream,TextAnchor.MiddleCenter);toastLabel.resizeTextForBestFit=true;toastLabel.resizeTextMinSize=13;toastLabel.resizeTextMaxSize=18;
            if(Application.isMobilePlatform||qaMode){
                ActionControl("LEFT","<",20,450,72,Cyan,null,v=>leftHeld=v);ActionControl("RIGHT",">",106,450,72,Cyan,null,v=>rightHeld=v);
                ActionControl("DUCK","v",194,466,56,Cyan,null,v=>{duckHeld=v;if(v&&playerBody&&Mathf.Abs(playerBody.linearVelocity.x)>.8f)Slide();});
                ActionControl("DASH",">>",682,466,56,Cyan,Dash);
                ActionControl("JUMP","^",844,430,94,Cyan,null,v=>jumpHeld=v);
                if(stats.HasWeapon)ActionControl("FIRE","*",752,452,70,Gold,null,v=>fireHeld=v);
            }
            controlHint=null;
            solveDock=Button(screenRoot,Application.isMobilePlatform||qaMode?"INTERACT":"INTERACT / E",405,421,150,36,new Color(.82f,.45f,1),OpenNearbyPuzzle).gameObject;solveDock.GetComponentInChildren<Text>().fontSize=18;
            BuildSupportHud();UpdateUi();
        }
        void UpdateUi(){
            if(graphicsLabel)graphicsLabel.text=RendererProgress.Progress(stats);
            if(canvas){var safe=Screen.safeArea;canvas.scaleFactor=Mathf.Min(safe.width/960f,safe.height/540f);uiRoot.anchoredPosition=(safe.center-new Vector2(Screen.width/2f,Screen.height/2f))/Mathf.Max(.01f,canvas.scaleFactor);}
            if(updateStatusLabel&&updater)updateStatusLabel.text=updater.Status;
            if(updateButtonLabel&&updater)updateButtonLabel.text=updater.ButtonText;
            if(mode!=ScreenMode.Play||!gpuLabel)return;UpdateSupportHud();
            if(xpHud)xpHud.text="LV "+stats.rank+" / XP "+stats.xp;if(xpFill)xpFill.rectTransform.sizeDelta=new Vector2(105*stats.xp/(float)stats.XpNeeded,4);
            gpuLabel.text=collected+"/"+totalGpus;var boss=enemies.FirstOrDefault(e=>(e.IsBoss||e.type==12)&&e.hp>0);if(bossHealthHud)bossHealthHud.text=boss!=null&&Mathf.Abs(boss.transform.position.x-playerBody.position.x)<16?EnemyAgent.Names[boss.type]+"  "+boss.hp+" / "+boss.maxHp+(boss.type==12?" / "+boss.CombatTell:""):"";pointsLabel.text=stats.points+" PTS";pointsLabel.fontSize=16;healthLabel.text=stats.health+"/"+stats.maxHealth+" HP";
            for(int i=0;i<10;i++)healthCells[i].color=i<Mathf.CeilToInt(stats.health/(float)stats.maxHealth*10)?Pink:new Color(.13f,.055f,.12f);
            if(miniPlayer)miniPlayer.rectTransform.anchoredPosition=new Vector2(60+playerBody.position.x/(worldLength/PPU)*572,-5);int count=0;
            foreach(var pickup in pickups.Where(p=>p.gpu)){if(count<miniGpus.Count)miniGpus[count].gameObject.SetActive(!pickup.taken);count++;}
            var nearest=pickups.Where(p=>p.gpu&&!p.taken).OrderBy(p=>Mathf.Abs(p.transform.position.x-playerBody.position.x)).FirstOrDefault();
            nextLabel.text=collected<RequiredGpus?"RECOVER "+Mathf.Max(0,RequiredGpus-collected)+" MORE":boss!=null?"DEFEAT UPLINK GUARD":"REACH THE UPLINK >";
            if(objectiveFill)objectiveFill.rectTransform.sizeDelta=new Vector2(306*Mathf.Clamp01(collected/(float)Mathf.Max(1,RequiredGpus)),4);
            string[] names={"SPEED+","SHIELD","MAGNET","RAPID","2X","PHASE","JUMP+"};var active=new List<string>();for(int i=0;i<buffs.Length;i++)if(buffs[i]>0)active.Add(names[i]+" "+(int)buffs[i]+"s");buffPanel.SetActive(active.Count>0);buffLabel.text=string.Join(" / ",active);
            toastPanel.SetActive(toastTime>0);toastLabel.text=toast;if(toastPanel){var fade=toastPanel.GetComponent<CanvasGroup>();if(!fade)fade=toastPanel.AddComponent<CanvasGroup>();fade.alpha=Mathf.Clamp01(toastTime/.35f);fade.blocksRaycasts=false;}solveDock.SetActive(NearbyTerminal()!=null);
            for(int i=0;i<gearLabels.Count;i++){gearLabels[i].text="T"+stats.gear[i];gearLabels[i].color=TierColor(stats.gear[i]);}
        }
        void BuildShop(RectTransform window){
            Label(window,"LV "+stats.rank+" / XP "+stats.xp+"/"+stats.XpNeeded+" / "+stats.talents+" TALENTS",30,84,815,32,23,Gold);
            string[] names={"SPEED","WEAPON TIER","ARMOR TIER","VITALITY","WEAPON MASTERY","DEFENSE TRAINING"};
            for(int i=0;i<6;i++){int slot=i;int value=i<4?stats.Level(i):i==4?stats.power:stats.defense;int cap=i<4?stats.Cap(i):20;
                var row=Panel(window,names[i],29,123+i*45,818,41,i==1||i==4?Gold:Cyan);
                Label(row,names[i]+" / LV "+value,14,3,495,34,23,Cream);
                int cost=i<4?stats.Cost(i):50+value*30;
                var buy=Button(row,i==1&&!stats.HasWeapon||i==2&&!stats.HasArmor?"FIND ITEM FIRST":value>=cap?"MAX":cost+" PTS"+(i>=4?" + TALENT":""),511,3,292,35,Gold,()=>{Toast(slot<4?stats.Upgrade(slot):stats.Train(slot==4));SaveProgress();BuildScreen();});
                buy.interactable=value<cap&&stats.points>=cost&&(i<4||stats.talents>0)&&(i!=1||stats.HasWeapon)&&(i!=2||stats.HasArmor);
            }
            Label(window,toastTime>0?toast:"XP: GPU +8 / PUZZLE +20 / KILL +12 / BOSS +80",30,398,805,30,20,Cyan);
            Label(window,"ULTRA 0.5% BOX ONLY / PROGRESS AUTO-SAVES",30,433,490,27,19,Gold);
            Button(window,"BACK TO RUN",560,432,287,35,Cyan,()=>SetMode(ScreenMode.Play));
        }
        void BuildGear(RectTransform window){
            stats.EnsureCollection();int slot=armorCollection?2:1;var found=stats.inventory.Where(i=>i.slot==slot).OrderByDescending(i=>i.tier).ThenBy(i=>i.id).ToList();int pages=Math.Max(1,(found.Count+3)/4);inventoryPage=Mathf.Clamp(inventoryPage,0,pages-1);
            Button(window,"WEAPONS / "+stats.WeaponCount,30,94,221,42,armorCollection?Cyan:Gold,()=>{armorCollection=false;inventoryPage=0;BuildScreen();});Button(window,"ARMOR / "+stats.SuitCount,267,94,221,42,armorCollection?Gold:Cyan,()=>{armorCollection=true;inventoryPage=0;BuildScreen();});
            Button(window,"CHARACTER",655,94,192,42,Cyan,()=>{characterReturn=ScreenMode.Gear;SetMode(ScreenMode.Characters);});
            if(found.Count==0){Label(window,armorCollection?"NO ARMOR FOUND":"NO WEAPONS FOUND",40,196,790,40,29,Cream,TextAnchor.MiddleCenter);Label(window,"SEARCH THE CITY. OPEN SUPPLY BOXES. KEEP WHAT YOU FIND.",40,248,790,45,20,Cyan,TextAnchor.MiddleCenter);}
            for(int cell=0;cell<4;cell++){int at=inventoryPage*4+cell;if(at>=found.Count)break;var item=found[at];float x=30+cell%2*416,y=150+cell/2*113;var card=Panel(window,item.Name,x,y,400,103,TierColor(item.tier));
                Image(card,"Found equipment",10,18,58,58,Color.white,EquipmentIcon(slot,item.family)).preserveAspect=true;
                Label(card,item.Name,82,7,300,28,22,Cream);Label(card,RunStats.Tiers[item.tier]+" / TIER "+item.tier,82,35,299,22,17,TierColor(item.tier));
                string detail=slot==1?new[]{"BALANCED FIRE","BREAKS SHIELDS","SPREAD / SHORT RANGE","PIERCES ARMOR"}[item.family]:new[]{"BALANCED PROTECTION","RESISTS CONTACT","RESISTS ENERGY","MOBILITY / LESS DEFENSE"}[item.family];Label(card,detail,13,69,228,24,14,Cyan);
                bool active=(slot==1?stats.weaponItemId:stats.armorItemId)==item.id;var equip=Button(card,active?"EQUIPPED":"EQUIP",252,65,135,29,active?Gold:Cyan,()=>{stats.EquipItem(item.id);SaveProgress();BuildScreen();});equip.GetComponentInChildren<Text>().fontSize=17;equip.interactable=!active;
            }
            if(pages>1){Button(window,"<",30,385,49,33,Cyan,()=>{inventoryPage--;BuildScreen();}).interactable=inventoryPage>0;Label(window,(inventoryPage+1)+" / "+pages,88,385,100,33,18,Cream);Button(window,">",194,385,49,33,Cyan,()=>{inventoryPage++;BuildScreen();}).interactable=inventoryPage<pages-1;}
            Label(window,"FOUND ITEMS ONLY. EACH COPY KEEPS ITS OWN TIER.",30,430,565,34,17,Cyan);Button(window,"BACK TO RUN",615,431,232,37,Cyan,()=>SetMode(ScreenMode.Play));
        }
        void BuildCharacters(RectTransform window){
            stats.EnsureCollection();cosmeticPopup=null;float step=318f/RunStats.ActiveCharacters.Length;
            for(int row=0;row<RunStats.ActiveCharacters.Length;row++){
                int choice=RunStats.ActiveCharacters[row];bool unlocked=stats.CharacterUnlocked(choice),active=stats.character==choice;
                var select=Button(window,"",28,103+row*step,260,step-3,active?Gold:Cyan,()=>{stats.character=choice;ApplyCharacter();SaveProgress();BuildScreen();});
                Image(select.transform,"Character silhouette or portrait",5,3,44,44,unlocked?Color.white:Color.black,CharacterFrame(choice,0)).preserveAspect=true;
                Label(select.transform,RunStats.Characters[choice],57,1,195,25,20,unlocked?Cream:new Color(.5f,.6f,.65f));
                string requirement=choice==4?"FREE AI WRATH":choice==5?"24 DISTINCT GPUS":choice==6?"CLEAR STAGE 6":choice==8?"CLEAR STAGE 12":"SECRET CODE";
                Label(select.transform,unlocked?(active?"ACTIVE":"SWITCH CHARACTER"):requirement,57,29,195,18,12,active?Gold:Cyan);
                select.interactable=unlocked&&!active;
            }
            var preview=Panel(window,"Equipped loadout",305,102,543,316,Gold,.8f);
            Label(preview,"ACTIVE / "+RunStats.Characters[stats.character],18,8,507,31,26,Gold);
            Image(preview,"Selected character",12,43,218,211,Color.white,playerFrames[0]).preserveAspect=true;
            for(int i=0;i<3;i++){
                int slot=i==0?1:i==1?2:0;int family=slot==1?stats.weapon:slot==2?stats.suit:0;int tier=stats.gear[slot];float y=49+i*62;
                if(tier>0||slot==0)Image(preview,"Equipped item",254,y+1,35,35,Color.white,EquipmentIcon(slot,family)).preserveAspect=true;else Label(preview,"--",254,y+1,35,35,18,Cyan,TextAnchor.MiddleCenter);
                Label(preview,slot==1?(stats.HasWeapon?RunStats.Weapons[family]:"UNARMED"):slot==2?(stats.HasArmor?RunStats.Suits[family]:"NO ARMOR"):"BOOTS",301,y,224,24,19,Cream);
                Label(preview,tier==0?(slot==0?"WORN SHOES":"FIND IN CITY"):RunStats.Tiers[tier],301,y+27,224,19,15,TierColor(tier));
            }
            string outfit=stats.character==4&&wonderWrathUnlocked&&wonderWrathEquipped?"WONDER WRATH":RunStats.Outfits[stats.outfit];
            cosmeticTrigger=Button(preview,"COSMETIC / "+outfit+"  v",18,264,248,39,Gold,()=>OpenCosmeticDropdown(preview));cosmeticTrigger.GetComponentInChildren<Text>().fontSize=16;
            Button(preview,"CHANGE GEAR",286,264,240,39,Cyan,()=>SetMode(ScreenMode.Gear)).GetComponentInChildren<Text>().fontSize=19;
            Label(window,"ALL ALLIES SHARE YOUR INVENTORY AND PROGRESS.",30,435,582,29,17,Cyan);Button(window,"BACK",645,432,202,37,Cyan,()=>SetMode(characterReturn));
        }
        void BuildPuzzle(RectTransform window){
            if(activeTerminal==null)return;if(activeTerminal.data.puzzle.circuit){BuildCircuit(window);return;}
            var question=Panel(window,"Logic question",30,102,568,110,Cyan);var text=Label(question,activeTerminal.data.puzzle.question,20,10,528,90,26,Cream);text.resizeTextForBestFit=true;text.resizeTextMinSize=21;text.resizeTextMaxSize=26;
            for(int i=0;i<3;i++){int choice=i;Button(window,(i+1)+".  "+activeTerminal.data.puzzle.options[i],40,231+i*62,548,52,i==0?Gold:Cyan,()=>AnswerPuzzle(choice));}
            var art=Panel(window,"Terminal console",621,108,225,288,Cyan,.7f);Image(art,"Terminal art",34,49,158,158,Color.white,itemFrames[2]);Label(art,"ONE ATTEMPT",15,213,195,36,26,Gold,TextAnchor.MiddleCenter);Label(art,"NO REROLLS",15,248,195,26,20,Cyan,TextAnchor.MiddleCenter);
            Label(window,"CORRECT = POWERUP / WRONG = NO REWARD",32,434,799,27,20,Cyan);
        }
        void BuildPause(RectTransform window){
            Label(window,CityRoute.Name(level),40,95,795,38,29,Cyan,TextAnchor.MiddleCenter);
            var texture=Resources.Load<Texture2D>("Art/background_"+new[]{"settlement","datafarm","citadel"}[level%3]);var image=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),100);Image(window,"District overview",65,143,746,66,Color.white,image);
            Label(window,RunStats.Characters[stats.character]+" / XP "+stats.xp+"/"+stats.XpNeeded+" / "+RendererProgress.Progress(stats),65,215,746,22,17,Gold,TextAnchor.MiddleCenter);
            string guide=controllerInUse?"STICK MOVE / A JUMP / X FIRE / B SLIDE / RB DASH\nY INTERACT / LB AI HELP / VIEW INVENTORY":Application.isMobilePlatform||qaMode?"ARROWS MOVE / HOLD DOWN TO DUCK / MOVING DOWN SLIDES\nJUMP / FIRE WITH FOUND WEAPON / DASH / EARN AI HELP":"A/D MOVE / SPACE JUMP / S DUCK / CTRL SLIDE / J FIRE / SHIFT DASH\nE INTERACT / R AI HELP / I INVENTORY / C CHARACTER / U UPGRADES";
            Label(window,guide,44,242,788,38,14,Cyan,TextAnchor.MiddleCenter);
            Button(window,"RESUME",263,287,350,48,Gold,()=>SetMode(ScreenMode.Play));Button(window,"INVENTORY",46,346,247,48,Cyan,()=>SetMode(ScreenMode.Gear));Button(window,"CHARACTER",314,346,247,48,Cyan,()=>{characterReturn=ScreenMode.Pause;SetMode(ScreenMode.Characters);});Button(window,"UPGRADES",582,346,247,48,Cyan,()=>SetMode(ScreenMode.Shop));
            Button(window,"AI TYPE / "+SupportNames[supportKind]+" >",46,411,247,47,Cyan,()=>{CycleSupport();BuildScreen();}).GetComponentInChildren<Text>().fontSize=17;
            Button(window,"CITY MAP",314,411,247,47,Gold,()=>SetMode(ScreenMode.CityMap));Button(window,"SETTINGS",582,411,247,47,Cyan,()=>{settingsReturn=ScreenMode.Pause;SetMode(ScreenMode.Settings);});
        }
        void BuildSettings(RectTransform window){
            Button(window,"SOUND / "+(soundOn?"ON":"OFF"),199,104,478,48,Cyan,()=>{soundOn=!soundOn;PlayerPrefs.SetInt("sound",soundOn?1:0);BuildScreen();});
            Button(window,"HAPTICS / "+(haptics?"ON":"OFF"),199,165,478,48,Cyan,()=>{haptics=!haptics;PlayerPrefs.SetInt("haptics",haptics?1:0);BuildScreen();});
            #if UNITY_WEBGL && !UNITY_EDITOR
            Label(window,"WEB EDITION / "+Application.version+"\nProgress saves in this browser. Reload for new releases.",75,227,726,103,22,Cyan,TextAnchor.MiddleCenter);
#else
            var update=Button(window,updater.ButtonText,199,227,478,48,Gold,()=>updater.Begin());updateButtonLabel=update.GetComponentInChildren<Text>();
            updateStatusLabel=Label(window,updater.Status,75,283,726,58,22,Cyan,TextAnchor.MiddleCenter);
            #endif
            Button(window,"DIFFICULTY / "+DifficultyName,199,343,478,42,Cyan,CycleDifficulty);Label(window,"ADAPTIVE: PERFORMANCE CHANGES THE NEXT STAGE",75,389,726,20,16,Cyan,TextAnchor.MiddleCenter);
            Button(window,"BACK TO TITLE",263,411,350,47,Gold,()=>SetMode(ScreenMode.Title));
        }
        void BuildEnd(RectTransform window){
            Color accent=mode==ScreenMode.Dead?Pink:Gold;Label(window,mode==ScreenMode.Clear?CityRoute.Name(level)+" / COMPLETE":mode==ScreenMode.Win?"THE PUBLIC AI BELONGS TO EVERYONE.":"THE CITY STILL NEEDS YOU.",38,102,800,55,28,Cyan,TextAnchor.MiddleCenter);
            string[] labels={"GPUs","SCORE","DISTRICT"},values={collected+"/"+totalGpus,stats.score.ToString("N0"),(level+1).ToString()};
            for(int i=0;i<3;i++){var card=Panel(window,labels[i],56+i*263,181,235,116,accent);Image(card,"Stat icon",13,25,56,56,Color.white,itemFrames[i==0?0:i==1?7:3]);Label(card,labels[i],79,14,139,34,22,Cyan);Label(card,values[i],79,50,139,47,34,Cream);}
            Label(window,mode==ScreenMode.Clear?"ROUTE CLEARED / UPGRADES SAVED / +2 HEALTH":mode==ScreenMode.Win?"REALITY RESTORED. THE SINISTER ACCESS LOCK IS BROKEN.":"PULSE TIER "+stats.gear[1]+" / ARMOR TIER "+stats.gear[2],35,328,806,46,24,accent,TextAnchor.MiddleCenter);
            Button(window,mode==ScreenMode.Clear?"CITY MAP":mode==ScreenMode.Win?"CONTINUE / NEXT CITY CYCLE":"RETRY / CITY MAP",231,401,414,60,accent,()=>{if(mode==ScreenMode.Clear||mode==ScreenMode.Win)NextLevel();else StartRun();});
        }
    }
}

