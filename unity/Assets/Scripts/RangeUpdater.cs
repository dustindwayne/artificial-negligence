using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ArtificialNegligence {
    public static class RangeProtocol {
        public const int Parallel=4, MaxRange=1048576;
        public class Range {public long start;public int length;public List<int> chunks=new List<int>();public int attempts;public UnityWebRequest request;}
        public static bool SafeUrl(string url){Uri uri;return Uri.TryCreate(url,UriKind.Absolute,out uri)&&uri.Scheme=="https"&&uri.Host=="github.com"&&uri.AbsolutePath.StartsWith("/dustindwayne/artificial-negligence/releases/download/",StringComparison.Ordinal)&&string.IsNullOrEmpty(uri.UserInfo);}
        public static List<Range> Plan(GameUpdater.Chunk[] chunks,HashSet<string> reused){
            var jobs=new List<Range>();long offset=0;Range current=null;
            for(int i=0;i<chunks.Length;i++){var chunk=chunks[i];if(!reused.Contains(chunk.sha256)){
                if(current==null||current.start+current.length!=offset||current.length+chunk.size>MaxRange){current=new Range{start=offset};jobs.Add(current);}
                current.length+=chunk.size;current.chunks.Add(i);
            }offset+=chunk.size;}return jobs;
        }
    }
    // Cap range responses before buffering a server's unintended full APK response.
    public class RangeBuffer:DownloadHandlerScript {
        readonly byte[] bytes;int count;
        public RangeBuffer(int length):base(new byte[65536]){bytes=new byte[length];}
        protected override bool ReceiveData(byte[] data,int length){if(data==null||length>bytes.Length-count)return false;Buffer.BlockCopy(data,0,bytes,count,length);count+=length;return true;}
        protected override byte[] GetData(){return count==bytes.Length?bytes:null;}
    }
    public partial class GameUpdater {
        [Serializable] class IndexRow {public string hash;public long offset;public int size;}
        [Serializable] class IndexCache {public string key;public IndexRow[] rows;}
        long scannedBytes;
        Task<Dictionary<string,Local>> Index(string source,string path,string key){
            try{var saved=JsonUtility.FromJson<IndexCache>(File.ReadAllText(path));if(saved!=null&&saved.key==key&&saved.rows!=null)return Task.FromResult(saved.rows.ToDictionary(x=>x.hash,x=>new Local{offset=x.offset,size=x.size}));}catch{}
            return Task.Run(()=>{var index=new Dictionary<string,Local>();foreach(var block in UpdateChunks.Read(source)){index[block.hash]=new Local{offset=block.offset,size=block.data.Length};Interlocked.Exchange(ref scannedBytes,block.offset+block.data.Length);}return index;});
        }
        IEnumerator QaUpdate(string manifest){
            yield return null;available=JsonUtility.FromJson<Manifest>(File.ReadAllText(manifest));if(!Valid(available))throw new Exception("QA manifest invalid");
#if UNITY_STANDALONE || UNITY_EDITOR
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-qa-update-url");
            if(at>=0&&at+1<args.Length){var uri=new Uri(args[at+1]);if(uri.Scheme!="http"||uri.Host!="127.0.0.1")throw new Exception("QA server must be loopback");available.downloadUrl=uri.AbsoluteUri;}
#endif
            yield return Download();if(ready==null)throw new Exception("QA update failed: "+Status);Debug.Log("AN_RANGE_UPDATE_PASS / "+Status);Application.Quit(0);
        }
        IEnumerator DownloadRanges(){
            busy=true;string source=null;try{source=InstalledApk();}catch{}
            if(string.IsNullOrEmpty(source)){Fail("Install updates on Android, or download the APK from GitHub Releases.");yield break;}
            string dir=UpdateDirectory,cache=Path.Combine(dir,"chunks"),target=Path.Combine(dir,"latest.apk"),part=target+".part",indexPath=Path.Combine(dir,"installed-index.json");
            string key=null;try{Directory.CreateDirectory(cache);var info=new FileInfo(source);key=InstalledVersion()+":"+info.Length+":"+info.LastWriteTimeUtc.Ticks;}catch{Fail("Update storage is unavailable.");yield break;}
            Interlocked.Exchange(ref scannedBytes,0);var scan=Index(source,indexPath,key);
            while(!scan.IsCompleted){Status="Finding reusable game data... "+(Interlocked.Read(ref scannedBytes)/1000000f).ToString("0.0")+" MB";yield return null;}
            if(scan.IsFaulted||scan.IsCanceled){Fail("Could not read installed game data.");yield break;}var local=scan.Result;
            try{File.WriteAllText(indexPath,JsonUtility.ToJson(new IndexCache{key=key,rows=local.Select(x=>new IndexRow{hash=x.Key,offset=x.Value.offset,size=x.Value.size}).ToArray()}));}catch{}
            // One cache generation: obsolete downloaded chunks cannot accumulate.
            var retained=new HashSet<string>(available.chunks.Select(c=>c.sha256));
            try{foreach(var file in Directory.GetFiles(cache,"*.bin"))if(!retained.Contains(Path.GetFileNameWithoutExtension(file)))File.Delete(file);}catch{}
            var usable=new HashSet<string>();long reused=0,offset=0;FileStream input=null,output=null;
            try{input=File.OpenRead(source);output=File.Create(part);output.SetLength(available.size);}catch{input?.Dispose();output?.Dispose();Fail("Not enough free space for the update.");yield break;}
            var active=new List<RangeProtocol.Range>();bool fallback=false,failed=false;long received=0;
            try{
                foreach(var chunk in available.chunks){byte[] bytes=null;string saved=Path.Combine(cache,chunk.sha256+".bin");
                    try{Local found;if(local.TryGetValue(chunk.sha256,out found)&&found.size==chunk.size){input.Position=found.offset;bytes=new byte[found.size];int read=0;while(read<bytes.Length){int n=input.Read(bytes,read,bytes.Length-read);if(n==0)throw new IOException();read+=n;}}
                        else if(File.Exists(saved))bytes=File.ReadAllBytes(saved);
                        if(bytes!=null&&(bytes.Length!=chunk.size||UpdateChunks.Hash(bytes)!=chunk.sha256))bytes=null;
                        if(bytes!=null){output.Position=offset;output.Write(bytes,0,bytes.Length);usable.Add(chunk.sha256);reused+=chunk.size;}
                    }catch{bytes=null;}offset+=chunk.size;
                }
                var queue=new Queue<RangeProtocol.Range>(RangeProtocol.Plan(available.chunks,usable));long needed=queue.Sum(x=>(long)x.length);int completed=0,totalJobs=queue.Count;Debug.Log("AN_RANGE_PLAN / requests "+totalJobs+" / missing "+needed+" / reused "+reused);
                while(queue.Count>0||active.Count>0){
                    while(queue.Count>0&&active.Count<RangeProtocol.Parallel){var job=queue.Dequeue();job.attempts++;job.request=UnityWebRequest.Get(available.downloadUrl);job.request.downloadHandler=new RangeBuffer(job.length);job.request.SetRequestHeader("Range","bytes="+job.start+"-"+(job.start+job.length-1));job.request.timeout=45;job.request.SendWebRequest();active.Add(job);}
                    bool any=false;
                    for(int j=active.Count-1;j>=0;j--){var job=active[j];var request=job.request;if(!request.isDone)continue;any=true;
                        if(request.responseCode==200){fallback=true;break;}
                        bool ok=request.result==UnityWebRequest.Result.Success&&request.responseCode==206&&request.GetResponseHeader("Content-Range")=="bytes "+job.start+"-"+(job.start+job.length-1)+"/"+available.size;
                        var bytes=ok?request.downloadHandler.data:null;ok=ok&&bytes!=null&&bytes.Length==job.length;
                        if(ok){int cursor=0;foreach(int n in job.chunks){var chunk=available.chunks[n];var block=new byte[chunk.size];Buffer.BlockCopy(bytes,cursor,block,0,chunk.size);cursor+=chunk.size;if(UpdateChunks.Hash(block)!=chunk.sha256){ok=false;break;}}}
                        if(ok){try{output.Position=job.start;output.Write(bytes,0,bytes.Length);int cursor=0;foreach(int n in job.chunks){var chunk=available.chunks[n];var block=new byte[chunk.size];Buffer.BlockCopy(bytes,cursor,block,0,chunk.size);cursor+=chunk.size;File.WriteAllBytes(Path.Combine(cache,chunk.sha256+".bin"),block);}received+=bytes.Length;completed++;}catch{failed=true;}}
                        long response=request.responseCode;request.Dispose();active.RemoveAt(j);
                        if(!ok){if(job.attempts<3)queue.Enqueue(job);else{fallback=true;Debug.LogWarning("AN_UPDATE_RANGE / HTTP "+response+"; switching to verified full download");}}
                    }
                    if(fallback||failed)break;
                    long inFlight=active.Sum(x=>(long)x.request.downloadedBytes);
                    Status="Downloading "+((received+inFlight)/1000000f).ToString("0.0")+" / "+(needed/1000000f).ToString("0.0")+" MB / reused "+(reused/1000000f).ToString("0.0")+" MB";
                    if(!any)yield return null;
                }
            }finally{foreach(var job in active){job.request.Abort();job.request.Dispose();}input.Dispose();output.Dispose();}
            if(failed){Fail("Download interrupted. Retry to reuse verified data, or check for a newer update.");yield break;}
            if(fallback){
                Status="This connection needs a full download. Downloading safely...";
                bool downloaded=false;string reason="";
                for(int attempt=0;attempt<3&&!downloaded;attempt++){
                    if(attempt>0)yield return new WaitForSecondsRealtime(attempt*2);
                    string url=available.downloadUrl+(available.downloadUrl.Contains("?")?"&":"?")+"update="+DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    using(var request=UnityWebRequest.Get(url)){request.downloadHandler=new DownloadHandlerFile(part);request.timeout=180;request.SendWebRequest();while(!request.isDone){Status="Downloading full update... "+(request.downloadedBytes/1000000f).ToString("0.0")+" MB";yield return null;}downloaded=request.result==UnityWebRequest.Result.Success;reason="HTTP "+request.responseCode+" / "+request.error;}
                }
                if(!downloaded){Fail("Download failed ("+reason+"). Check your connection and retry; verified data is saved.");yield break;}
            }
            var verify=Task.Run(()=>new FileInfo(part).Length==available.size&&UpdateChunks.HashFile(part)==available.sha256);
            while(!verify.IsCompleted){Status="Verifying update...";yield return null;}
            if(verify.IsFaulted||verify.IsCanceled||!verify.Result){Fail("Update verification failed. Please retry.");yield break;}
            try{if(File.Exists(target))File.Delete(target);File.Move(part,target);ready=target;}catch{Fail("Could not finalize the update.");yield break;}
            Status="Update verified. Tap Install Update; your progress is preserved.";busy=false;
        }
    }
}
