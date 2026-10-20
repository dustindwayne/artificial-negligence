using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace ArtificialNegligence {
    public static class UpdateChunks {
        public const int Min=32768,Max=262144;
        public class Block {public long offset;public byte[] data;public string hash;}
        public static string Hash(byte[] bytes){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();}
        public static string HashFile(string path){using(var stream=File.OpenRead(path))using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant();}
        public static IEnumerable<Block> Read(string path){
            using(var file=new BufferedStream(File.OpenRead(path),65536)){
                byte[] buffer=new byte[Max];int count=0;uint rolling=0;long offset=0;int value;
                while((value=file.ReadByte())>=0){buffer[count++]=(byte)value;
                    unchecked {uint gear=((uint)(value+1)*2654435761u)^((uint)(value+17)*2246822519u);rolling=(rolling<<1)+gear;}
                    if(count>=Min&&((rolling&65535)==0||count==Max)){byte[] data=new byte[count];Array.Copy(buffer,data,count);yield return new Block{offset=offset,data=data,hash=Hash(data)};offset+=count;count=0;rolling=0;}
                }
                if(count>0){byte[] data=new byte[count];Array.Copy(buffer,data,count);yield return new Block{offset=offset,data=data,hash=Hash(data)};}
            }
        }
    }
    public partial class GameUpdater:MonoBehaviour {
        const string Root="https://raw.githubusercontent.com/dustindwayne/artificial-negligence/main/apk/";
        const int Version=249;
        [Serializable] public class Chunk {public string sha256;public int size;}
        [Serializable] public class Manifest {public int versionCode,protocol;public string version,sha256,downloadUrl;public long size;public Chunk[] chunks;}
        struct Local {public long offset;public int size;}
        Manifest available;bool busy;string ready;GameController game;
        public string Status {get;private set;}="Check for the latest game update.";
        public string ButtonText {get{return busy?"WORKING...":ready!=null?"INSTALL UPDATE":available!=null?"DOWNLOAD UPDATE":"CHECK FOR UPDATES";}}
        public void Initialize(GameController controller){game=controller;var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-qa-update-manifest");if(at>=0&&at+1<args.Length)StartCoroutine(QaUpdate(args[at+1]));}
        public void Begin(){if(busy)return;if(ready!=null){Install();return;}StartCoroutine(available==null?Check():Download());}
        string UpdateDirectory {get{
#if UNITY_STANDALONE || UNITY_EDITOR
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-qa-update-dir");
            if(Array.IndexOf(args,"-qa-update-manifest")>=0&&at>=0&&at+1<args.Length)return Path.GetFullPath(args[at+1]);
#endif
            return Path.Combine(Application.persistentDataPath,"updates");
        }}
        IEnumerator Check(){
            busy=true;Status="Checking for updates...";string stamp="?check="+DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            using(var contentRequest=UnityWebRequest.Get(Root+"gameplay.json"+stamp))using(var request=UnityWebRequest.Get(Root+"latest-v2.json"+stamp)){
                contentRequest.timeout=15;request.timeout=25;var contentOperation=contentRequest.SendWebRequest();var appOperation=request.SendWebRequest();yield return appOperation;yield return contentOperation;
                bool contentUpdated=contentRequest.result==UnityWebRequest.Result.Success&&GameplayContent.Save(GameplayContent.Parse(contentRequest.downloadHandler.text));
                if(request.result!=UnityWebRequest.Result.Success){Fail("Could not check updates. Try again with internet.");yield break;}
                Manifest manifest=null;try{manifest=JsonUtility.FromJson<Manifest>(request.downloadHandler.text);}catch{}
                if(!Valid(manifest)){Fail("Update information is invalid.");yield break;}
                if(manifest.versionCode<=InstalledVersion()){Status=contentUpdated?"Gameplay balance updated. No APK download needed.":"Your game is up to date ("+Application.version+").";busy=false;yield break;}
                available=manifest;
                string existing=Path.Combine(UpdateDirectory,"latest.apk");
                var verify=System.Threading.Tasks.Task.Run(()=>File.Exists(existing)&&new FileInfo(existing).Length==manifest.size&&UpdateChunks.HashFile(existing)==manifest.sha256);
                while(!verify.IsCompleted){Status="Checking saved update...";yield return null;}
                if(!verify.IsFaulted&&!verify.IsCanceled&&verify.Result){ready=existing;Status="Saved update verified. Tap Install Update.";busy=false;yield break;}
                Status="Version "+manifest.version+" is available. Only missing data will download.";
            }busy=false;
        }
        static bool Valid(Manifest m){
            if(m==null||m.protocol<0||m.protocol>2||m.versionCode<1||m.size<1||m.size>500000000||m.chunks==null||m.chunks.Length<1||m.chunks.Length>16000||!IsHash(m.sha256))return false;
            long sum=0;foreach(var c in m.chunks){if(c==null||!IsHash(c.sha256)||c.size<1||c.size>UpdateChunks.Max)return false;sum+=c.size;}return sum==m.size&&(m.protocol<2||RangeProtocol.SafeUrl(m.downloadUrl));
        }
        static bool IsHash(string hash){return hash!=null&&Regex.IsMatch(hash,"^[a-f0-9]{64}$");}
        void Fail(string text){Status=text;busy=false;Debug.LogWarning("AN_UPDATE / "+text);}
        string InstalledApk(){
#if UNITY_ANDROID && !UNITY_EDITOR
            using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))using(var info=activity.Call<AndroidJavaObject>("getApplicationInfo"))return info.Get<string>("sourceDir");
#else
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-qa-update-source");return at>=0&&at+1<args.Length?args[at+1]:null;
#endif
        }
        int InstalledVersion(){
#if UNITY_ANDROID && !UNITY_EDITOR
            using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))using(var manager=activity.Call<AndroidJavaObject>("getPackageManager"))using(var info=manager.Call<AndroidJavaObject>("getPackageInfo",Application.identifier,0))return info.Get<int>("versionCode");
