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
        PlayerSettings.companyName="Artificial Negligence";PlayerSettings.productName="Artificial Negligence";PlayerSettings.bundleVersion="2.1.1";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.artificialnegligence.game");PlayerSettings.Android.bundleVersionCode=211;
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
            for(int i=0;i<9;i++){var puzzle=factory.Create(i);Assert(within.Add(puzzle.question),"Repeated puzzle");across.Add(puzzle.question);Assert(puzzle.options.Distinct().Count()==3,"Duplicate options");Assert(puzzle.options[puzzle.correct]==ExpectedPuzzle(puzzle),"Wrong generated solution");positions[puzzle.correct]++;
                int correct=puzzle.circuit?puzzle.circuitTarget:puzzle.correct;int wrong=puzzle.circuit?(correct+1)%16:(correct+1)%3;
                var terminal=new TerminalData{puzzle=puzzle};Assert(!terminal.Answer(wrong)&&terminal.attempted&&!terminal.solved,"Wrong attempt rewarded");Assert(!terminal.Answer(correct),"Wrong attempt can retry");
                var good=new TerminalData{puzzle=puzzle};Assert(good.Answer(correct)&&!good.Answer(correct),"Reward can repeat");
            }
        }
        Assert(across.Count>300&&positions.All(x=>x>400),"Puzzles do not vary");
        var stats=new RunStats{points=500};stats.Upgrade(1);stats.Upgrade(2);Assert(stats.gear[1]==2&&stats.gear[2]==2&&stats.maxHealth==6,"Upgrade logic");stats.gear[1]=4;stats.Upgrade(1);Assert(stats.gear[1]==4,"Ultra can be purchased");
        CheckReachability();File.WriteAllText(Path.Combine(Root,"unity-validation.txt"),"PASS: 1800 puzzle solutions; one-attempt rewards; shuffled answers; 0.5% Ultra; upgrade rules; all generated GPUs, rooftop landings and gaps across 30 unique routes using Unity Physics2D; safe boxes, checkpoints, enemies and terminals; XP leveling, talent purchases, weapon counters, armor weaknesses, save roundtrip.\n");
        Debug.Log("AN_VALIDATION_PASS / puzzles, drops, upgrades, 3 levels, Unity Physics2D reachability");
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
        int gpuCount=0,gaps=0;
        try{
            for(int level=0;level<30;level++){
                var rows=CityRoute.Generate(level);var floors=rows.Where(r=>r.kind=="platform"&&r.y==218).OrderBy(r=>r.x).ToList();
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);var physics=scene.GetPhysicsScene2D();
                foreach(var row in rows.Where(r=>r.kind=="platform")){
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
                var reachable=new List<LevelRow>(floors);
                foreach(var roof in rows.Where(r=>r.kind=="platform"&&r.y<218).OrderByDescending(r=>r.y)){
                    bool hit=reachable.Where(r=>r.y>roof.y&&r.y-roof.y<=52&&r.x<roof.x+roof.w+90&&r.x+r.w>roof.x-90).Any(r=>attempt(r,GameController.PixelCenter(roof.x+roof.w/2,roof.y-11),roof.w/2-8,2));
                    Assert(hit,"Unreachable rooftop / "+level+" / "+roof.x);reachable.Add(roof);
                }
                foreach(var gpu in rows.Where(r=>r.kind=="gpu")){
                    bool hit=reachable.Where(r=>Mathf.Abs(r.y-(gpu.y+6))<80&&r.x<gpu.x+100&&r.x+r.w>gpu.x-100).OrderBy(r=>Mathf.Abs(r.y-(gpu.y+27))).Any(r=>attempt(r,GameController.PixelCenter(gpu.x+6,gpu.y+6),12,15));
                    Assert(hit,"Unreachable generated GPU / "+level+" / "+gpu.x);gpuCount++;
                }
                foreach(var row in rows.Where(r=>r.kind=="box"||r.kind=="terminal"||r.kind=="checkpoint"||r.kind=="enemy"))Assert(floors.Any(f=>row.x>=f.x&&row.x+18<=f.x+f.w),"Object off safe ground");
                EditorSceneManager.CloseScene(scene,true);Debug.Log("AN_ROUTE_PASS / "+level);
            }
        }finally{Physics2D.gravity=oldGravity;Physics2D.simulationMode=oldMode;}
        Debug.Log("AN_GENERATED_REACHABILITY_PASS / "+gpuCount+" GPUs / "+gaps+" gaps / 30 generated levels");
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




