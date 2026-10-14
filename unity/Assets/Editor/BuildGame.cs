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
        PlayerSettings.companyName="Artificial Negligence";PlayerSettings.productName="Artificial Negligence";PlayerSettings.bundleVersion="1.3.0";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.artificialnegligence.game");PlayerSettings.Android.bundleVersionCode=130;
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;PlayerSettings.allowedAutorotateToPortrait=false;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
        PlayerSettings.defaultScreenWidth=960;PlayerSettings.defaultScreenHeight=540;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=false;PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.runInBackground=true;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.OpenGLES3});
        PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevel35;PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.Android,Il2CppCompilerConfiguration.Release);
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android,ManagedStrippingLevel.Low);
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown,new[]{Resources.Load<Texture2D>("Art/Logo")},IconKind.Any);
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);new GameObject("Artificial Negligence").AddComponent<GameController>();
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/ArtificialNegligence.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/ArtificialNegligence.unity",true)};
        EditorUtility.SetDirty(pipeline);AssetDatabase.SaveAssets();
        Debug.Log("AN_CONFIGURE_PASS / Unity URP 2D, mobile UI, logo, ARM64");
    }
    public static void Validate(){
        Assert(GameplayContent.Parse("{\"version\":2,\"enemyHealth\":1,\"bossHealth\":1,\"enemySpeed\":1,\"enemyDamage\":1,\"weaponDamage\":1,\"xp\":1}")!=null,"Gameplay content rejected");
        Assert(GameplayContent.Parse("{\"version\":2,\"enemyHealth\":-1}")==null&&GameplayContent.Parse("invalid")==null,"Invalid gameplay content accepted");
        string baseline=Path.GetFullPath(Path.Combine(Root,"../work/updater-baseline.apk"));
        if(File.Exists(baseline)){
            long sum=0;var protocol=new List<string>();foreach(var chunk in UpdateChunks.Read(baseline)){Assert(chunk.data.Length<=UpdateChunks.Max&&chunk.offset==sum,"Chunk offsets invalid");sum+=chunk.data.Length;protocol.Add(chunk.offset+","+chunk.data.Length+","+chunk.hash);}
            Assert(sum==new FileInfo(baseline).Length,"Chunk reconstruction length invalid");File.WriteAllLines(Path.Combine(Path.GetDirectoryName(baseline),"protocol-csharp.txt"),protocol);
        }
        foreach(var name in new[]{"menu","gameplay","upgrades","bosscombat","victory","gameover"})Assert(Resources.Load<AudioClip>("Music/"+name)?.length>0,"Music missing: "+name);
        Assert(MusicDirector.Pick(ScreenMode.Title,false)=="menu"&&MusicDirector.Pick(ScreenMode.Play,false)=="gameplay"&&MusicDirector.Pick(ScreenMode.Play,true)=="bosscombat"&&MusicDirector.Pick(ScreenMode.Shop,false)=="upgrades"&&MusicDirector.Pick(ScreenMode.Gear,true)=="upgrades"&&MusicDirector.Pick(ScreenMode.Clear,false)=="victory"&&MusicDirector.Pick(ScreenMode.Dead,true)=="gameover","Music state mapping failed");
        var progression=new RunStats();progression.GainXp(300);Assert(progression.rank>=3&&progression.talents>=2&&progression.WeaponCount>=2,"XP does not unlock builds");
        progression.points=500;Assert(progression.Train(true)=="WEAPON MASTERY +1"&&progression.power==1,"Talent purchase failed");
        progression.weapon=1;int arcShield=progression.HitDamage(3),arcHeavy=progression.HitDamage(2);Assert(arcShield>arcHeavy,"Arc counters missing");
        progression.weapon=3;Assert(progression.HitDamage(2)>progression.HitDamage(3),"Rail counters missing");
        progression.suit=1;Assert(progression.Incoming(3,true)>progression.Incoming(3,false),"Armor tradeoff missing");
        var restored=JsonUtility.FromJson<RunStats>(JsonUtility.ToJson(progression));Assert(restored.rank==progression.rank&&restored.power==1&&restored.weapon==3,"Progress save failed");
        var tierCounts=new int[6];for(int i=0;i<200;i++)tierCounts[RunStats.RollTier(i)]++;
        Assert(tierCounts.SequenceEqual(new[]{0,120,50,20,9,1}),"Ultra drop is not exactly 0.5 percent");
        int[] positions=new int[3];var across=new HashSet<string>();
        for(int run=0;run<200;run++){
            var factory=new PuzzleFactory(new System.Random(run));var within=new HashSet<string>();
            for(int i=0;i<9;i++){var puzzle=factory.Create(i);Assert(within.Add(puzzle.question),"Repeated puzzle");across.Add(puzzle.question);Assert(puzzle.options.Distinct().Count()==3,"Duplicate options");Assert(puzzle.options[puzzle.correct]==Expected(puzzle.question),"Wrong generated solution");positions[puzzle.correct]++;
                var terminal=new TerminalData{puzzle=puzzle};Assert(!terminal.Answer((puzzle.correct+1)%3)&&terminal.attempted&&!terminal.solved,"Wrong attempt rewarded");Assert(!terminal.Answer(puzzle.correct),"Wrong attempt can retry");
                var good=new TerminalData{puzzle=puzzle};Assert(good.Answer(puzzle.correct)&&!good.Answer(puzzle.correct),"Reward can repeat");
            }
        }
        Assert(across.Count>300&&positions.All(x=>x>400),"Puzzles do not vary");
        var stats=new RunStats{points=500};stats.Upgrade(1);stats.Upgrade(2);Assert(stats.gear[1]==2&&stats.gear[2]==2&&stats.maxHealth==6,"Upgrade logic");stats.gear[1]=4;stats.Upgrade(1);Assert(stats.gear[1]==4,"Ultra can be purchased");
        CheckReachability();File.WriteAllText(Path.Combine(Root,"unity-validation.txt"),"PASS: 1800 puzzle solutions; one-attempt rewards; shuffled answers; 0.5% Ultra; upgrade rules; all 96 GPU pickups and 24 gaps across six layout variants using Unity Physics2D; 90 box anchors and 18 terminals; XP leveling, talent purchases, weapon counters, armor weaknesses, save roundtrip.\n");
        Debug.Log("AN_VALIDATION_PASS / puzzles, drops, upgrades, 3 levels, Unity Physics2D reachability");
    }
    static string Expected(string q){var numbers=Regex.Matches(q,"\\d+").Cast<Match>().Select(m=>int.Parse(m.Value)).ToList();
        if(q.StartsWith("Sequence:"))return (numbers.Count==4?numbers[3]+numbers[1]-numbers[0]:numbers[2]*(numbers[1]/numbers[0])).ToString();
        if(q.StartsWith("Binary"))return Convert.ToInt32(q.Split(' ')[1],2).ToString();if(q.StartsWith("Even"))return numbers[0]%2==0?"PORT A":"PORT B";if(q.StartsWith("Need"))return "B + C";
        int a=numbers[numbers.Count-2],b=numbers[numbers.Count-1];return q.StartsWith("AND")?(a==1&&b==1?"1":"0"):q.StartsWith("OR")?(a==1||b==1?"1":"0"):(a!=b?"1":"0");
    }
    static void CheckReachability(){
        var oldGravity=Physics2D.gravity;var oldMode=Physics2D.simulationMode;Physics2D.gravity=new Vector2(0,-550f/16);Physics2D.simulationMode=SimulationMode2D.Script;
        try{
            string csv=Resources.Load<TextAsset>("Data/levels").text;
            for(int level=0;level<6;level++){
                var rows=LevelRow.Parse(csv,level%3);
                if(level>=3){for(int r=0;r<rows.Count;r++){var row=rows[r];float width=row.kind=="platform"?row.w:row.kind=="gpu"?12:row.kind=="terminal"?18:15;row.x=3120-row.x-width;rows[r]=row;}rows=rows.OrderBy(r=>r.x).ToList();}
                var floors=rows.Where(r=>r.kind=="platform"&&r.y==218).ToList();Assert(rows.Count(r=>r.kind=="gpu")==16,"GPU count");
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);var physics=scene.GetPhysicsScene2D();
                foreach(var row in rows.Where(r=>r.kind=="platform")){
                    var go=new GameObject("Validation platform");SceneManager.MoveGameObjectToScene(go,scene);go.transform.position=GameController.PixelCenter(row.x+row.w/2,row.y+row.h/2);var box=go.AddComponent<BoxCollider2D>();box.size=new Vector2(row.w/16,row.h/16);
                    if(row.h<15){var effector=go.AddComponent<PlatformEffector2D>();effector.useOneWay=true;effector.surfaceArc=180;box.usedByEffector=true;}
                }
                var character=new GameObject("Base movement test");SceneManager.MoveGameObjectToScene(character,scene);var body=character.AddComponent<Rigidbody2D>();body.constraints=RigidbodyConstraints2D.FreezeRotation;var collider=character.AddComponent<BoxCollider2D>();collider.size=new Vector2(12f/16,22f/16);collider.sharedMaterial=new PhysicsMaterial2D("Validation / no friction"){friction=0,bounciness=0};
                foreach(var row in rows.Where(r=>r.kind=="gpu")){
                    bool reachable=false;
                    foreach(var floor in floors){if(reachable)break;int lo=Math.Max((int)floor.x,(int)row.x-88),hi=Math.Min((int)(floor.x+floor.w-14),(int)row.x+88);
                        for(int launch=lo;launch<=hi&&!reachable;launch+=4)for(int dir=-1;dir<=1&&!reachable;dir++){
                            body.position=GameController.PixelCenter(launch+6,207);body.linearVelocity=new Vector2(dir*91f/16,235f/16);
                            for(int tick=0;tick<90;tick++){physics.Simulate(1f/60);var target=GameController.PixelCenter(row.x+6,row.y+6);if(Mathf.Abs(body.position.x-target.x)<12f/16&&Mathf.Abs(body.position.y+1f/16-target.y)<15f/16){reachable=true;break;}}
                        }
                    }
                    Assert(reachable,"Unreachable GPU in Unity physics / level "+level+" at "+row.x+","+row.y);
                }
                for(int i=0;i<floors.Count-1;i++){
                    bool reachable=false;float edge=floors[i].x+floors[i].w,next=floors[i+1].x;
                    for(int launch=(int)edge-28;launch<edge-4&&!reachable;launch+=2){body.position=GameController.PixelCenter(launch+6,207);body.linearVelocity=new Vector2(91f/16,235f/16);for(int tick=0;tick<90;tick++){physics.Simulate(1f/60);if(body.position.x*16>next+14&&Mathf.Abs(body.position.y+207f/16)<.08f){reachable=true;break;}}}
                    Assert(reachable,"Gap cannot be crossed / level "+level);
                }
                foreach(var row in rows.Where(r=>r.kind=="box"||r.kind=="terminal"))Assert(floors.Any(f=>row.x>=f.x&&row.x+18<=f.x+f.w),"Object anchor off ground");
                EditorSceneManager.CloseScene(scene,true);
            }
        }finally{Physics2D.gravity=oldGravity;Physics2D.simulationMode=oldMode;}
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