#else
            return Version;
#endif
        }
        IEnumerator Download(){if(available.protocol>=2)yield return DownloadRanges();else yield return LegacyDownload();}
        IEnumerator LegacyDownload(){
            busy=true;Status="Checking which game files can be reused...";
            string source=null;try{source=InstalledApk();}catch{}
            if(string.IsNullOrEmpty(source)){Fail("Updates install on Android. Use the GitHub APK on desktop.");yield break;}
            var local=new Dictionary<string,Local>();
            var scan=UpdateChunks.Read(source).GetEnumerator();
            while(true){bool more=false;try{more=scan.MoveNext();}catch{scan.Dispose();Fail("Could not read the installed game.");yield break;}
                if(!more)break;var block=scan.Current;local[block.hash]=new Local{offset=block.offset,size=block.data.Length};yield return null;}
            scan.Dispose();
            string dir=UpdateDirectory,cache=Path.Combine(dir,"chunks"),target=Path.Combine(dir,"latest.apk");
            try{Directory.CreateDirectory(cache);}catch{Fail("Not enough space or storage is unavailable.");yield break;}
            long reused=0,received=0,total=0;foreach(var chunk in available.chunks)if(local.ContainsKey(chunk.sha256))reused+=chunk.size;else total+=chunk.size;
            FileStream output=null,input=null;try{output=File.Create(target);input=File.OpenRead(source);}catch{output?.Dispose();input?.Dispose();Fail("Could not prepare update storage.");yield break;}
            bool failed=false;
            foreach(var chunk in available.chunks){
                byte[] bytes=null;string cached=Path.Combine(cache,chunk.sha256+".bin");
                try{if(local.TryGetValue(chunk.sha256,out var found)&&found.size==chunk.size){input.Position=found.offset;bytes=new byte[found.size];int n=0;while(n<bytes.Length){int got=input.Read(bytes,n,bytes.Length-n);if(got==0)throw new IOException();n+=got;}}
                    else if(File.Exists(cached)){var saved=File.ReadAllBytes(cached);if(saved.Length==chunk.size&&UpdateChunks.Hash(saved)==chunk.sha256)bytes=saved;}
                }catch{bytes=null;}
                if(bytes==null){
                    for(int attempt=0;attempt<3&&bytes==null;attempt++){
                        using(var request=UnityWebRequest.Get(Root+"chunks/"+chunk.sha256+".bin")){
                            request.timeout=30;yield return request.SendWebRequest();
                            if(request.result==UnityWebRequest.Result.Success){var data=request.downloadHandler.data;if(data.Length==chunk.size&&UpdateChunks.Hash(data)==chunk.sha256){bytes=data;try{File.WriteAllBytes(cached,data);}catch{}}}
                        }
                    }
                    if(bytes==null){failed=true;break;}received+=bytes.Length;
                }
                try{if(UpdateChunks.Hash(bytes)!=chunk.sha256)throw new IOException();output.Write(bytes,0,bytes.Length);}catch{failed=true;break;}
                Status="Downloading "+(received/1000000f).ToString("0.0")+" / "+(total/1000000f).ToString("0.0")+" MB / reusing "+(reused/1000000f).ToString("0.0")+" MB";yield return null;
            }
            output.Dispose();input.Dispose();
            if(failed){Fail("Download interrupted. Tap Download Update to resume.");yield break;}
            bool verified=false;try{verified=new FileInfo(target).Length==available.size&&UpdateChunks.HashFile(target)==available.sha256;}catch{}
            if(!verified){Fail("Update verification failed. Please retry.");yield break;}
            ready=target;Status="Update verified. Tap Install Update; your progress is preserved.";busy=false;
        }
        void Install(){
            if(ready==null||!File.Exists(ready)){ready=null;Status="Saved update is missing. Tap Download Update again.";return;}
#if UNITY_ANDROID && !UNITY_EDITOR
            try{
                using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity")){
                    using(var manager=activity.Call<AndroidJavaObject>("getPackageManager")){
                        if(!manager.Call<bool>("canRequestPackageInstalls")){
                            using(var uriClass=new AndroidJavaClass("android.net.Uri"))using(var uri=uriClass.CallStatic<AndroidJavaObject>("parse","package:"+Application.identifier))using(var settings=new AndroidJavaObject("android.content.Intent","android.settings.MANAGE_UNKNOWN_APP_SOURCES",uri))activity.Call("startActivity",settings);
                            Status="Allow game updates in Android settings, then tap Install again.";return;
                        }
                    }
                    using(var provider=new AndroidJavaClass("androidx.core.content.FileProvider"))using(var file=new AndroidJavaObject("java.io.File",ready))using(var uri=provider.CallStatic<AndroidJavaObject>("getUriForFile",activity,Application.identifier+".updates",file))using(var intent=new AndroidJavaObject("android.content.Intent","android.intent.action.VIEW")){
                        intent.Call<AndroidJavaObject>("setDataAndType",uri,"application/vnd.android.package-archive");intent.Call<AndroidJavaObject>("addFlags",1);
                        using(var clipClass=new AndroidJavaClass("android.content.ClipData"))using(var clip=clipClass.CallStatic<AndroidJavaObject>("newRawUri","Game update",uri))intent.Call("setClipData",clip);
                        activity.Call("startActivity",intent);Status="Android installer opened. Confirm Update to finish.";
                    }
                }
            }catch(Exception error){Debug.LogError("AN_UPDATE_INSTALL / "+error);Status="Android could not open the installer. Check Allow from this source in Settings, then retry. Your verified download is saved.";}
#else
            Status="Install updates on your Android device.";
#endif
        }
    }
}
