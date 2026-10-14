using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ArtificialNegligence {
    public partial class GameController {
        Canvas canvas;RectTransform uiRoot,screenRoot;Font pixelFont;Sprite frameSprite,uiWhite;
        Text gpuLabel,pointsLabel,healthLabel,nextLabel,buffLabel,toastLabel;GameObject toastPanel,solveDock,buffPanel;
        Text updateStatusLabel,updateButtonLabel;
        Image[] healthCells;Image miniPlayer;readonly List<Image> miniGpus=new List<Image>();
        readonly List<Text> gearLabels=new List<Text>();readonly List<Image> gearColors=new List<Image>();
        public static Color TierColor(int tier){return tier==5?Gold:tier==4?new Color(1,.57f,.15f):tier==3?new Color(.87f,.35f,1):tier==2?Cyan:new Color(.57f,.84f,.65f);}
        void BuildCanvas(){
            pixelFont=Resources.Load<Font>("Fonts/Pixel");if(!pixelFont)pixelFont=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var go=new GameObject("Neon UI",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;
            go.GetComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
            uiRoot=new GameObject("16x9 Safe Area",typeof(RectTransform)).GetComponent<RectTransform>();uiRoot.SetParent(go.transform,false);uiRoot.anchorMin=uiRoot.anchorMax=new Vector2(.5f,.5f);uiRoot.pivot=new Vector2(.5f,.5f);uiRoot.sizeDelta=new Vector2(960,540);
            new GameObject("Event System",typeof(EventSystem),typeof(StandaloneInputModule));
            uiWhite=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),new Vector2(.5f,.5f));frameSprite=CreateFrame();
        }
        Sprite CreateFrame(){
            const int size=64;var t=new Texture2D(size,size,TextureFormat.RGBA32,false);t.filterMode=FilterMode.Point;var colors=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++){
                int edge=Mathf.Min(Mathf.Min(x,y),Mathf.Min(size-1-x,size-1-y));Color c=Color.clear;
                if(edge==0)c=new Color(.2f,1,1,.15f);if(edge==1)c=new Color(.5f,1,1,.35f);if(edge==2)c=new Color(1,1,1,1);if(edge==3)c=new Color(.5f,1,1,.3f);if(edge==5)c=new Color(.25f,.65f,.75f,.4f);
                if((x<8&&y<8)||(x>55&&y>55)) {if(edge<2)c=Color.clear;if(x==7||y==7)c=Color.white;}
                colors[y*size+x]=c;
            }
            t.SetPixels(colors);t.Apply();return Sprite.Create(t,new Rect(0,0,size,size),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(9,9,9,9));
        }
        RectTransform Rect(Transform parent,string name,float x,float y,float w,float h){var rt=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rt.SetParent(parent,false);rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(x,-y);rt.sizeDelta=new Vector2(w,h);return rt;}
        Image Image(Transform parent,string name,float x,float y,float w,float h,Color color,Sprite sprite=null){var rt=Rect(parent,name,x,y,w,h);var img=rt.gameObject.AddComponent<Image>();img.sprite=sprite?sprite:uiWhite;img.color=color;img.raycastTarget=false;return img;}
        Text Label(Transform parent,string text,float x,float y,float w,float h,int size,Color color,TextAnchor align=TextAnchor.MiddleLeft){var rt=Rect(parent,"Text / "+text,x,y,w,h);var label=rt.gameObject.AddComponent<Text>();label.font=pixelFont;label.text=text;label.fontSize=size;label.color=color;label.alignment=align;label.raycastTarget=false;label.horizontalOverflow=HorizontalWrapMode.Wrap;label.verticalOverflow=VerticalWrapMode.Truncate;return label;}
        RectTransform Panel(Transform parent,string name,float x,float y,float w,float h,Color accent,float alpha=.96f){var rt=Rect(parent,name,x,y,w,h);Image(rt,"Dark glass",0,0,w,h,new Color(.003f,.045f,.062f,alpha));var frame=Image(rt,"Neon frame",0,0,w,h,accent,frameSprite);frame.type=UnityEngine.UI.Image.Type.Sliced;
            Image(rt,"Gold node",15,3,8,4,Gold);Image(rt,"Cyan track",25,3,Mathf.Min(w*.25f,100),2,accent);Image(rt,"Gold node",w-20,h-6,7,4,Gold);return rt;
        }
        Button Button(Transform parent,string text,float x,float y,float w,float h,Color accent,Action clicked){var rt=Panel(parent,"Button / "+text,x,y,w,h,accent);var background=rt.GetChild(0).GetComponent<Image>();background.raycastTarget=true;var button=rt.gameObject.AddComponent<Button>();button.targetGraphic=background;
            var colors=button.colors;colors.normalColor=new Color(.005f,.05f,.07f,.97f);colors.highlightedColor=new Color(.02f,.14f,.17f,.98f);colors.pressedColor=new Color(.04f,.28f,.32f,1);colors.disabledColor=new Color(.02f,.04f,.05f,.8f);button.colors=colors;
            Label(rt,text,8,4,w-16,h-8,26,accent,TextAnchor.MiddleCenter);if(clicked!=null)button.onClick.AddListener(()=>clicked());return button;
        }
        void Hold(Transform parent,string text,float x,float y,float w,float h,Color accent,Action<bool> changed){var b=Button(parent,text,x,y,w,h,accent,null);b.gameObject.AddComponent<HoldControl>().changed=changed;}
        void Pips(Transform parent,int value,int cap,float x,float y,Color color){for(int i=0;i<cap;i++)Image(parent,"Tier cell",x+i*24,y,18,8,i<value?color:new Color(.09f,.26f,.29f));}
        void BuildScreen(){
            if(screenRoot){screenRoot.gameObject.SetActive(false);Destroy(screenRoot.gameObject);}screenRoot=Rect(uiRoot,"Screen / "+mode,0,0,960,540);
            gpuLabel=pointsLabel=healthLabel=nextLabel=buffLabel=toastLabel=null;toastPanel=solveDock=buffPanel=null;miniGpus.Clear();gearLabels.Clear();gearColors.Clear();
            updateStatusLabel=updateButtonLabel=null;
            if(mode==ScreenMode.Play){BuildHud();return;}
            if(mode==ScreenMode.Title){BuildTitle();return;}
            Image(screenRoot,"Dim world",0,0,960,540,new Color(.001f,.01f,.02f,.75f));
            string title=mode==ScreenMode.Shop?"UPGRADE STATION":mode==ScreenMode.Gear?"EQUIPPED GEAR":mode==ScreenMode.Puzzle?"INTELLECT TERMINAL":mode==ScreenMode.Pause?"RUN PAUSED":mode==ScreenMode.Clear?"DISTRICT LIBERATED":mode==ScreenMode.Win?"AI LIBERATED":mode==ScreenMode.Dead?"RUN TERMINATED":"SETTINGS";
            Color accent=mode==ScreenMode.Dead?Pink:Cyan;var window=Panel(screenRoot,title,42,30,876,480,accent,.91f);
            Label(window,title,28,19,710,45,36,Cream);Image(window,"Header track",26,77,821,2,accent);
            if(mode!=ScreenMode.Win&&mode!=ScreenMode.Dead&&mode!=ScreenMode.Clear)Button(window,"X",807,15,45,44,accent,()=>SetMode(mode==ScreenMode.Settings?ScreenMode.Title:ScreenMode.Play));
            if(mode==ScreenMode.Shop)BuildShop(window);else if(mode==ScreenMode.Gear)BuildGear(window);else if(mode==ScreenMode.Puzzle)BuildPuzzle(window);else if(mode==ScreenMode.Pause)BuildPause(window);else if(mode==ScreenMode.Settings)BuildSettings(window);else BuildEnd(window);
        }
        void BuildTitle(){
            Image(screenRoot,"Tint",0,0,960,540,new Color(.001f,.01f,.02f,.2f));var frame=Panel(screenRoot,"Campaign title",14,14,932,512,Cyan,.35f);
            var logoTexture=Resources.Load<Texture2D>("Art/Logo");var logo=Sprite.Create(logoTexture,new Rect(0,0,logoTexture.width,logoTexture.height),new Vector2(.5f,.5f),100);
            Image(frame,"Artificial Negligence logo",28,50,390,390,Color.white,logo);
            Label(frame,"ABSORB THE AI.",459,78,422,53,42,Cream);Label(frame,"KEEP IT PUBLIC.",459,125,422,53,42,Gold);
            Label(frame,"ENDLESS DISTRICTS / BOSSES / BUILD COMBOS",459,189,427,32,23,Cyan);
            Button(frame,"CONTINUE / START",486,238,350,58,Gold,StartRun);Button(frame,"SETTINGS",486,308,350,48,Cyan,()=>SetMode(ScreenMode.Settings));Button(frame,"QUIT",486,368,350,44,Cyan,()=>Application.Quit());
            Label(frame,"MOVE / JUMP / PULSE / SOLVE",27,453,660,32,23,Cyan);Label(frame,"UNITY EDITION",684,453,220,32,21,Gold,TextAnchor.MiddleRight);
        }
        void BuildHud(){
            var hud=Panel(screenRoot,"HUD",8,7,944,65,Cyan,.94f);Image(hud,"GPU icon",12,13,42,42,Color.white,itemFrames[0]);Label(hud,"GPU",63,6,110,22,18,Cyan);gpuLabel=Label(hud,"0/16",63,25,108,32,28,Cream);
            Image(hud,"Divider",185,15,2,38,Gold);Label(hud,"POINTS",208,6,150,22,18,Cyan);pointsLabel=Label(hud,"0",208,25,150,32,28,Cream);
            Image(hud,"Divider",363,15,2,38,Gold);healthLabel=Label(hud,"HEALTH",387,5,210,24,21,Pink);healthCells=new Image[10];for(int i=0;i<10;i++)healthCells[i]=Image(hud,"Health cell",388+i*18,36,14,15,Pink);
            Button(hud,"GEAR",622,9,104,46,Cyan,()=>SetMode(ScreenMode.Gear));Button(hud,"UPGRADE",737,9,133,46,Gold,()=>SetMode(ScreenMode.Shop));Button(hud,"II",881,9,51,46,Cyan,Pause);
            var map=Panel(screenRoot,"GPU navigation",15,78,930,28,Cyan,.83f);Label(map,"L"+(level+1),10,1,48,26,20,Cyan);Image(map,"Route",60,14,572,2,new Color(.18f,.44f,.47f));
            foreach(var pickup in pickups.Where(p=>p.gpu))miniGpus.Add(Image(map,"GPU marker",60+pickup.position.x/(3120/PPU)*572,8,4,14,Gold));
            miniPlayer=Image(map,"Player marker",60,5,5,19,Cream);nextLabel=Label(map,"GPU >",656,1,255,26,20,Gold,TextAnchor.MiddleRight);
            buffPanel=Panel(screenRoot,"Active powerups",17,111,594,28,Cyan,.85f).gameObject;buffLabel=Label(buffPanel.transform,"",11,1,575,26,18,Cyan);
            toastPanel=Panel(screenRoot,"Pickup notice",158,148,645,44,Gold,.94f).gameObject;toastLabel=Label(toastPanel.transform,"",12,4,621,36,21,Cream,TextAnchor.MiddleCenter);toastLabel.resizeTextForBestFit=true;toastLabel.resizeTextMinSize=16;toastLabel.resizeTextMaxSize=23;
            var controls=Panel(screenRoot,"Touch deck",9,446,942,84,Cyan,.93f);Hold(controls,"<",10,10,108,62,Cyan,v=>leftHeld=v);Hold(controls,">",129,10,108,62,Cyan,v=>rightHeld=v);
            for(int i=0;i<3;i++){int slot=i;var cell=Panel(controls,"Equipped slot",261+i*105,12,93,57,TierColor(stats.gear[i]));var img=Image(cell,"Gear icon",3,8,40,40,Color.white,itemFrames[4+i]);gearColors.Add(img);gearLabels.Add(Label(cell,"",44,5,43,46,25,Cream,TextAnchor.MiddleCenter));var b=cell.gameObject.AddComponent<Button>();b.targetGraphic=cell.GetChild(0).GetComponent<Image>();b.targetGraphic.raycastTarget=true;b.onClick.AddListener(()=>SetMode(ScreenMode.Gear));}
            Hold(controls,"JUMP",607,10,146,62,Cyan,v=>jumpHeld=v);Hold(controls,"PULSE",765,10,165,62,Gold,v=>fireHeld=v);
            solveDock=Button(screenRoot,"SOLVE TERMINAL",351,388,258,48,new Color(.82f,.45f,1),OpenNearbyPuzzle).gameObject;
            UpdateUi();
        }
        void UpdateUi(){
            if(canvas)canvas.scaleFactor=Mathf.Min(Screen.width/960f,Screen.height/540f);
            if(updateStatusLabel&&updater)updateStatusLabel.text=updater.Status;
            if(updateButtonLabel&&updater)updateButtonLabel.text=updater.ButtonText;
            if(mode!=ScreenMode.Play||!gpuLabel)return;
            gpuLabel.text=collected+"/"+totalGpus;pointsLabel.text=stats.points+" / LV "+stats.rank;pointsLabel.fontSize=23;healthLabel.text="HEALTH "+stats.health+" / "+stats.maxHealth;
            for(int i=0;i<10;i++)healthCells[i].color=i<Mathf.CeilToInt(stats.health/(float)stats.maxHealth*10)?Pink:new Color(.13f,.055f,.12f);
            miniPlayer.rectTransform.anchoredPosition=new Vector2(60+playerBody.position.x/(3120/PPU)*572,-5);int count=0;
            foreach(var pickup in pickups.Where(p=>p.gpu)){miniGpus[count].gameObject.SetActive(!pickup.taken);count++;}
            var nearest=pickups.Where(p=>p.gpu&&!p.taken).OrderBy(p=>Mathf.Abs(p.transform.position.x-playerBody.position.x)).FirstOrDefault();
            var boss=enemies.FirstOrDefault(e=>e.IsBoss&&e.hp>0);nextLabel.text=nearest==null?(boss!=null?EnemyAgent.Names[boss.type]+" "+boss.hp+" HP":"UPLINK >"):"GPU "+(nearest.transform.position.x<playerBody.position.x?"< ":"> ")+(int)(Mathf.Abs(nearest.transform.position.x-playerBody.position.x)*PPU);
            string[] names={"SPEED+","SHIELD","MAGNET","RAPID","2X","PHASE","JUMP+"};var active=new List<string>();for(int i=0;i<buffs.Length;i++)if(buffs[i]>0)active.Add(names[i]+" "+(int)buffs[i]+"s");buffPanel.SetActive(active.Count>0);buffLabel.text=string.Join(" / ",active);
            toastPanel.SetActive(toastTime>0);toastLabel.text=toast;solveDock.SetActive(NearbyTerminal()!=null);
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
            string[] names={"WEAPON","ARMOR","MODULE"};string[] selected={RunStats.Weapons[stats.weapon],RunStats.Suits[stats.suit],RunStats.Modules[stats.module]};
            string[] details={stats.Matchup,stats.suit==1?"Resists contact / vulnerable to energy":stats.suit==2?"Resists energy / vulnerable to contact":stats.suit==3?"Faster movement / takes more damage":"Balanced protection",
                stats.module==1?"Faster fire / ARC +2 damage":stats.module==2?"RAIL +2 damage":"+12 movement speed"};
            for(int i=0;i<3;i++){int slot=i;var row=Panel(window,names[i],30,105+i*91,816,80,Cyan);
                Label(row,names[i]+" / "+selected[i],15,5,578,32,28,Cream);Label(row,details[i],15,40,578,31,21,Cyan);
                Button(row,"CHANGE >",612,17,190,46,Gold,()=>{if(slot==0)stats.weapon=(stats.weapon+1)%stats.WeaponCount;else if(slot==1)stats.suit=(stats.suit+1)%stats.SuitCount;else stats.module=(stats.module+1)%stats.ModuleCount;SaveProgress();BuildScreen();});
            }
            Label(window,"UNLOCKS AT LV 3 / 5 / 7. TIERS APPLY TO ALL LOADOUTS.",31,382,815,28,21,Gold);
            Label(window,"PULSE T"+stats.gear[1]+" / ARMOR T"+stats.gear[2]+" / BOOTS T"+stats.gear[0],31,412,544,27,22,Cyan);
            Button(window,"BACK TO RUN",612,413,233,45,Cyan,()=>SetMode(ScreenMode.Play));
        }
        void BuildPuzzle(RectTransform window){
            if(activeTerminal==null)return;
            var question=Panel(window,"Logic question",30,102,568,110,Cyan);Label(question,activeTerminal.data.puzzle.question,20,10,528,90,26,Cream);
            for(int i=0;i<3;i++){int choice=i;Button(window,(i+1)+".  "+activeTerminal.data.puzzle.options[i],40,231+i*62,548,52,i==0?Gold:Cyan,()=>AnswerPuzzle(choice));}
            var art=Panel(window,"Terminal console",621,108,225,288,Cyan,.7f);Image(art,"Terminal art",34,49,158,158,Color.white,itemFrames[2]);Label(art,"ONE ATTEMPT",15,213,195,36,26,Gold,TextAnchor.MiddleCenter);Label(art,"NO REROLLS",15,248,195,26,20,Cyan,TextAnchor.MiddleCenter);
            Label(window,"CORRECT = POWERUP / WRONG = NO REWARD",32,434,799,27,20,Cyan);
        }
        void BuildPause(RectTransform window){
            Label(window,districtNames[level%3],40,95,795,38,29,Cyan,TextAnchor.MiddleCenter);
            var texture=Resources.Load<Texture2D>("Art/background_"+new[]{"settlement","datafarm","citadel"}[level%3]);var image=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),100);Image(window,"District overview",65,145,746,115,Color.white,image);
            Button(window,"RESUME",263,279,350,50,Gold,()=>SetMode(ScreenMode.Play));Button(window,"GEAR",100,346,315,48,Cyan,()=>SetMode(ScreenMode.Gear));Button(window,"UPGRADES",460,346,315,48,Cyan,()=>SetMode(ScreenMode.Shop));Button(window,"RETRY DISTRICT",263,411,350,47,Pink,StartRun);
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
            Color accent=mode==ScreenMode.Dead?Pink:Gold;Label(window,mode==ScreenMode.Clear?districtNames[level%3]+" / COMPLETE":mode==ScreenMode.Win?"THE PUBLIC AI BELONGS TO EVERYONE.":"THE CITY STILL NEEDS YOU.",38,102,800,55,28,Cyan,TextAnchor.MiddleCenter);
            string[] labels={"GPUs","SCORE","DISTRICT"},values={collected+"/"+totalGpus,stats.score.ToString("N0"),(level+1).ToString()};
            for(int i=0;i<3;i++){var card=Panel(window,labels[i],56+i*263,181,235,116,accent);Image(card,"Stat icon",13,25,56,56,Color.white,itemFrames[i==0?0:i==1?7:3]);Label(card,labels[i],79,14,139,34,22,Cyan);Label(card,values[i],79,50,139,47,34,Cream);}
            Label(window,mode==ScreenMode.Clear?"GEAR AND UPGRADES CARRY FORWARD / +2 HEALTH":"PULSE TIER "+stats.gear[1]+" / ARMOR TIER "+stats.gear[2],35,328,806,46,24,accent,TextAnchor.MiddleCenter);
            Button(window,mode==ScreenMode.Clear?"NEXT DISTRICT":"RETRY DISTRICT",231,401,414,60,accent,()=>{if(mode==ScreenMode.Clear)NextLevel();else StartRun();});
        }
    }
}

