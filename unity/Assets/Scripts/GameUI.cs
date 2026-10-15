using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ArtificialNegligence {
    public partial class GameController {
        Canvas canvas;RectTransform uiRoot,screenRoot;Font pixelFont;Sprite frameSprite,uiWhite,roundedFill;
        Image xpFill;Text xpHud;bool armorCollection;ScreenMode characterReturn=ScreenMode.CityMap;
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
            var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1.4f,1.4f,1.4f,1);colors.pressedColor=new Color(2,2,2,1);colors.disabledColor=new Color(.02f,.04f,.05f,.8f);button.colors=colors;
            Label(rt,text,8,4,w-16,h-8,26,accent,TextAnchor.MiddleCenter);if(clicked!=null)button.onClick.AddListener(()=>clicked());return button;
        }
        void Hold(Transform parent,string text,float x,float y,float w,float h,Color accent,Action<bool> changed){var b=Button(parent,text,x,y,w,h,accent,null);b.gameObject.AddComponent<HoldControl>().changed=changed;}
        void Pips(Transform parent,int value,int cap,float x,float y,Color color){for(int i=0;i<cap;i++)Image(parent,"Tier cell",x+i*24,y,18,8,i<value?color:new Color(.09f,.26f,.29f));}
        void BuildScreen(){
            if(screenRoot){screenRoot.gameObject.SetActive(false);Destroy(screenRoot.gameObject);}screenRoot=Rect(uiRoot,"Screen / "+mode,0,0,960,540);
            gpuLabel=pointsLabel=healthLabel=nextLabel=buffLabel=toastLabel=null;toastPanel=solveDock=buffPanel=null;miniGpus.Clear();gearLabels.Clear();gearColors.Clear();
            updateStatusLabel=updateButtonLabel=bossHealthHud=graphicsLabel=null;
            if(mode==ScreenMode.Play){BuildHud();return;}
            if(mode==ScreenMode.Title){BuildTitle();return;}if(mode==ScreenMode.CityMap){BuildCityMap();return;}
            Image(screenRoot,"Dim world",0,0,960,540,new Color(.001f,.01f,.02f,.75f));
            string title=mode==ScreenMode.PuzzleResult?"TERMINAL RESULT":mode==ScreenMode.Shop?"UPGRADE STATION":mode==ScreenMode.Characters?"SURVIVOR CUSTOMIZATION":mode==ScreenMode.Gear?"ITEM COLLECTION":mode==ScreenMode.Puzzle?"INTELLECT TERMINAL":mode==ScreenMode.Pause?"RUN PAUSED":mode==ScreenMode.Clear?"DISTRICT LIBERATED":mode==ScreenMode.Win?"AI LIBERATED":mode==ScreenMode.Dead?"RUN TERMINATED":"SETTINGS";
            Color accent=mode==ScreenMode.Dead?Pink:Cyan;var window=Panel(screenRoot,title,42,30,876,480,accent,.91f);
            Label(window,title,28,19,710,45,36,Cream);Image(window,"Header track",26,77,821,2,accent);
            if(mode!=ScreenMode.Win&&mode!=ScreenMode.Dead&&mode!=ScreenMode.Clear)Button(window,"X",807,15,45,44,accent,()=>SetMode(mode==ScreenMode.Characters?characterReturn:mode==ScreenMode.Settings?ScreenMode.Title:ScreenMode.Play));
            if(mode==ScreenMode.Characters)BuildCharacters(window);else if(mode==ScreenMode.PuzzleResult)BuildPuzzleResult(window);else if(mode==ScreenMode.Shop)BuildShop(window);else if(mode==ScreenMode.Gear)BuildGear(window);else if(mode==ScreenMode.Puzzle)BuildPuzzle(window);else if(mode==ScreenMode.Pause)BuildPause(window);else if(mode==ScreenMode.Settings)BuildSettings(window);else BuildEnd(window);
        }
        void BuildTitle(){
            Image(screenRoot,"Tint",0,0,960,540,new Color(.001f,.01f,.02f,1));var frame=Panel(screenRoot,"Campaign title",14,14,932,512,Cyan,.35f);
            var logoTexture=Resources.Load<Texture2D>("Art/Logo");var logo=Sprite.Create(logoTexture,new Rect(0,0,logoTexture.width,logoTexture.height),new Vector2(.5f,.5f),100);
            Image(frame,"Artificial Negligence logo",28,50,390,390,Color.white,logo);
            Label(frame,"ABSORB THE AI.",459,78,422,53,42,Cream);Label(frame,"KEEP IT PUBLIC.",459,125,422,53,42,Gold);
            Button(frame,PlayerPrefs.HasKey("progress-v2")?"CONTINUE":"START GAME",486,221,350,58,Gold,StartRun);Button(frame,"SETTINGS",486,296,350,48,Cyan,()=>SetMode(ScreenMode.Settings));Button(frame,"QUIT",486,361,350,44,Cyan,()=>Application.Quit());
            Label(frame,"v"+Application.version,789,470,115,24,18,Cyan,TextAnchor.MiddleRight);
        }
        void BuildHud(){
            // One compact status card; secondary progression lives in pause/collection.
            var status=Panel(screenRoot,"Survivor status",12,10,352,60,Cyan,.77f);
            Image(status,"GPU icon",12,9,26,26,Color.white,itemFrames[0]);gpuLabel=Label(status,"",45,6,91,27,24,Cream);
            pointsLabel=Label(status,"",145,8,192,25,20,Gold);pointsLabel.resizeTextForBestFit=true;pointsLabel.resizeTextMinSize=14;pointsLabel.resizeTextMaxSize=20;
            healthLabel=Label(status,"",12,35,88,20,17,Pink);healthCells=new Image[10];for(int i=0;i<10;i++)healthCells[i]=Image(status,"Health segment",103+i*22,41,17,6,Pink);
            nextLabel=Label(screenRoot,"",383,15,232,25,18,Gold);bossHealthHud=Label(screenRoot,"",383,43,385,25,18,Pink);
            Button(screenRoot,"II",892,12,54,48,Cyan,Pause);
            // Buffs appear only while active, with no empty panel or navigation strip.
            buffPanel=Rect(screenRoot,"Active boosts",22,75,340,24).gameObject;buffLabel=Label(buffPanel.transform,"",0,0,340,24,18,Cyan);buffLabel.resizeTextForBestFit=true;buffLabel.resizeTextMinSize=13;buffLabel.resizeTextMaxSize=18;
            toastPanel=Panel(screenRoot,"Pickup notice",383,74,365,39,Gold,.75f).gameObject;toastLabel=Label(toastPanel.transform,"",12,2,341,35,20,Cream,TextAnchor.MiddleCenter);toastLabel.resizeTextForBestFit=true;toastLabel.resizeTextMinSize=15;toastLabel.resizeTextMaxSize=21;
            // Independent, translucent thumb controls expose the center of the scene.
            Hold(screenRoot,"<",16,454,74,70,Cyan,v=>leftHeld=v);Hold(screenRoot,">",102,454,74,70,Cyan,v=>rightHeld=v);
            Button(screenRoot,"DASH",641,465,83,59,Cyan,Dash);Hold(screenRoot,"JUMP",739,446,94,78,Cyan,v=>jumpHeld=v);Hold(screenRoot,"FIRE",848,446,98,78,Gold,v=>fireHeld=v);
            foreach(var button in screenRoot.GetComponentsInChildren<Button>()){
                var fill=button.transform.GetChild(0).GetComponent<Image>();var color=fill.color;color.a=.58f;fill.color=color;
            }
            solveDock=Button(screenRoot,"SOLVE TERMINAL",354,426,252,46,new Color(.82f,.45f,1),OpenNearbyPuzzle).gameObject;UpdateUi();
        }
        void UpdateUi(){
            if(graphicsLabel)graphicsLabel.text=RendererProgress.Progress(stats);
            if(canvas){var safe=Screen.safeArea;canvas.scaleFactor=Mathf.Min(safe.width/960f,safe.height/540f);uiRoot.anchoredPosition=(safe.center-new Vector2(Screen.width/2f,Screen.height/2f))/Mathf.Max(.01f,canvas.scaleFactor);}
            if(updateStatusLabel&&updater)updateStatusLabel.text=updater.Status;
            if(updateButtonLabel&&updater)updateButtonLabel.text=updater.ButtonText;
            if(mode!=ScreenMode.Play||!gpuLabel)return;
            if(xpHud)xpHud.text="XP "+stats.xp+" / "+stats.XpNeeded;if(xpFill)xpFill.rectTransform.sizeDelta=new Vector2(105*stats.xp/(float)stats.XpNeeded,4);
            gpuLabel.text=collected+"/"+totalGpus;var boss=enemies.FirstOrDefault(e=>e.IsBoss&&e.hp>0);if(bossHealthHud)bossHealthHud.text=boss!=null&&Mathf.Abs(boss.transform.position.x-playerBody.position.x)<16?EnemyAgent.Names[boss.type]+"  "+boss.hp+" / "+boss.maxHp:"";pointsLabel.text=stats.points+" PTS / LV "+stats.rank;pointsLabel.fontSize=20;healthLabel.text=stats.health+"/"+stats.maxHealth+" HP";
            for(int i=0;i<10;i++)healthCells[i].color=i<Mathf.CeilToInt(stats.health/(float)stats.maxHealth*10)?Pink:new Color(.13f,.055f,.12f);
            if(miniPlayer)miniPlayer.rectTransform.anchoredPosition=new Vector2(60+playerBody.position.x/(worldLength/PPU)*572,-5);int count=0;
            foreach(var pickup in pickups.Where(p=>p.gpu)){if(count<miniGpus.Count)miniGpus[count].gameObject.SetActive(!pickup.taken);count++;}
            var nearest=pickups.Where(p=>p.gpu&&!p.taken).OrderBy(p=>Mathf.Abs(p.transform.position.x-playerBody.position.x)).FirstOrDefault();
            nextLabel.text=nearest==null?(boss!=null?EnemyAgent.Names[boss.type]+" "+boss.hp+" HP":"UPLINK >"):"GPU "+(nearest.transform.position.x<playerBody.position.x?"< ":"> ")+(int)(Mathf.Abs(nearest.transform.position.x-playerBody.position.x)*PPU);
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
                var buy=Button(row,value>=cap?"MAX":cost+" PTS"+(i>=4?" + TALENT":""),511,3,292,35,Gold,()=>{Toast(slot<4?stats.Upgrade(slot):stats.Train(slot==4));SaveProgress();BuildScreen();});
                buy.interactable=value<cap&&stats.points>=cost&&(i<4||stats.talents>0);
            }
            Label(window,toastTime>0?toast:"XP: GPU +8 / PUZZLE +20 / KILL +12 / BOSS +80",30,398,805,30,20,Cyan);
            Label(window,"ULTRA 0.5% BOX ONLY / PROGRESS AUTO-SAVES",30,433,490,27,19,Gold);
            Button(window,"BACK TO RUN",560,432,287,35,Cyan,()=>SetMode(ScreenMode.Play));
        }
        void BuildGear(RectTransform window){
            stats.EnsureCollection();
            Button(window,"WEAPONS",30,94,180,42,armorCollection?Cyan:Gold,()=>{armorCollection=false;BuildScreen();});Button(window,"ARMOR",224,94,180,42,armorCollection?Gold:Cyan,()=>{armorCollection=true;BuildScreen();});
            Button(window,"SURVIVOR",655,94,192,42,Cyan,()=>{characterReturn=ScreenMode.Gear;SetMode(ScreenMode.Characters);});
            int slot=armorCollection?2:1;var owned=armorCollection?stats.armorRarities:stats.weaponRarities;var names=armorCollection?RunStats.Suits:RunStats.Weapons;
            string[] descriptions=armorCollection?new[]{"Balanced protection","Contact resistance / energy weakness","Energy resistance / contact weakness","Fast movement / lower protection"}:new[]{"Reliable balanced fire","Shield breaker / weak vs armor","Wide spread / short range","Armor pierce / weak vs shields"};
            for(int i=0;i<4;i++){int family=i;int tier=owned[i];float x=30+(i%2)*416,y=150+(i/2)*113;var card=Panel(window,names[i],x,y,400,103,tier==0?new Color(.23f,.32f,.36f):TierColor(tier));
                Image(card,"Item",12,19,62,62,Color.white,EquipmentIcon(slot,i));Label(card,names[i],87,7,292,29,26,Cream);Label(card,tier==0?"LOCKED / FIND IN BOX OR REACH LV "+(i*2+1):RunStats.Tiers[tier]+" / TIER "+tier,87,36,294,23,17,tier==0?Cyan:TierColor(tier));
                bool selected=armorCollection?stats.suit==i:stats.weapon==i;var equip=Button(card,tier==0?"LOCKED":selected?"EQUIPPED":"EQUIP",252,66,135,29,selected?Gold:Cyan,()=>{stats.SelectEquipment(slot,family);SaveProgress();BuildScreen();});equip.GetComponentInChildren<Text>().fontSize=18;equip.interactable=tier>0&&!selected;
                Label(card,descriptions[i],13,65,226,31,14,Cyan);
            }
            Button(window,"MODULE: "+RunStats.Modules[stats.module],30,389,382,39,Cyan,()=>{stats.module=(stats.module+1)%stats.ModuleCount;SaveProgress();BuildScreen();}).GetComponentInChildren<Text>().fontSize=22;
            Label(window,"BOOTS "+RunStats.Tiers[stats.gear[0]]+" / ULTRA: 0.5% IN BOXES",432,386,413,42,19,Gold);
            Label(window,"EACH ITEM KEEPS ITS OWN RARITY. DUPLICATES = POINTS.",30,436,574,26,17,Cyan);Button(window,"BACK TO RUN",615,431,232,37,Cyan,()=>SetMode(ScreenMode.Play));
        }
        void BuildCharacters(RectTransform window){
            stats.EnsureCollection();
            for(int i=0;i<4;i++){int choice=i;bool unlocked=stats.CharacterUnlocked(i);var card=Panel(window,"Survivor "+i,30+i*208,98,193,222,stats.character==i?Gold:Cyan);
                Image(card,"Character",48,12,96,96,unlocked?Color.white:new Color(.3f,.4f,.43f),CharacterFrame(i,0));Label(card,RunStats.Characters[i],10,108,173,35,26,Cream,TextAnchor.MiddleCenter);
                Label(card,unlocked?RunStats.CharacterDescriptions[i]:i==1?"COLLECT 24 GPUS":i==2?"CLEAR STAGE 6":"CLEAR STAGE 12",10,144,173,35,17,Cyan,TextAnchor.MiddleCenter);
                var button=Button(card,unlocked?(stats.character==i?"SELECTED":"SELECT"):"LOCKED",15,183,163,31,stats.character==i?Gold:Cyan,()=>{stats.character=choice;ApplyCharacter();SaveProgress();BuildScreen();});button.GetComponentInChildren<Text>().fontSize=20;button.interactable=unlocked&&stats.character!=i;
            }
            Label(window,"THREAD COLOR / COSMETIC ONLY",31,328,780,30,23,Cream);
            string[] conditions={"STARTER","LEVEL 3","75 GPUS","12 STAGES"};
            for(int i=0;i<4;i++){int choice=i;bool open=stats.OutfitUnlocked(i);var b=Button(window,RunStats.Outfits[i]+"\n"+(open?stats.outfit==i?"EQUIPPED":"UNLOCKED":conditions[i]),30+i*208,364,193,55,stats.outfit==i?Gold:Cyan,()=>{stats.outfit=choice;ApplyCharacter();SaveProgress();BuildScreen();});b.GetComponentInChildren<Text>().fontSize=18;b.interactable=open&&stats.outfit!=i;}
            Label(window,"ALL SURVIVORS SHARE YOUR SAVED GEAR AND PROGRESS.",30,435,582,29,18,Cyan);Button(window,"BACK",645,432,202,37,Cyan,()=>SetMode(characterReturn));
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
            var texture=Resources.Load<Texture2D>("Art/background_"+new[]{"settlement","datafarm","citadel"}[level%3]);var image=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),100);Image(window,"District overview",65,145,746,115,Color.white,image);
            Label(window,RunStats.Characters[stats.character]+" / XP "+stats.xp+"/"+stats.XpNeeded+" / "+RendererProgress.Progress(stats),65,258,746,22,17,Gold,TextAnchor.MiddleCenter);
            Button(window,"RESUME",263,287,350,48,Gold,()=>SetMode(ScreenMode.Play));Button(window,"GEAR",100,346,315,48,Cyan,()=>SetMode(ScreenMode.Gear));Button(window,"UPGRADES",460,346,315,48,Cyan,()=>SetMode(ScreenMode.Shop));Button(window,"CITY MAP",263,411,350,47,Gold,()=>SetMode(ScreenMode.CityMap));
        }
        void BuildSettings(RectTransform window){
            Button(window,"SOUND / "+(soundOn?"ON":"OFF"),199,104,478,48,Cyan,()=>{soundOn=!soundOn;PlayerPrefs.SetInt("sound",soundOn?1:0);BuildScreen();});
            Button(window,"HAPTICS / "+(haptics?"ON":"OFF"),199,165,478,48,Cyan,()=>{haptics=!haptics;PlayerPrefs.SetInt("haptics",haptics?1:0);BuildScreen();});
            var update=Button(window,updater.ButtonText,199,227,478,48,Gold,()=>updater.Begin());updateButtonLabel=update.GetComponentInChildren<Text>();
            updateStatusLabel=Label(window,updater.Status,75,283,726,58,22,Cyan,TextAnchor.MiddleCenter);
            Label(window,"TOUCH: MOVE / JUMP / PULSE / SOLVE",75,347,726,30,24,Cream,TextAnchor.MiddleCenter);Label(window,"KEYBOARD: A/D / SPACE / J / E / U / G",75,380,726,26,22,Cyan,TextAnchor.MiddleCenter);
            Button(window,"BACK TO TITLE",263,411,350,47,Gold,()=>SetMode(ScreenMode.Title));
        }
        void BuildEnd(RectTransform window){
            Color accent=mode==ScreenMode.Dead?Pink:Gold;Label(window,mode==ScreenMode.Clear?CityRoute.Name(level)+" / COMPLETE":mode==ScreenMode.Win?"THE PUBLIC AI BELONGS TO EVERYONE.":"THE CITY STILL NEEDS YOU.",38,102,800,55,28,Cyan,TextAnchor.MiddleCenter);
            string[] labels={"GPUs","SCORE","DISTRICT"},values={collected+"/"+totalGpus,stats.score.ToString("N0"),(level+1).ToString()};
            for(int i=0;i<3;i++){var card=Panel(window,labels[i],56+i*263,181,235,116,accent);Image(card,"Stat icon",13,25,56,56,Color.white,itemFrames[i==0?0:i==1?7:3]);Label(card,labels[i],79,14,139,34,22,Cyan);Label(card,values[i],79,50,139,47,34,Cream);}
            Label(window,mode==ScreenMode.Clear?"ROUTE CLEARED / UPGRADES SAVED / +2 HEALTH":"PULSE TIER "+stats.gear[1]+" / ARMOR TIER "+stats.gear[2],35,328,806,46,24,accent,TextAnchor.MiddleCenter);
            Button(window,mode==ScreenMode.Clear?"CITY MAP":"RETRY / CITY MAP",231,401,414,60,accent,()=>{if(mode==ScreenMode.Clear)NextLevel();else StartRun();});
        }
    }
}

