using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using ArtificialNegligence;

public static class BuildGame {
    static string Root=>Path.GetFullPath(Path.Combine(Application.dataPath,"../.."));
    static void Assert(bool value,string message){if(!value)throw new Exception("CHECK FAILED / "+message);}
    public static void Configure(){
        foreach(string path in AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/Resources/Music"}).Select(AssetDatabase.GUIDToAssetPath)){
            var importer=AssetImporter.GetAtPath(path) as AudioImporter;if(importer==null)continue;
            var sample=importer.defaultSampleSettings;sample.loadType=AudioClipLoadType.Streaming;sample.compressionFormat=AudioCompressionFormat.Vorbis;sample.quality=.25f;sample.preloadAudioData=false;
            sample.sampleRateSetting=AudioSampleRateSetting.OverrideSampleRate;sample.sampleRateOverride=32000;
            importer.defaultSampleSettings=sample;importer.SetOverrideSampleSettings("Android",sample);importer.loadInBackground=true;importer.SaveAndReimport();
        }
        foreach(string path in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Resources/Art"}).Select(AssetDatabase.GUIDToAssetPath)){
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)continue;
            bool changed=importer.textureCompression!=TextureImporterCompression.Uncompressed||importer.filterMode!=FilterMode.Point||!importer.isReadable||importer.npotScale!=TextureImporterNPOTScale.None||importer.mipmapEnabled;
            importer.textureType=TextureImporterType.Default;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.isReadable=true;importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=2048;if(changed)importer.SaveAndReimport();
        }
        Directory.CreateDirectory("Assets/Resources");Directory.CreateDirectory("Assets/Settings");
        var renderer=AssetDatabase.LoadAssetAtPath<Renderer2DData>("Assets/Settings/Neon2DRenderer.asset");
        if(!renderer){renderer=ScriptableObject.CreateInstance<Renderer2DData>();AssetDatabase.CreateAsset(renderer,"Assets/Settings/Neon2DRenderer.asset");}
        var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/NeonURP.asset");
        if(!pipeline){pipeline=UniversalRenderPipelineAsset.Create(renderer);AssetDatabase.CreateAsset(pipeline,"Assets/Settings/NeonURP.asset");}
        pipeline.msaaSampleCount=1;pipeline.supportsHDR=true;pipeline.renderScale=1;GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;
        foreach(var shader in new[]{"Sprite-Lit-Default","Sprite-Unlit-Default"}){
            string path="Assets/Resources/"+(shader.Contains("Unlit")?"WorldUnlit":"WorldLit")+".mat";
            if(!AssetDatabase.LoadAssetAtPath<Material>(path)){var s=Shader.Find("Universal Render Pipeline/2D/"+shader);Assert(s!=null,"2D shader missing");AssetDatabase.CreateAsset(new Material(s),path);}
        }
        PlayerSettings.companyName="Artificial Negligence";PlayerSettings.productName="Artificial Negligence";PlayerSettings.bundleVersion="2.4.6";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.artificialnegligence.game");PlayerSettings.Android.bundleVersionCode=246;
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;PlayerSettings.allowedAutorotateToPortrait=false;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
        PlayerSettings.defaultScreenWidth=960;PlayerSettings.defaultScreenHeight=540;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=false;PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.runInBackground=true;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.OpenGLES3});
        PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevel35;PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.Android,Il2CppCompilerConfiguration.Release);
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android,ManagedStrippingLevel.Low);
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown,new[]{Resources.Load<Texture2D>("Art/Logo")},IconKind.Any);AndroidBranding.Configure();
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);new GameObject("Artificial Negligence").AddComponent<GameController>();
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/ArtificialNegligence.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/ArtificialNegligence.unity",true)};
        EditorUtility.SetDirty(pipeline);AssetDatabase.SaveAssets();
        Debug.Log("AN_CONFIGURE_PASS / Unity URP 2D, mobile UI, logo, ARM64");
    }
    public static void Validate(){
        var gpuProgress=new RunStats();Assert(gpuProgress.AbsorbGraphicsGpu("0:0")&&!gpuProgress.AbsorbGraphicsGpu("0:0")&&gpuProgress.GraphicsGpus==1,"Duplicate GPU graphics credit");
        for(int tier=1;tier<6;tier++){Assert(RendererProgress.Tier(RendererProgress.Thresholds[tier]-1)==tier-1&&RendererProgress.Tier(RendererProgress.Thresholds[tier])==tier,"Graphics threshold incorrect");}
        var restoredGpu=JsonUtility.FromJson<RunStats>(JsonUtility.ToJson(gpuProgress));Assert(restoredGpu.GraphicsGpus==1&&!restoredGpu.AbsorbGraphicsGpu("0:0"),"Graphics progression does not save");
        Assert(Resources.Load<Shader>("Evolution")!=null,"Graphics progression shader missing");
        Assert(GameplayContent.Parse("{\"version\":2,\"enemyHealth\":1,\"bossHealth\":1,\"enemySpeed\":1,\"enemyDamage\":1,\"weaponDamage\":1,\"xp\":1}")!=null,"Gameplay content rejected");
        Assert(GameplayContent.Parse("{\"version\":2,\"enemyHealth\":-1}")==null&&GameplayContent.Parse("invalid")==null,"Invalid gameplay content accepted");
        string baseline=Path.GetFullPath(Path.Combine(Root,"../work/updater-baseline.apk"));
        if(File.Exists(baseline)){
            long sum=0;var protocol=new List<string>();foreach(var chunk in UpdateChunks.Read(baseline)){Assert(chunk.data.Length<=UpdateChunks.Max&&chunk.offset==sum,"Chunk offsets invalid");sum+=chunk.data.Length;protocol.Add(chunk.offset+","+chunk.data.Length+","+chunk.hash);}
            Assert(sum==new FileInfo(baseline).Length,"Chunk reconstruction length invalid");File.WriteAllLines(Path.Combine(Path.GetDirectoryName(baseline),"protocol-csharp.txt"),protocol);
        }
        foreach(var name in new[]{"menu","gameplay","level99","glitch8bit","electronicpunk","upgrades","bosscombat","victory","gameover"})Assert(Resources.Load<AudioClip>("Music/"+name)?.length>0,"Music missing: "+name);
        Assert(MusicDirector.Pick(ScreenMode.Title,false)=="menu"&&MusicDirector.Pick(ScreenMode.Play,false)=="gameplay"&&MusicDirector.Pick(ScreenMode.Play,true)=="bosscombat"&&MusicDirector.Pick(ScreenMode.Shop,false)=="upgrades"&&MusicDirector.Pick(ScreenMode.Gear,true)=="upgrades"&&MusicDirector.Pick(ScreenMode.Clear,false)=="victory"&&MusicDirector.Pick(ScreenMode.Dead,true)=="gameover","Music state mapping failed");
        Assert(Enumerable.Range(0,4).Select(i=>MusicDirector.PickGameplay(i)).Distinct().Count()==4&&MusicDirector.PickGameplay(1,1)=="glitch8bit"&&MusicDirector.RotationDuration(20)==20,"Gameplay music rotation failed");
        var progression=new RunStats();progression.GainXp(300);Assert(progression.rank>=3&&progression.talents>=2&&progression.WeaponCount==0&&progression.SuitCount==0,"Leveling granted equipment");
        progression.points=500;Assert(progression.Train(true)=="WEAPON MASTERY +1"&&progression.power==1,"Talent purchase failed");
        progression.Discover(1,1,1);progression.SelectEquipment(1,1);int arcShield=progression.HitDamage(3),arcHeavy=progression.HitDamage(2);Assert(arcShield>arcHeavy,"Arc counters missing");
        progression.Discover(1,3,1);progression.SelectEquipment(1,3);Assert(progression.HitDamage(2)>progression.HitDamage(3),"Rail counters missing");
        progression.Discover(2,1,1);progression.SelectEquipment(2,1);Assert(progression.Incoming(3,true)>progression.Incoming(3,false),"Armor tradeoff missing");
        var restored=JsonUtility.FromJson<RunStats>(JsonUtility.ToJson(progression));Assert(restored.rank==progression.rank&&restored.power==1&&restored.weapon==3,"Progress save failed");
        var tierCounts=new int[6];for(int i=0;i<200;i++)tierCounts[RunStats.RollTier(i)]++;
        Assert(tierCounts.SequenceEqual(new[]{0,120,50,20,9,1}),"Ultra drop is not exactly 0.5 percent");
        int[] positions=new int[3];var across=new HashSet<string>();
        for(int run=0;run<200;run++){
            var factory=new PuzzleFactory(new System.Random(run));var within=new HashSet<string>();
            for(int i=0;i<9;i++){var puzzle=factory.Create(i);Assert(within.Add(puzzle.question),"Repeated puzzle");across.Add(puzzle.question);Assert(puzzle.options.Distinct().Count()==3,"Duplicate options");Assert(puzzle.options[puzzle.correct]==ExpectedPuzzle(puzzle),"Wrong generated solution");positions[puzzle.correct]++;
                int correct=puzzle.circuit?puzzle.circuitTarget:puzzle.correct;int wrong=puzzle.circuit?(correct+1)%16:(correct+1)%3;
                var terminal=new TerminalData{puzzle=puzzle};Assert(!terminal.Answer(wrong)&&terminal.attempted&&!terminal.solved,"Wrong attempt rewarded");Assert(!terminal.Answer(correct),"Wrong attempt can retry");
                var good=new TerminalData{puzzle=puzzle};Assert(good.Answer(correct)&&!good.Answer(correct),"Reward can repeat");
            }
        }
        Assert(across.Count>300&&positions.All(x=>x>400),"Puzzles do not vary");
        var stats=new RunStats{points=500};stats.Upgrade(1);stats.Upgrade(2);Assert(stats.gear[1]==0&&stats.gear[2]==0&&stats.points==500,"Workshop created equipment");stats.Discover(1,0,1);stats.SelectEquipment(1,0);stats.Discover(2,0,1);stats.SelectEquipment(2,0);stats.Upgrade(1);stats.Upgrade(2);Assert(stats.gear[1]==2&&stats.gear[2]==2&&stats.maxHealth==7,"Owned item upgrade logic");stats.points=10000;stats.Upgrade(1);stats.Upgrade(1);stats.Upgrade(1);Assert(stats.gear[1]==4,"Ultra can be purchased");
        var protocolChunks=Enumerable.Range(0,20).Select(i=>new GameUpdater.Chunk{sha256=i.ToString(),size=100000}).ToArray();
        var ranges=RangeProtocol.Plan(protocolChunks,new System.Collections.Generic.HashSet<string>(new[]{"3","4","13"}));
        Assert(ranges.All(x=>x.length<=RangeProtocol.MaxRange)&&ranges.Sum(x=>x.length)==1700000,"Range grouping byte count");
        Assert(ranges.SelectMany(x=>x.chunks).SequenceEqual(Enumerable.Range(0,20).Where(i=>i!=3&&i!=4&&i!=13)),"Range grouping skips or repeats data");
        Assert(RangeProtocol.Plan(protocolChunks,new System.Collections.Generic.HashSet<string>(protocolChunks.Select(x=>x.sha256))).Count==0,"Unchanged APK downloads data");
        Assert(RangeProtocol.SafeUrl("https://github.com/dustindwayne/artificial-negligence/releases/download/v2.3.0/ArtificialNegligence.apk")&&!RangeProtocol.SafeUrl("https://example.com/bad.apk"),"Unsafe release URL");
        Debug.Log("AN_RANGE_PLAN_PASS / bounded grouped requests, reused holes, zero-download case and URL validation");
        foreach(int cleared in new[]{0,5,6,11,12,13,40}){
            var legacy=JsonUtility.FromJson<RunStats>("{\"cityUnlocked\":"+cleared+"}");legacy.EnsureCollection();
            Assert(legacy.wrathEncounters==(cleared>=12?2:cleared>=6?1:0)&&!legacy.wrathFreed,"Legacy Wrath chapters not restored / "+cleared);
            int before=legacy.wrathEncounters;legacy.EnsureCollection();Assert(legacy.wrathEncounters==before&&legacy.wrathProgressVersion==1,"Wrath recovery repeated");
        }
        var oldDistrict=JsonUtility.FromJson<RunStats>("{\"district\":12}");oldDistrict.EnsureCollection();Assert(oldDistrict.wrathEncounters==2&&!oldDistrict.CharacterUnlocked(4),"District-only save lost pending final fight");
        Debug.Log("AN_WRATH_SAVE_MIGRATION_PASS / legacy stages, district-only saves, idempotence, final fight still required");
        var inventory=new RunStats();inventory.EnsureCollection();Assert(inventory.WeaponCount==0&&inventory.SuitCount==0&&!inventory.HasWeapon&&!inventory.HasArmor&&!inventory.SelectEquipment(1,3),"Starter equipment was granted");
        inventory.GainXp(10000);Assert(inventory.WeaponCount==0&&inventory.SuitCount==0,"XP granted equipment");
        inventory.Discover(1,3,3);Assert(!inventory.HasWeapon,"Find auto-equipped a weapon");inventory.SelectEquipment(1,3);int first=inventory.weaponItemId;
        inventory.Discover(1,3,2);Assert(inventory.WeaponCount==2&&inventory.weaponItemId==first&&inventory.gear[1]==3,"Duplicate was discarded or auto-equipped");inventory.EquipItem(inventory.inventory.Last().id);Assert(inventory.gear[1]==2,"Cannot equip an individual weaker find");inventory.EquipItem(first);
        inventory.Discover(2,2,4);inventory.SelectEquipment(2,2);int hp=inventory.maxHealth;inventory.Discover(2,0,1);inventory.SelectEquipment(2,0);inventory.SelectEquipment(2,2);Assert(inventory.maxHealth==hp,"Armor switch farms health");
        var oldSave=new RunStats{collectionVersion=1,rank=7,gear=new[]{2,4,3},weapon=3,suit=2,maxHealth=8};oldSave.EnsureCollection();Assert(oldSave.WeaponCount==0&&oldSave.SuitCount==0&&oldSave.rank==7&&oldSave.maxHealth==5,"Legacy menu grants remain");
        Assert(inventory.character==0&&RunStats.Characters[0]=="LOOPER"&&!inventory.CharacterUnlocked(1)&&!inventory.CharacterUnlocked(2),"Starter or locked characters wrong");inventory.rank=1;inventory.graphicsCores=Enumerable.Range(0,24).Select(i=>"unlock:"+i).ToList();Assert(inventory.CharacterUnlocked(1)&&!inventory.CharacterUnlocked(2),"GPU character unlock");inventory.cityUnlocked=6;Assert(inventory.CharacterUnlocked(2)&&!inventory.CharacterUnlocked(3),"Boss character unlock");
        var inventorySave=JsonUtility.FromJson<RunStats>(JsonUtility.ToJson(inventory));inventorySave.EnsureCollection();Assert(inventorySave.WeaponCount==2&&inventorySave.SuitCount==2&&inventorySave.weaponRarities[3]==3&&inventorySave.armorRarities[2]==4&&inventorySave.CharacterUnlocked(2),"Collection save failed");
        foreach(var spec in new[]{new[]{"looper","4","2","64","64","32"},new[]{"survivors-v3","8","3","64","64","32"},new[]{"enemies-v3","4","9","192","120","60"},new[]{"items-v3","4","4","64","64","32"},new[]{"ai_ships","4","1","128","64","16"},new[]{"sinister-agents","4","3","64","64","32"},new[]{"ai-wrath","4","2","64","64","32"},new[]{"wonder-wrath","4","2","64","64","32"}}){
            var frames=SpriteAtlas.Load(spec[0],int.Parse(spec[1]),int.Parse(spec[2]),int.Parse(spec[3]),int.Parse(spec[4]),int.Parse(spec[5]),spec[0]!="items-v3"&&spec[0]!="ai_ships",spec[0]=="enemies-v3"?3:spec[0]=="ai_ships"?2:4);
            foreach(var frame in frames){var texture=frame.texture;var pixels=texture.GetPixels32();Assert(pixels.Any(c=>c.a>24),"Empty normalized sprite");for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)if(x<2||y<2||x>=texture.width-2||y>=texture.height-2)Assert(pixels[y*texture.width+x].a==0,"Sprite gutter clipped / "+frame.name);}
        }
        Debug.Log("AN_COLLECTION_PASS / migration, zero starter/XP gear, individual finds, manual equipping, duplicates, armor swaps, gameplay character unlocks, saves, 116 sprite gutters");
        CheckReachability();File.WriteAllText(Path.Combine(Root,"unity-validation.txt"),"PASS: 1800 puzzle solutions; one-attempt rewards; shuffled answers; 0.5% Ultra; upgrade rules; all generated GPUs, rooftop landings and gaps across 60 unique routes using Unity Physics2D; 116 isolated sprite frames with clear gutters; independent equipment rarities, duplicates and armor swaps; legacy save migration; Looper starter and gameplay character unlocks; safe boxes, checkpoints, enemies and terminals; XP leveling, talent purchases, weapon counters, armor weaknesses, save roundtrip.\n");
        Debug.Log("AN_VALIDATION_PASS / puzzles, drops, upgrades, collection, sprite gutters, 60 levels, Unity Physics2D reachability");
    }
    static string ExpectedPuzzle(LogicPuzzle p){
        if(p.family<6)return Expected(p.question);var n=Regex.Matches(p.question,"\\d+").Cast<Match>().Select(m=>int.Parse(m.Value)).ToArray();
        if(p.family==6){var o=n.Skip(n.Length-4).ToArray();int a=o[0],b=a^o[1],c=b^o[2],d=c^o[3];return (a+b*2+c*4+d*8).ToString();}
        if(p.family==7)return ((n[0]+n[1])*n[2]).ToString();if(p.family==8)return n.Sum().ToString();if(p.family==9)return (n[0]*(n[0]+1)/2).ToString();
        if(p.family==10)return Regex.Match(p.question,"([ABC]) has").Groups[1].Value;return (n[n.Length-1]/4).ToString();
    }
    static string Expected(string q){var numbers=Regex.Matches(q,"\\d+").Cast<Match>().Select(m=>int.Parse(m.Value)).ToList();
        if(q.StartsWith("Sequence:"))return (numbers.Count==4?numbers[3]+numbers[1]-numbers[0]:numbers[2]*(numbers[1]/numbers[0])).ToString();
        if(q.StartsWith("Binary"))return Convert.ToInt32(q.Split(' ')[1],2).ToString();if(q.StartsWith("Even"))return numbers[0]%2==0?"PORT A":"PORT B";if(q.StartsWith("Need"))return "B + C";
        int a=numbers[numbers.Count-2],b=numbers[numbers.Count-1];return q.StartsWith("AND")?(a==1&&b==1?"1":"0"):q.StartsWith("OR")?(a==1||b==1?"1":"0"):(a!=b?"1":"0");
    }
    static void CheckReachability(){
        var oldGravity=Physics2D.gravity;var oldMode=Physics2D.simulationMode;Physics2D.gravity=new Vector2(0,-550f/16);Physics2D.simulationMode=SimulationMode2D.Script;
        int gpuCount=0,gaps=0,jumpOnly=0;
        try{
            for(int level=0;level<60;level++){
                var rows=CityRoute.Generate(level);var floors=rows.Where(r=>r.kind=="platform"&&r.y==218).OrderBy(r=>r.x).ToList();
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);var physics=scene.GetPhysicsScene2D();
                foreach(var row in rows.Where(r=>r.kind=="platform"||r.kind=="crawl")){
                    var go=new GameObject("Validation platform");SceneManager.MoveGameObjectToScene(go,scene);go.transform.position=GameController.PixelCenter(row.x+row.w/2,row.y+row.h/2);var box=go.AddComponent<BoxCollider2D>();box.size=new Vector2(row.w/16,row.h/16);
                    if(row.h<15){var effector=go.AddComponent<PlatformEffector2D>();effector.useOneWay=true;effector.surfaceArc=180;box.usedByEffector=true;}
                }
                var character=new GameObject("Base jump test");SceneManager.MoveGameObjectToScene(character,scene);var body=character.AddComponent<Rigidbody2D>();body.constraints=RigidbodyConstraints2D.FreezeRotation;var collider=character.AddComponent<BoxCollider2D>();collider.size=new Vector2(12f/16,22f/16);collider.sharedMaterial=new PhysicsMaterial2D("Validation friction"){friction=0,bounciness=0};
                Func<LevelRow,Vector2,float,float,bool> attempt=(source,target,tx,ty)=>{
                    float targetX=target.x*16;int lo=Math.Max((int)source.x,(int)(targetX-tx-90)),hi=Math.Min((int)(source.x+source.w-12),(int)(targetX+tx+90));
                    for(int launch=lo;launch<=hi;launch+=6)for(int dir=-1;dir<=1;dir++){
                        body.position=GameController.PixelCenter(launch+6,source.y-11);body.linearVelocity=new Vector2(dir*91f/16,235f/16);
                        for(int tick=0;tick<80;tick++){physics.Simulate(1f/60);if(Mathf.Abs(body.position.x-target.x)<tx/16&&Mathf.Abs(body.position.y-target.y)<ty/16)return true;}
                    }return false;
                };
                for(int i=0;i<floors.Count-1;i++){
                    var next=floors[i+1];Assert(attempt(floors[i],GameController.PixelCenter(next.x+20,207),12,2),"Uncrossable generated gap / "+level);gaps++;
                }
                float exitX=CityRoute.Width(rows)-89;
                Assert(floors.Any(f=>exitX>=f.x+6&&exitX<=f.x+f.w-6),"Exit has no walkable floor / "+level);
                var reachable=new List<LevelRow>(floors);
                foreach(var roof in rows.Where(r=>r.kind=="platform"&&r.y<218).OrderByDescending(r=>r.y)){
                    bool hit=reachable.Where(r=>r.y>roof.y&&r.y-roof.y<=52&&r.x<roof.x+roof.w+90&&r.x+r.w>roof.x-90).Any(r=>attempt(r,GameController.PixelCenter(roof.x+roof.w/2,roof.y-11),roof.w/2-8,2));
                    Assert(hit,"Unreachable rooftop / "+level+" / "+roof.x);reachable.Add(roof);
                }
                foreach(var gpu in rows.Where(r=>r.kind=="gpu")){
                    collider.size=new Vector2(12f/16,(gpu.h==1?10f:22f)/16);collider.offset=new Vector2(0,gpu.h==1?-6f/16:0);
                    bool hit=reachable.Where(r=>Mathf.Abs(r.y-(gpu.y+6))<80&&r.x<gpu.x+100&&r.x+r.w>gpu.x-100).OrderBy(r=>Mathf.Abs(r.y-(gpu.y+27))).Any(r=>attempt(r,GameController.PixelCenter(gpu.x+6,gpu.y+6),12,15));
                    Assert(hit,"Unreachable generated GPU / "+level+" / "+gpu.x);gpuCount++;
                    if(!reachable.Any(r=>gpu.x+6>=r.x-12&&gpu.x+6<=r.x+r.w+12&&Mathf.Abs((r.y-11)-(gpu.y+6))<=17))jumpOnly++;
                }
                foreach(var row in rows.Where(r=>r.kind=="box"||r.kind=="terminal"||r.kind=="checkpoint"||r.kind=="enemy"))Assert(floors.Any(f=>row.x>=f.x&&row.x+18<=f.x+f.w),"Object off safe ground");
                EditorSceneManager.CloseScene(scene,true);Debug.Log("AN_ROUTE_PASS / "+level);
            }
        }finally{Physics2D.gravity=oldGravity;Physics2D.simulationMode=oldMode;}
        Assert(jumpOnly>=120,"Not enough jump-only GPU placements");Debug.Log("AN_JUMP_GPU_PASS / "+jumpOnly+" airborne pickups requiring a jump without magnet buffs");
        Debug.Log("AN_GENERATED_REACHABILITY_PASS / "+gpuCount+" GPUs / "+gaps+" gaps / 60 generated levels");
    }
    public static void Desktop(){Configure();Validate();Build("ArtificialNegligence-preview/ArtificialNegligence.exe",BuildTarget.StandaloneWindows64);}
    public static void Android(){
        Configure();Validate();
        var assemblies=AppDomain.CurrentDomain.GetAssemblies();var external=assemblies.Select(a=>a.GetType("UnityEditor.Android.AndroidExternalToolsSettings")).FirstOrDefault(t=>t!=null);
        Assert(external!=null,"Unity Android Build Support is not installed");
        var work=Path.GetFullPath(Path.Combine(Root,"../work"));
        if(Directory.Exists(Path.Combine(work,"unity-jdk17"))&&File.Exists(Path.Combine(work,"android-ndk-r27c/.complete"))){
            Set(external,"sdkUseEmbedded",false);Set(external,"jdkUseEmbedded",false);Set(external,"ndkUseEmbedded",false);
            Set(external,"sdkRootPath",Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Android/Sdk"));
            string jdk=Directory.GetFiles(Path.Combine(work,"unity-jdk17"),"javac.exe",SearchOption.AllDirectories).Select(Path.GetDirectoryName).Select(Path.GetDirectoryName).First();
            Set(external,"jdkRootPath",jdk);Set(external,"ndkRootPath",Path.Combine(work,"android-ndk-r27c/android-ndk-r27c"));
        }else Debug.Log("Using Android tools configured in Unity preferences.");
        string key=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".android/debug.keystore");
        if(File.Exists(key)){PlayerSettings.Android.useCustomKeystore=true;PlayerSettings.Android.keystoreName=key;PlayerSettings.Android.keystorePass="android";PlayerSettings.Android.keyaliasName="androiddebugkey";PlayerSettings.Android.keyaliasPass="android";}
        EditorUserBuildSettings.buildAppBundle=false;Build("ArtificialNegligence-Unity.apk",BuildTarget.Android);
    }
    static void Set(Type type,string name,object value){var p=type.GetProperty(name,System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static);if(p!=null&&p.CanWrite)p.SetValue(null,value);else Debug.LogWarning("External tool setting unavailable: "+name);}
    static void Build(string name,BuildTarget target){string output=Path.Combine(Root,name);Directory.CreateDirectory(Path.GetDirectoryName(output));var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/ArtificialNegligence.unity"},locationPathName=output,target=target,options=BuildOptions.None});Assert(report.summary.result==BuildResult.Succeeded,"Build failed: "+report.summary.result);Debug.Log("AN_BUILD_PASS / "+output+" / "+report.summary.totalSize+" bytes");}
}




