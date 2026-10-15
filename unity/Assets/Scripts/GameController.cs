using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ArtificialNegligence {
    public partial class GameController : MonoBehaviour {
        public static GameController Instance;
        public const float PPU=16f;
        public static readonly Color Cyan=new Color(.16f,.9f,.95f),Gold=new Color(1f,.82f,.23f),Pink=new Color(1f,.35f,.7f),Ink=new Color(.005f,.045f,.07f),Cream=new Color(1f,.97f,.78f);
        public readonly string[] districtNames={"STREET SETTLEMENT","DATA FARM","AI CITADEL"};
        public RunStats stats=new RunStats();public ScreenMode mode=ScreenMode.Title;public int level,collected,totalGpus;public float worldLength=3120;public int RequiredGpus {get{return Mathf.CeilToInt(totalGpus*.6f);}}
        public Camera gameCamera;public Rigidbody2D playerBody;public SpriteRenderer playerVisual;
        public readonly List<EnemyAgent> enemies=new List<EnemyAgent>();
        public System.Random rng=new System.Random();PuzzleFactory puzzleFactory;
        public List<LevelRow> rows;public GameObject worldRoot;
        public Sprite[] playerFrames,enemyFrames,itemFrames,shipFrames;
        Sprite white,platformSprite,glowSprite;SpriteRenderer background;
        Material lit,unlit;Light2D playerLight;
        bool leftHeld,rightHeld,jumpHeld,fireHeld,jumpLatch,grounded;
        float jumpBuffer,coyote,shotCooldown,hurtCooldown,knockback,flash,toastTime;
        public float[] buffs=new float[7]; // speed, shield, magnet, rapid, insight, phase, jump
        public string toast="";int facing=1;
        readonly List<Pickup> pickups=new List<Pickup>();readonly List<TerminalObject> terminals=new List<TerminalObject>();
        readonly List<Bullet> bullets=new List<Bullet>();readonly List<Effect> effects=new List<Effect>();readonly List<SpriteRenderer> ships=new List<SpriteRenderer>();
        public TerminalObject activeTerminal;AudioSource audioSource;AudioClip pulseSound,pickupSound,hitSound,successSound;
        public bool soundOn=true,haptics,qaMode;MusicDirector music;public GameUpdater updater;
        public class Pickup {public Transform transform;public SpriteRenderer sprite;public Vector3 position;public Light2D light;public bool gpu,taken;public string coreId;}
        public class TerminalObject {public Transform transform;public SpriteRenderer sprite;public Light2D light;public TerminalData data;}
        class Bullet {public Transform transform;public Vector2 velocity;public bool hostile;public float life;public int damage,pierce,weaponKind;public readonly HashSet<EnemyAgent> hit=new HashSet<EnemyAgent>();}
        class Effect {public Transform transform;public SpriteRenderer visual;public Vector2 velocity;public float life,total;}

        void Awake() {
            GameplayContent.Load();
            Instance=this;Application.targetFrameRate=60;QualitySettings.vSyncCount=0;Time.fixedDeltaTime=1f/60;
            Physics2D.gravity=new Vector2(0,-550f/PPU);Physics2D.IgnoreLayerCollision(0,9,true);
            soundOn=PlayerPrefs.GetInt("sound",1)==1;haptics=PlayerPrefs.GetInt("haptics",0)==1;
            lit=Resources.Load<Material>("WorldLit");unlit=Resources.Load<Material>("WorldUnlit");
            InitializeSpriteSheets();
            white=CreateWhite();platformSprite=CreatePlatform();glowSprite=CreateGlow();
            gameCamera=new GameObject("Game Camera").AddComponent<Camera>();gameCamera.tag="MainCamera";gameCamera.orthographic=true;gameCamera.orthographicSize=270f/PPU/2;
            gameCamera.backgroundColor=Ink;gameCamera.clearFlags=CameraClearFlags.SolidColor;gameCamera.transform.position=new Vector3(15,-8.4375f,-20);gameCamera.allowHDR=true;gameCamera.gameObject.AddComponent<AudioListener>();
            var cameraData=gameCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();cameraData.renderPostProcessing=true;
            var global=new GameObject("Ambient 2D light").AddComponent<Light2D>();global.lightType=Light2D.LightType.Global;global.intensity=.85f;global.color=Color.white;
            var volume=new GameObject("Neon bloom").AddComponent<Volume>();volume.isGlobal=true;volume.profile=ScriptableObject.CreateInstance<VolumeProfile>();
            var bloom=volume.profile.Add<Bloom>(true);bloom.threshold.Override(.9f);bloom.intensity.Override(.38f);bloom.scatter.Override(.5f);graphicsBloom=bloom;
            audioSource=gameObject.AddComponent<AudioSource>();audioSource.playOnAwake=false;audioSource.volume=.18f;
            pulseSound=Tone(330,.09f);pickupSound=Tone(780,.15f);hitSound=Tone(130,.18f);successSound=Tone(1020,.2f);
            music=gameObject.AddComponent<MusicDirector>();music.Initialize(this);
            updater=gameObject.AddComponent<GameUpdater>();updater.Initialize(this);
            BuildCanvas();puzzleFactory=new PuzzleFactory(rng);LoadLevel(0);SetMode(ScreenMode.Title);
            if(Environment.GetCommandLineArgs().Contains("-qa")){qaMode=true;StartCoroutine(PreviewQa());}
        }

        public static Vector2 PixelCenter(float x,float y){return new Vector2(x/PPU,-y/PPU);}
        Sprite[] Slice(string name,int width,int height,int columns,int rowsCount){
            Texture2D texture=Resources.Load<Texture2D>("Art/"+name);if(!texture)throw new Exception("Missing art: "+name);
            var result=new Sprite[columns*rowsCount];for(int i=0;i<result.Length;i++){int col=i%columns,row=i/columns;result[i]=Sprite.Create(texture,new Rect(col*width,texture.height-(row+1)*height,width,height),new Vector2(.5f,.5f),PPU);result[i].name=name+"/"+i;}return result;
        }
        Sprite CreateWhite(){var texture=new Texture2D(1,1);texture.SetPixel(0,0,Color.white);texture.Apply();return Sprite.Create(texture,new Rect(0,0,1,1),new Vector2(.5f,.5f),1);}
        Sprite CreatePlatform(){var t=new Texture2D(24,24,TextureFormat.RGBA32,false);t.filterMode=FilterMode.Point;for(int y=0;y<24;y++)for(int x=0;x<24;x++){
            Color c=new Color(.065f,.12f,.14f);if(x==0||y==0)c=new Color(.17f,.32f,.35f);if((x==3||x==20)&&(y==3||y==20))c=new Color(.5f,.65f,.65f);if(y>=10&&y<=12&&x>5&&x<18)c=new Color(.025f,.06f,.08f);t.SetPixel(x,y,c);}
            t.Apply();return Sprite.Create(t,new Rect(0,0,24,24),new Vector2(.5f,.5f),PPU,0,SpriteMeshType.FullRect);
        }
        Sprite CreateGlow(){var t=new Texture2D(32,32,TextureFormat.RGBA32,false);t.filterMode=FilterMode.Bilinear;for(int y=0;y<32;y++)for(int x=0;x<32;x++){float a=Mathf.Pow(Mathf.Clamp01(1-Vector2.Distance(new Vector2(x,y),new Vector2(15.5f,15.5f))/16),2);t.SetPixel(x,y,new Color(1,1,1,a));}t.Apply();return Sprite.Create(t,new Rect(0,0,32,32),new Vector2(.5f,.5f),PPU);}
        public SpriteRenderer SpriteObject(string name,Sprite sprite,Vector2 position,int order,bool lighting=true,Transform parent=null){var go=new GameObject(name);go.transform.SetParent(parent?parent:worldRoot.transform,false);go.transform.position=position;var sr=go.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.sortingOrder=order;sr.sharedMaterial=lighting?lit:unlit;return sr;}
        public Light2D PointLight(Transform parent,Color color,float radius,float intensity){var go=new GameObject("Neon light");go.transform.SetParent(parent,false);var l=go.AddComponent<Light2D>();l.lightType=Light2D.LightType.Point;l.color=color;l.pointLightOuterRadius=radius;l.pointLightInnerRadius=.15f;l.intensity=intensity;return l;}

        public void SaveProgress(){if(qaMode)return;stats.EnsureCollection();if(mode!=ScreenMode.Title&&mode!=ScreenMode.Characters)stats.district=level;PlayerPrefs.SetString("progress-v2",JsonUtility.ToJson(stats));PlayerPrefs.Save();}
        public void StartRun(){
            if(!qaMode&&PlayerPrefs.HasKey("progress-v2")){try{string saved=PlayerPrefs.GetString("progress-v2");stats=JsonUtility.FromJson<RunStats>(saved);if(stats.collectionVersion<2&&!PlayerPrefs.HasKey("progress-before-loot-inventory")){PlayerPrefs.SetString("progress-before-loot-inventory",saved);PlayerPrefs.Save();}}catch{stats=new RunStats();}}
            else stats=new RunStats();stats.EnsureCollection();ApplyCharacter();stats.health=stats.maxHealth;buffs=new float[7];if(stats.stageStars==null||stats.stageStars.Length!=30)stats.stageStars=new int[30];stats.cityUnlocked=Math.Max(stats.cityUnlocked,stats.district);level=stats.district;LoadLevel(level);if(!stats.storySeen)BeginStory(ScreenMode.CityMap);else SetMode(ScreenMode.CityMap);}
        public void LoadLevel(int index){
            BeginDifficultyStage();
            if(music)music.ResetEncounter();
            if(worldRoot){worldRoot.SetActive(false);Destroy(worldRoot);}enemies.Clear();pickups.Clear();terminals.Clear();bullets.Clear();effects.Clear();ships.Clear();
            level=index;collected=0;rows=CityRoute.Generate(index);worldLength=CityRoute.Width(rows);puzzleFactory=new PuzzleFactory(new System.Random(rng.Next()));
            worldRoot=new GameObject("District "+(index+1)+" / "+CityRoute.Name(index));
            var bgTexture=Resources.Load<Texture2D>("Art/background_"+new[]{"settlement","datafarm","citadel"}[index%3]);var bgSprite=Sprite.Create(bgTexture,new Rect(0,0,bgTexture.width,bgTexture.height),new Vector2(.5f,.5f),PPU);
            background=SpriteObject("Parallax skyline",bgSprite,new Vector2(20,-6.4375f),-100,false);
            for(int i=0;i<5;i++){var ship=SpriteObject("AI ship / background",shipFrames[new[]{3,2,1,0,0}[i]],Vector2.zero,-60+i,false);ship.color=new Color(.6f,.8f,.85f,.62f+i*.06f);ship.transform.localScale=Vector3.one*(new[]{116f,88,68,38,26}[i]/128);ships.Add(ship);}
            foreach(var row in rows.Where(r=>r.kind=="platform")){
                var sr=SpriteObject("Walkable platform",PlatformStyle((row.x+row.w/2)/PPU),PixelCenter(row.x+row.w/2,row.y+row.h/2),0);sr.drawMode=SpriteDrawMode.Tiled;sr.size=new Vector2(row.w/PPU,row.h/PPU);sr.gameObject.layer=8;
                var collider=sr.gameObject.AddComponent<BoxCollider2D>();collider.size=sr.size;
                if(row.h<15){var oneWay=sr.gameObject.AddComponent<PlatformEffector2D>();oneWay.useOneWay=true;oneWay.surfaceArc=180;collider.usedByEffector=true;}
                var edge=SpriteObject("Neon footing",white,PixelCenter(row.x+row.w/2,row.y+1),2,false);edge.color=CityRoute.Boss(index)?Gold:Cyan;edge.transform.localScale=new Vector3(row.w/PPU,2f/PPU,1);
            }
            var player=new GameObject("Street survivor");player.transform.SetParent(worldRoot.transform);player.transform.position=PixelCenter(30,201);
            playerBody=player.AddComponent<Rigidbody2D>();playerBody.constraints=RigidbodyConstraints2D.FreezeRotation;playerBody.interpolation=RigidbodyInterpolation2D.Interpolate;playerBody.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            var playerCollider=player.AddComponent<BoxCollider2D>();playerCollider.size=new Vector2(12f/PPU,22f/PPU);
            var surface=new PhysicsMaterial2D("No friction"){friction=0,bounciness=0};playerCollider.sharedMaterial=surface;
            playerVisual=SpriteObject("Survivor animation",playerFrames[0],player.transform.position+Vector3.up*(3f/PPU),10,false,player.transform);
            var halo=SpriteObject("Survivor silhouette glow",glowSprite,player.transform.position,9,false,player.transform);halo.transform.localScale=new Vector3(.8f,1.15f,1);halo.color=new Color(Cyan.r,Cyan.g,Cyan.b,.18f);
            playerLight=PointLight(player.transform,Cyan,3.8f,.45f);
            foreach(var row in rows.Where(r=>r.kind=="gpu")) {
                var sr=SpriteObject("GPU",itemFrames[0],PixelCenter(row.x+6,row.y+6),6,false);sr.transform.localScale=Vector3.one*.6f;var l=PointLight(sr.transform,Gold,1.7f,.6f);
                pickups.Add(new Pickup{transform=sr.transform,sprite=sr,position=sr.transform.position,gpu=true,light=l,coreId=index+":"+pickups.Count});
            }
            var boxRows=rows.Where(r=>r.kind=="box").OrderBy(r=>rng.Next()).Take(10);
            foreach(var row in boxRows){var sr=SpriteObject("Random equipment box",itemFrames[1],PixelCenter(row.x+7.5f,row.y+7.5f),5);sr.transform.localScale=Vector3.one*.7f;pickups.Add(new Pickup{transform=sr.transform,sprite=sr,position=sr.transform.position});}
            foreach(var row in rows.Where(r=>r.kind=="terminal")){
                var sr=SpriteObject("Intellect terminal",itemFrames[2],PixelCenter(row.x+9,row.y+7),5);sr.transform.localScale=Vector3.one*.85f;
                var l=PointLight(sr.transform,Cyan,2.5f,.6f);var data=new TerminalData{puzzle=puzzleFactory.Create(index*3+terminals.Count)};
                terminals.Add(new TerminalObject{transform=sr.transform,sprite=sr,light=l,data=data});
            }
            int enemyIndex=0;foreach(var row in rows.Where(r=>r.kind=="enemy")){
                int type=(int)row.w;enemyIndex++;var sr=SpriteObject("Security / "+EnemyAgent.Names[type],enemyFrames[type*4],PixelCenter(row.x+7.5f,202.8f),8,false);
                var e=sr.gameObject.AddComponent<EnemyAgent>();e.Initialize(this,type,row.x/PPU);enemies.Add(e);
            }
            if(CityRoute.Boss(index)){
                int bossType=6+CityRoute.Area(index)%3;var bossVisual=SpriteObject(EnemyAgent.Names[bossType],enemyFrames[bossType*4],PixelCenter(worldLength-165,193.68f),8,false);
                var boss=bossVisual.gameObject.AddComponent<EnemyAgent>();boss.Initialize(this,bossType,(worldLength-165)/PPU);enemies.Add(boss);
            }
            BuildSceneDepth();
            var uplink=SpriteObject("Public AI uplink",itemFrames[3],PixelCenter(worldLength-63,187),5);uplink.transform.localScale=new Vector3(1.5f,2,1);PointLight(uplink.transform,Gold,4,.8f);
            totalGpus=pickups.Count(p=>p.gpu);activeTerminal=null;ClearInput();shotCooldown=hurtCooldown=knockback=jumpBuffer=coyote=0;Array.Clear(buffs,0,buffs.Length);
            gameCamera.transform.position=new Vector3(15,-8.4375f,-20);Toast("LEVEL "+(index+1)+" / "+CityRoute.Name(index));
        }
        public void SetMode(ScreenMode next){if(next==ScreenMode.Clear||next==ScreenMode.Dead)RecordDifficultyResult(next==ScreenMode.Dead);mode=next;Time.timeScale=mode==ScreenMode.Play?1:0;ClearInput();BuildScreen();}
        public void ClearInput(){leftHeld=rightHeld=jumpHeld=fireHeld=jumpLatch=false;}
        public void Pause(){if(mode==ScreenMode.Play)SetMode(ScreenMode.Pause);}
        public void NextLevel(){stats.health=Math.Min(stats.maxHealth,stats.health+2);level=stats.cityUnlocked;SetMode(ScreenMode.CityMap);}
        public void Toast(string text){toast=text;toastTime=2.2f;if(toastLabel)toastLabel.text=text;}
        void OnApplicationPause(bool value){if(value&&!qaMode)Pause();}
        void OnApplicationFocus(bool value){if(!value&&!qaMode)Pause();}
        void Update(){
            if(mode==ScreenMode.Story){if(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Return))AdvanceStory();if(Input.GetKeyDown(KeyCode.Escape))FinishStory();UpdateUi();return;}
            if(mode==ScreenMode.Play&&Input.GetKeyDown(KeyCode.I))SetMode(ScreenMode.Gear);
            if(mode==ScreenMode.Play&&Input.GetKeyDown(KeyCode.C)){characterReturn=ScreenMode.Pause;SetMode(ScreenMode.Characters);}
            if(Input.GetKeyDown(KeyCode.Escape)){if(mode==ScreenMode.Play)Pause();else if(mode==ScreenMode.Characters)SetMode(characterReturn);else if(mode==ScreenMode.CityMap)SetMode(ScreenMode.Title);else if(mode!=ScreenMode.Title&&mode!=ScreenMode.Dead&&mode!=ScreenMode.Win&&mode!=ScreenMode.Clear)SetMode(ScreenMode.Play);}
            TickTitlePresentation();TickSceneDepth();if(mode!=ScreenMode.Play){UpdateUi();return;}
            if(Input.GetKeyDown(KeyCode.U)){SetMode(ScreenMode.Shop);return;}if(Input.GetKeyDown(KeyCode.G)){SetMode(ScreenMode.Gear);return;}
            bool jump=jumpHeld||Input.GetKey(KeyCode.Space)||Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow);
            if(jump&&!jumpLatch)jumpBuffer=.13f;jumpLatch=jump;
            if(Input.GetKeyDown(KeyCode.E))OpenNearbyPuzzle();if(Input.GetKeyDown(KeyCode.LeftShift)||Input.GetKeyDown(KeyCode.K))Dash();
            float dt=Time.deltaTime;shotCooldown=Mathf.Max(0,shotCooldown-dt);hurtCooldown=Mathf.Max(0,hurtCooldown-dt);knockback=Mathf.Max(0,knockback-dt);flash=Mathf.Max(0,flash-dt);toastTime=Mathf.Max(0,toastTime-dt);
            for(int i=0;i<buffs.Length;i++)buffs[i]=Mathf.Max(0,buffs[i]-dt);
            if((fireHeld||Input.GetKey(KeyCode.J)||Input.GetKey(KeyCode.F))&&shotCooldown<=0)Fire();
            playerVisual.flipX=facing<0;int frame=!grounded?3:Mathf.Abs(playerBody.linearVelocity.x)>.2f?1+(int)(Time.time*8)%2:0;if(grounded&&landingTimer>0)frame=7;if(shotCooldown>.25f)frame=stats.HasWeapon?4:5;if(hurtCooldown>0)frame=stats.character==0?6:5;else if(stats.character==0&&(buffs[1]>0||buffs[5]>0)&&Mathf.Abs(playerBody.linearVelocity.x)<.2f)frame=5;playerVisual.sprite=playerFrames[frame];
            playerVisual.color=hurtCooldown>0&&((int)(hurtCooldown*12)%2==0)?new Color(1,1,1,.35f):Color.white;playerLight.intensity=.4f+flash*2;playerLight.color=buffs[1]>0||buffs[5]>0?Gold:Cyan;
            TickPickups(dt);TickBullets(dt);TickEffects(dt);TickPlatformer(dt);UpdateUi();
            if(playerBody.position.y<-20)Respawn();
            if(playerBody.position.x>(worldLength-90)/PPU&&collected>=RequiredGpus){
                if(enemies.Any(e=>e.IsBoss&&e.hp>0)){if(toastTime<=0)Toast("DEFEAT THE UPLINK BOSS FIRST");return;}
                stats.points+=50+level*5+(collected==totalGpus?50:0);stats.GainXp(60+level*5);
                stats.stageStars[level%30]=Math.Max(stats.stageStars[level%30],collected==totalGpus?3:collected>=totalGpus*.85f?2:1);
                stats.cityUnlocked=Math.Max(stats.cityUnlocked,level+1);stats.EnsureCollection();SaveProgress();Sound(successSound);SetMode(ScreenMode.Clear);
            }

        }
        void FixedUpdate(){
            if(mode!=ScreenMode.Play)return;
            grounded=Physics2D.BoxCast(playerBody.position+Vector2.down*.59f,new Vector2(.63f,.15f),0,Vector2.down,.2f,1<<8).collider!=null;
            coyote=grounded?.10f:Mathf.Max(0,coyote-Time.fixedDeltaTime);jumpBuffer=Mathf.Max(0,jumpBuffer-Time.fixedDeltaTime);
            float input=((rightHeld||Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow))?1:0)-((leftHeld||Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow))?1:0);
            float speed=(91+stats.speed*19+stats.gear[0]*7+(buffs[0]>0?50:0)+(stats.module==0?12:0)+(stats.HasArmor?(stats.suit==3?18:stats.suit==1?-10:0):0))/PPU;
            var velocity=playerBody.linearVelocity;if(knockback<=0)velocity.x=Mathf.MoveTowards(velocity.x,input*speed,(grounded?55:35)*Time.fixedDeltaTime);if(dashTime>0)velocity=new Vector2(facing*15,0);velocity.y=Mathf.Max(velocity.y,-460/PPU);
            if(input!=0)facing=input>0?1:-1;
            if(jumpBuffer>0&&coyote>0){velocity.y=(buffs[6]>0?290:235)/PPU;jumpBuffer=coyote=0;grounded=false;}
            if(!jumpLatch&&velocity.y>6&&dashTime<=0)velocity.y*=.72f;playerBody.linearVelocity=velocity;if(playerBody.position.x<.4f||playerBody.position.x>worldLength/PPU-.4f)playerBody.position=new Vector2(Mathf.Clamp(playerBody.position.x,.4f,worldLength/PPU-.4f),playerBody.position.y);
        }
        void LateUpdate(){
            if(!gameCamera||!playerBody)return;RefreshGraphics();
            float aspect=(float)Screen.width/Screen.height;const float target=16f/9;
            gameCamera.rect=graphicsTexture?new Rect(0,0,1,1):aspect>target?new Rect((1-target/aspect)/2,0,target/aspect,1):new Rect(0,(1-aspect/target)/2,1,aspect/target);
            float targetX=mode==ScreenMode.Title?15:Mathf.Clamp(playerBody.position.x+5.5f,15,worldLength/PPU-15);float x=Mathf.SmoothDamp(gameCamera.transform.position.x,targetX,ref followVelocity,.12f,Mathf.Infinity,Time.unscaledDeltaTime);gameCamera.transform.position=new Vector3(x,-8.4375f+(hurtCooldown>.85f?Mathf.Sin(Time.unscaledTime*48)*.05f:0),-20);
            if(background)background.transform.position=new Vector3(x+5-(x-15)*.03f,-6.4375f,0);
            float[] widths={116,88,68,38,26},ys={81,119,96,74,131},speeds={4,-7,12,24,-31},phases={145,440,280,85,520};
            for(int i=0;i<ships.Count;i++){float span=480+widths[i]+100;float raw=phases[i]+Time.unscaledTime*speeds[i]-(x-15)*PPU*(.05f+i*.025f);float sx=((raw%span)+span)%span-widths[i]-50;ships[i].flipX=speeds[i]<0;ships[i].transform.position=new Vector3(x-15+(sx+widths[i]/2)/PPU,-(ys[i]+widths[i]/4+Mathf.Sin(Time.unscaledTime*.5f+i)*2)/PPU,0);}
            var nearest=pickups.Where(p=>p.gpu&&!p.taken&&p.transform).OrderBy(p=>Mathf.Abs(p.transform.position.x-playerBody.position.x)).Take(4).ToArray();
            foreach(var p in pickups)if(p.light)p.light.enabled=nearest.Contains(p)&&Mathf.Abs(p.transform.position.x-gameCamera.transform.position.x)<17;
            foreach(var t in terminals)if(t.light)t.light.enabled=Mathf.Abs(t.transform.position.x-gameCamera.transform.position.x)<17;
        }
        void TickPickups(float dt){
            foreach(var p in pickups){if(p.taken||!p.transform)continue;
                if(p.gpu){if(buffs[2]>0&&Vector2.Distance(playerBody.position,p.transform.position)<5)p.position=Vector3.MoveTowards(p.position,playerBody.position,140/PPU*dt);p.transform.position=p.position+Vector3.up*Mathf.Sin(Time.time*2+p.position.x)*.06f;}
                Vector2 d=p.transform.position-(Vector3)playerBody.position;
                if(Mathf.Abs(d.x)>.85f||Mathf.Abs(d.y)>1.02f)continue;
                p.taken=true;p.sprite.enabled=false;if(p.light)p.light.enabled=false;Burst(p.transform.position,p.gpu?Gold:new Color(.8f,.3f,1),12);Sound(pickupSound);
                if(p.gpu){collected++;int amount=buffs[4]>0?20:10;stats.points+=amount;stats.score+=amount;stats.GainXp(8);int previousTier=stats.GraphicsTier;AwardGraphicsGpu(p.coreId);SaveProgress();if(stats.GraphicsTier==previousTier){Toast("+"+amount+" POINTS");toastTime=1.2f;}}
                else{int slot=stats.WeaponCount==0?1:rng.Next(5)==0?0:rng.Next(1,3),tier=RunStats.RollTier(rng.Next(200));string gear=slot==0?stats.Equip(0,tier):stats.Discover(slot,rng.Next(4),tier);string boost=GrantPowerup(false);SaveProgress();Toast(gear+" / "+boost);}
            }
        }
        public string GrantPowerup(bool puzzle){
            int choice=rng.Next(4);string result;
            if(puzzle){if(choice==0){buffs[4]=14;result="DOUBLE GPU POINTS";}else if(choice==1){buffs[5]=12;result="PHASE FIELD";}else if(choice==2){buffs[6]=16;result="SUPER JUMP";}else{stats.health=Math.Min(stats.maxHealth,stats.health+2);result="REPAIR +2";}}
            else{buffs[choice]=new[]{11f,10,12,10}[choice];result=new[]{"OVERCLOCK","SHIELD","GPU MAGNET","RAPID PULSE"}[choice];}
            return result;
        }
        public void OpenNearbyPuzzle(){var t=NearbyTerminal();if(t==null||t.data.attempted)return;activeTerminal=t;SetMode(ScreenMode.Puzzle);}
        public TerminalObject NearbyTerminal(){return terminals.FirstOrDefault(t=>!t.data.attempted&&Mathf.Abs(t.transform.position.x-playerBody.position.x)<35/PPU);}
        public void AnswerPuzzle(int choice){
            if(mode!=ScreenMode.Puzzle||activeTerminal==null||activeTerminal.data.attempted)return;
            bool correct=activeTerminal.data.Answer(choice);activeTerminal.sprite.color=correct?new Color(.55f,1,.8f):new Color(1,.4f,.6f);activeTerminal.light.color=correct?Cyan:Pink;
            if(correct){stats.GainXp(20);SaveProgress();Toast("PUZZLE REWARD / "+GrantPowerup(true));Burst(activeTerminal.transform.position,Cyan,18);Sound(successSound);}else{Toast("WRONG ANSWER / NO POWERUP");Sound(hitSound);}
            SetMode(ScreenMode.PuzzleResult);
        }
        public void Fire(){
            if(!stats.HasWeapon){shotCooldown=.5f;var target=enemies.FirstOrDefault(e=>e&&e.hp>0&&Mathf.Abs(e.transform.position.y-playerBody.position.y)<1.3f&&(e.transform.position.x-playerBody.position.x)*facing>=0&&Mathf.Abs(e.transform.position.x-playerBody.position.x)<1.6f);if(target)target.Hit(1);Burst(playerBody.position+Vector2.right*facing*.8f,Cyan,3);return;}
            float interval=new[]{.36f,.5f,.65f,.85f}[stats.weapon];shotCooldown=buffs[3]>0?.12f:Mathf.Max(.12f,interval-stats.gear[1]*.025f-(stats.module==1?.06f:0));flash=.18f;
            var origin=playerBody.position+new Vector2(facing*.6f,.04f);float speed=stats.weapon==3?27:stats.weapon==1?12:245/PPU;
            SpawnBullet(origin,new Vector2(facing*speed,0),false,stats.gear[1],stats.weapon==3?3:stats.gear[1]>=4?1:0);
            if(stats.weapon==2){SpawnBullet(origin,new Vector2(facing*speed,2.4f),false);SpawnBullet(origin,new Vector2(facing*speed,-2.4f),false);}
            Sound(pulseSound);
        }
        public void SpawnBullet(Vector2 position,Vector2 velocity,bool hostile,int damage=1,int pierce=0){var sr=SpriteObject(hostile?"Security bolt":"Pulse",white,position,12,false);sr.transform.localScale=new Vector3(.42f,.14f,1);sr.color=hostile?Pink:Cyan;var glow=SpriteObject("Pulse glow",glowSprite,position,11,false,sr.transform);glow.transform.localScale=Vector3.one*1.9f;glow.color=new Color(sr.color.r,sr.color.g,sr.color.b,.6f);bullets.Add(new Bullet{transform=sr.transform,velocity=velocity,hostile=hostile,damage=damage,pierce=pierce,weaponKind=stats.weapon,life=!hostile&&stats.weapon==2?.45f:1.4f});}
        void TickBullets(float dt){
            for(int i=bullets.Count-1;i>=0;i--){var b=bullets[i];if(!b.transform){bullets.RemoveAt(i);continue;}Vector2 prev=b.transform.position,next=prev+b.velocity*dt;b.transform.position=next;b.life-=dt;
                if(Physics2D.Linecast(prev,next,1<<8).collider)b.life=0;
                if(b.hostile){if(Mathf.Abs(next.x-playerBody.position.x)<.55f&&Mathf.Abs(next.y-playerBody.position.y)<.75f){Damage(b.damage,Mathf.Sign(b.velocity.x),true);b.life=0;}}
                else foreach(var e in enemies){if(!e||e.hp<=0||b.hit.Contains(e))continue;if(Mathf.Abs(next.x-e.transform.position.x)<(e.IsBoss?1.7f:.8f)&&Mathf.Abs(next.y-e.transform.position.y)<(e.IsBoss?1.3f:.75f)){b.hit.Add(e);e.Hit(stats.HitDamage(e.type));if(b.weaponKind==1){foreach(var nearby in enemies.Where(n=>n!=e&&n.hp>0&&Vector2.Distance(n.transform.position,e.transform.position)<2.8f).Take(2))nearby.Hit(Math.Max(1,stats.HitDamage(nearby.type)/2));}if(b.pierce--<=0)b.life=0;break;}}
                if(b.life<=0){Destroy(b.transform.gameObject);bullets.RemoveAt(i);}
            }
        }
        public void Damage(int amount,float direction,bool energy=false){if(hurtCooldown>0||qaMode)return;if(buffs[1]>0||buffs[5]>0){Burst(playerBody.position,Gold,6);hurtCooldown=.2f;return;}
            stageHits++;stats.health-=stats.Incoming(Math.Max(1,Mathf.RoundToInt(amount*GameplayContent.Current.enemyDamage)),energy);hurtCooldown=1.1f;knockback=.18f;playerBody.linearVelocity=new Vector2(direction*5,7);Burst(playerBody.position,Pink,12);Sound(hitSound);if(haptics)Haptic();Toast("SECURITY HIT / HEALTH "+stats.health);
            if(stats.health<=0)SetMode(ScreenMode.Dead);
        }
        void Respawn(){stageHits++;float x=playerBody.position.x*PPU;var safe=rows.Where(r=>r.kind=="platform"&&r.y==218&&r.x<x).OrderByDescending(r=>r.x).FirstOrDefault();playerBody.position=checkpointPosition;playerBody.linearVelocity=Vector2.zero;stats.health--;hurtCooldown=1;Toast("FELL INTO THE GRID / HEALTH "+stats.health);if(stats.health<=0)SetMode(ScreenMode.Dead);}
        public void EnemyDown(EnemyAgent enemy){stageKills++;int reward=enemy.IsBoss?60+level*10:4+level;stats.points+=reward;stats.score+=reward;string gain=stats.GainXp(enemy.IsBoss?80:12);SaveProgress();Burst(enemy.transform.position,Pink,15);Toast((enemy.IsBoss?"BOSS DEFEATED":EnemyAgent.Names[enemy.type]+" DISABLED")+" / +"+reward+" PTS / "+gain);}
        public void Burst(Vector2 position,Color color,int count){count=Math.Max(2,Mathf.RoundToInt(count*new[]{.3f,.45f,.6f,.8f,1f,1.2f}[stats.GraphicsTier]));for(int i=0;i<count;i++){var sr=SpriteObject("Pixel spark",white,position,14,false);sr.color=color;sr.transform.localScale=Vector3.one*UnityEngine.Random.Range(.07f,.16f);float life=UnityEngine.Random.Range(.3f,.65f);effects.Add(new Effect{transform=sr.transform,visual=sr,velocity=UnityEngine.Random.insideUnitCircle*4,life=life,total=life});}}
        void TickEffects(float dt){for(int i=effects.Count-1;i>=0;i--){var e=effects[i];if(!e.transform){effects.RemoveAt(i);continue;}e.life-=dt;e.velocity+=Vector2.down*5*dt;e.transform.position+=(Vector3)(e.velocity*dt);var c=e.visual.color;c.a=e.life/e.total;e.visual.color=c;if(e.life<=0){Destroy(e.transform.gameObject);effects.RemoveAt(i);}}}
        AudioClip Tone(float frequency,float duration){int rate=22050;float[] samples=new float[(int)(duration*rate)];for(int i=0;i<samples.Length;i++)samples[i]=Mathf.Sin(2*Mathf.PI*frequency*i/rate)*Mathf.Pow(1-i/(float)samples.Length,2);var clip=AudioClip.Create("Synth effect",samples.Length,1,rate,false);clip.SetData(samples,0);return clip;}
        void Sound(AudioClip clip){if(soundOn&&clip)audioSource.PlayOneShot(clip);}
        void Haptic(){
#if UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }
        void CaptureQa(string path){
            var worldTarget=gameCamera.targetTexture;
            if(worldTarget){canvas.enabled=false;RenderPipeline.SubmitRenderRequest(gameCamera,new UniversalRenderPipeline.SingleCameraRequest{destination=worldTarget});canvas.enabled=true;gameCamera.targetTexture=null;}
            var target=new RenderTexture(960,540,24,RenderTextureFormat.ARGB32);target.Create();
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=gameCamera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();
            var request=new UniversalRenderPipeline.SingleCameraRequest{destination=target};RenderPipeline.SubmitRenderRequest(gameCamera,request);
            var previous=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(960,540,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,960,540),0,0);image.Apply();
            System.IO.File.WriteAllBytes(path,image.EncodeToPNG());RenderTexture.active=previous;target.Release();Destroy(target);Destroy(image);canvas.renderMode=RenderMode.ScreenSpaceOverlay;gameCamera.targetTexture=worldTarget;
            Debug.Log("AN_CAPTURE / "+path);
        }

        void CaptureAtlasQa(string path,string title,Sprite[] frames,int columns){
            SetMode(ScreenMode.Pause);var panel=Panel(screenRoot,"Sprite inspection",8,8,944,524,Cyan,1);
            Label(panel,title+" / ISOLATED FRAMES",20,12,900,35,28,Cream);
            int rowCount=Mathf.CeilToInt(frames.Length/(float)columns);float cellWidth=900f/columns,cellHeight=Mathf.Min(105,450f/rowCount);
            for(int i=0;i<frames.Length;i++){
                float x=22+(i%columns)*cellWidth,y=56+(i/columns)*cellHeight;
                var tile=Panel(panel,"Frame "+i,x,y,cellWidth-8,cellHeight-5,new Color(.15f,.3f,.35f),1);
                float size=Mathf.Min(72,cellHeight-23);Image(tile,"Full sprite",(cellWidth-8-size)/2,3,size,size,Color.white,frames[i]).preserveAspect=true;Label(tile,"FRAME "+i,4,cellHeight-23,cellWidth-16,17,14,Cyan,TextAnchor.MiddleCenter);
            }CaptureQa(System.IO.Path.Combine(path,"atlas-"+title.ToLowerInvariant()+".png"));
        }
        IEnumerator PreviewQa(){
            string[] args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-qaOutput");string path=at>=0?args[at+1]:Application.persistentDataPath;System.IO.Directory.CreateDirectory(path);
            yield return new WaitForSecondsRealtime(1);
            CaptureAtlasQa(path,"Looper",looperFrames,4);CaptureAtlasQa(path,"Survivors",survivorAtlas,8);CaptureAtlasQa(path,"Enemies",enemyFrames,6);CaptureAtlasQa(path,"Equipment",itemFrames,4);CaptureAtlasQa(path,"Ships",shipFrames,4);SetMode(ScreenMode.Title);
            yield return new WaitForSecondsRealtime(.3f);var attractStart=titleRunner.transform.position;yield return new WaitForSecondsRealtime(.7f);if(titleRunner.transform.position==attractStart)throw new Exception("Start scene actor did not animate");if(stats.GraphicsGpus!=0)throw new Exception("Title granted GPU progress");CaptureQa(System.IO.Path.Combine(path,"unity-title.png"));Debug.Log("AN_TITLE_QA_PASS / live actor movement, no progression awarded");
            StartRun();if(mode!=ScreenMode.Story)throw new Exception("First run did not start with story");yield return new WaitForSecondsRealtime(.2f);CaptureQa(System.IO.Path.Combine(path,"unity-story.png"));for(int page=0;page<4;page++)AdvanceStory();if(!stats.storySeen||mode!=ScreenMode.CityMap)throw new Exception("Story did not lead to city map");Debug.Log("AN_STORY_QA_PASS / first-run opening and completion");yield return new WaitForSecondsRealtime(.2f);CaptureQa(System.IO.Path.Combine(path,"unity-city-map.png"));
            characterReturn=ScreenMode.CityMap;SetMode(ScreenMode.Characters);yield return new WaitForSecondsRealtime(.2f);
            var portraits=screenRoot.GetComponentsInChildren<UnityEngine.UI.Image>().Where(i=>i.name=="Character silhouette or portrait").ToArray();if(portraits.Length!=4||portraits.Count(i=>i.color==Color.black)!=3)throw new Exception("Locked character silhouettes failed");
            if(screenRoot.GetComponentsInChildren<UnityEngine.UI.Image>().Count(i=>i.name=="Equipped item")!=1)throw new Exception("Character equipped item preview failed");
            CaptureQa(System.IO.Path.Combine(path,"unity-characters-locked.png"));
            var feedback=screenRoot.GetComponentInChildren<ButtonFeedback>();var feedbackLabel=feedback.GetComponentInChildren<UnityEngine.UI.Text>();var labelPosition=feedbackLabel.rectTransform.anchoredPosition;
            feedback.OnPointerDown(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));yield return new WaitForSecondsRealtime(.12f);if(feedbackLabel.rectTransform.anchoredPosition==labelPosition)throw new Exception("Paused button feedback failed");feedback.OnPointerUp(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
            SetMode(ScreenMode.Settings);yield return new WaitForSecondsRealtime(.2f);CaptureQa(System.IO.Path.Combine(path,"unity-settings.png"));
            Debug.Log("AN_UI_QA_PASS / three locked silhouettes, empty weapon/armor slots and starter shoes, unscaled press feedback");
            SetMode(ScreenMode.Gear);yield return new WaitForSecondsRealtime(.2f);if(stats.WeaponCount!=0||stats.SuitCount!=0||screenRoot.GetComponentsInChildren<UnityEngine.UI.Image>().Any(i=>i.name=="Found equipment"))throw new Exception("Inventory granted starter loot");CaptureQa(System.IO.Path.Combine(path,"unity-empty-inventory.png"));StartStage(0);int initialBullets=bullets.Count;Fire();if(bullets.Count!=initialBullets)throw new Exception("Unarmed player fired a free weapon");
            float start=playerBody.position.x;rightHeld=true;fireHeld=true;jumpHeld=true;yield return new WaitForSecondsRealtime(.9f);jumpHeld=false;Dash();yield return new WaitForSecondsRealtime(.4f);ClearInput();
            if(playerBody.position.x<=start+2)throw new Exception("QA movement failed");stats.points=300;stats.rank=7;stats.Discover(1,0,3);stats.SelectEquipment(1,0);stats.Discover(2,0,4);stats.SelectEquipment(2,0);stats.maxHealth=stats.health=9;
            yield return new WaitForSecondsRealtime(.2f);CaptureQa(System.IO.Path.Combine(path,"unity-gameplay.png"));
            CaptureQa(System.IO.Path.Combine(path,"unity-graphics-bootstrap.png"));var originalCores=stats.graphicsCores;
            for(int tier=0;tier<6;tier++){stats.graphicsCores=Enumerable.Range(0,RendererProgress.Thresholds[tier]).Select(i=>"qa:"+i).ToList();RefreshGraphics(true);yield return new WaitForSecondsRealtime(.2f);if(graphicsTexture.width!=RendererProgress.Widths[tier])throw new Exception("Graphics tier render failed");CaptureQa(System.IO.Path.Combine(path,"unity-graphics-tier-"+tier+".png"));}
            stats.graphicsCores=originalCores;RefreshGraphics(true);
            SetMode(ScreenMode.Shop);yield return new WaitForSecondsRealtime(.2f);CaptureQa(System.IO.Path.Combine(path,"unity-upgrades.png"));stats.Upgrade(1);if(stats.gear[1]!=4)throw new Exception("QA upgrade failed");
            SetMode(ScreenMode.Play);activeTerminal=terminals[0];activeTerminal.data.puzzle=new PuzzleFactory(new System.Random(42)).Create(0);SetMode(ScreenMode.Puzzle);yield return new WaitForSecondsRealtime(.2f);CaptureQa(System.IO.Path.Combine(path,"unity-puzzle.png"));
            var terminal=activeTerminal;AnswerPuzzle(terminal.data.puzzle.circuit?(terminal.data.puzzle.circuitTarget+1)%16:(terminal.data.puzzle.correct+1)%3);if(terminal.data.solved||!terminal.data.attempted)throw new Exception("QA wrong answer");
            activeTerminal.data=new TerminalData{puzzle=new LogicPuzzle("Circuit","5","0","16"){circuit=true,circuitTarget=5,explanation="XOR compares neighboring switch states."}};SetMode(ScreenMode.Puzzle);yield return new WaitForSecondsRealtime(.2f);CaptureQa(System.IO.Path.Combine(path,"unity-circuit.png"));AnswerPuzzle(5);if(!activeTerminal.data.solved)throw new Exception("Circuit reward failed");yield return new WaitForSecondsRealtime(.2f);CaptureQa(System.IO.Path.Combine(path,"unity-puzzle-result.png"));
            stats.Discover(1,1,3);stats.Discover(2,2,2);SetMode(ScreenMode.Gear);yield return new WaitForSecondsRealtime(.2f);CaptureQa(System.IO.Path.Combine(path,"unity-loadout.png"));
            armorCollection=true;BuildScreen();yield return new WaitForSecondsRealtime(.2f);CaptureQa(System.IO.Path.Combine(path,"unity-armor.png"));
            stats.graphicsCores=Enumerable.Range(0,75).Select(i=>"qa-character:"+i).ToList();stats.cityUnlocked=6;RefreshGraphics(true);characterReturn=ScreenMode.Gear;SetMode(ScreenMode.Characters);yield return new WaitForSecondsRealtime(.2f);CaptureQa(System.IO.Path.Combine(path,"unity-characters.png"));
            stats.character=1;stats.outfit=2;ApplyCharacter();if(playerFrames.Length!=8)throw new Exception("Character frames failed");SetMode(ScreenMode.Play);yield return new WaitForSecondsRealtime(.2f);CaptureQa(System.IO.Path.Combine(path,"unity-custom-survivor.png"));
            stats.graphicsCores=originalCores;RefreshGraphics(true);
            int[] stages={5,11,17};foreach(int stage in stages){LoadLevel(stage);SetMode(ScreenMode.Play);var boss=enemies.First(e=>e.IsBoss);playerBody.position=(Vector2)boss.transform.position+Vector2.left*4;yield return new WaitForSecondsRealtime(2.8f);if(boss.AttackCount==0)throw new Exception("Boss cannot attack");boss.Hit(Mathf.CeilToInt(boss.maxHp*.6f));yield return new WaitForSecondsRealtime(2.8f);if(stage==11)CaptureQa(System.IO.Path.Combine(path,"unity-boss.png"));boss.Hit(999);}
            LoadLevel(12);SetMode(ScreenMode.Play);yield return new WaitForSecondsRealtime(.3f);CaptureQa(System.IO.Path.Combine(path,"unity-datafarm.png"));LoadLevel(24);yield return new WaitForSecondsRealtime(.3f);CaptureQa(System.IO.Path.Combine(path,"unity-citadel.png"));
            if(NextAdaptivePressure(0,0,5,1,false)<=0||NextAdaptivePressure(0,6,0,.6f,false)>=0||NextAdaptivePressure(0,0,0,0,true)>=0||NextAdaptivePressure(.45f,0,5,1,false)>.45f||NextAdaptivePressure(-.3f,9,0,0,true)<-.3f)throw new Exception("Adaptive difficulty bounds or performance response failed");
            Debug.Log("AN_ADAPTIVE_QA_PASS / strong clears, struggling clears, defeat, upper and lower bounds");
            Debug.Log("AN_RUNTIME_QA_PASS 88 isolated atlas frames, compact HUD, collection, four characters, outfit selection, all six graphics tiers, city map, movement, dash, upgrades, one-attempt puzzles, all three bosses");Application.Quit(0);
        }
    }
}

