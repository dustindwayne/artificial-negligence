using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ArtificialNegligence {
 public class MusicDirector:MonoBehaviour {
  public static readonly string[] GameplayTracks={"gameplay","level99","glitch8bit","electronicpunk"};
  GameController game;readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
  readonly Dictionary<string,float> resume=new Dictionary<string,float>(),listened=new Dictionary<string,float>();
  readonly List<string> playlist=new List<string>();readonly System.Random shuffle=new System.Random();string gameplayTrack;
  AudioSource[] sources;int active;float blend=1;bool bossEngaged;
  public string CurrentTrack {get;private set;}
  public bool IsPlaying {get{return sources!=null&&sources[active].isPlaying;}}
  public float PlaybackSeconds {get{return sources==null?0:sources[active].time;}}
  public static string PickGameplay(int stage,int offset=0){return GameplayTracks[((stage+offset)%GameplayTracks.Length+GameplayTracks.Length)%GameplayTracks.Length];}
  public static float RotationDuration(float length){return Mathf.Max(5,length);}
  static bool IsGameplay(string key){return key!=null&&System.Array.IndexOf(GameplayTracks,key)>=0;}
  public static string Pick(ScreenMode mode,bool boss){
   if(mode==ScreenMode.Narrative||mode==ScreenMode.Story||mode==ScreenMode.Title||mode==ScreenMode.Settings||mode==ScreenMode.CityMap)return "menu";
   if(mode==ScreenMode.Shop||mode==ScreenMode.Gear||mode==ScreenMode.Characters)return "upgrades";
   if(mode==ScreenMode.Clear||mode==ScreenMode.Win)return "victory";
   if(mode==ScreenMode.Dead)return "gameover";
   return boss?"bosscombat":"gameplay";
  }
  public void Initialize(GameController controller){
   game=controller;sources=new AudioSource[2];for(int i=0;i<2;i++){sources[i]=gameObject.AddComponent<AudioSource>();sources[i].playOnAwake=false;sources[i].spatialBlend=0;sources[i].volume=0;}
   foreach(var name in new[]{"menu","gameplay","level99","glitch8bit","electronicpunk","upgrades","bosscombat","victory","gameover"}){var clip=Resources.Load<AudioClip>("Music/"+name);if(clip)clips[name]=clip;else Debug.LogError("Missing music: "+name);}
  }
  public void ResetEncounter(){bossEngaged=false;}
  string NextGameplay(){
   if(playlist.Count==0){playlist.AddRange(GameplayTracks);for(int i=playlist.Count-1;i>0;i--){int j=shuffle.Next(i+1);var t=playlist[i];playlist[i]=playlist[j];playlist[j]=t;}if(playlist[0]==gameplayTrack){var t=playlist[0];playlist[0]=playlist[1];playlist[1]=t;}}
   var next=playlist[0];playlist.RemoveAt(0);return next;
  }
  public void QaEndGameplayTrack(){if(IsGameplay(CurrentTrack))sources[active].time=Mathf.Max(0,sources[active].clip.length-.15f);}

  void Update(){
   if(!game||sources==null)return;
   var boss=game.enemies.FirstOrDefault(e=>e&&(e.IsBoss||e.type==12)&&e.hp>0);if(!boss)bossEngaged=false;else if(game.mode==ScreenMode.Play&&game.playerBody&&Mathf.Abs(boss.transform.position.x-game.playerBody.position.x)<13)bossEngaged=true;
   string desired=Pick(game.mode,bossEngaged);if(desired=="gameplay"){
    if(gameplayTrack==null){gameplayTrack=NextGameplay();}desired=gameplayTrack;
   }
   bool rotating=false;
   if(game.mode==ScreenMode.Play&&CurrentTrack==desired&&IsGameplay(desired)){
    float played=listened.TryGetValue(desired,out var seconds)?seconds:0;played+=Time.unscaledDeltaTime;listened[desired]=played;
    if(played>=RotationDuration(sources[active].clip.length)-.8f||sources[active].time>=sources[active].clip.length-.8f||!sources[active].isPlaying){gameplayTrack=NextGameplay();desired=gameplayTrack;resume[desired]=0;listened[desired]=0;rotating=true;}
   }
   if(desired!=CurrentTrack&&clips.TryGetValue(desired,out var clip)){
    if(IsGameplay(CurrentTrack)){resume[CurrentTrack]=rotating?0:sources[active].time;if(rotating)listened[CurrentTrack]=0;}
    active=1-active;var incoming=sources[active];incoming.Stop();incoming.clip=clip;incoming.loop=!IsGameplay(desired)&&desired!="victory"&&desired!="gameover";incoming.volume=0;incoming.Play();
    if(IsGameplay(desired)&&resume.TryGetValue(desired,out var seconds)&&seconds>0)incoming.time=Mathf.Clamp(seconds,0,Mathf.Max(0,clip.length-.1f));blend=0;CurrentTrack=desired;
   }
   blend=Mathf.Min(1,blend+Time.unscaledDeltaTime/.8f);float volume=!game.soundOn?0:game.mode==ScreenMode.Pause||game.mode==ScreenMode.Puzzle||game.mode==ScreenMode.PuzzleResult?.1f:.23f;
   sources[active].volume=volume*blend;sources[1-active].volume=volume*(1-blend);if(blend>=1&&sources[1-active].isPlaying)sources[1-active].Stop();
  }
 }
}
