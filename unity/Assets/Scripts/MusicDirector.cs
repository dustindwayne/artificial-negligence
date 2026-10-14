using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArtificialNegligence {
    // Two streaming sources allow short crossfades without interrupting sound effects.
    public class MusicDirector : MonoBehaviour {
        GameController game;
        readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
        AudioSource[] sources;int active;float blend=1;bool bossEngaged;
        public string CurrentTrack {get;private set;}
        public static string Pick(ScreenMode mode,bool boss){
            if(mode==ScreenMode.Title||mode==ScreenMode.Settings||mode==ScreenMode.CityMap)return "menu";
            if(mode==ScreenMode.Shop||mode==ScreenMode.Gear)return "upgrades";
            if(mode==ScreenMode.Clear||mode==ScreenMode.Win)return "victory";
            if(mode==ScreenMode.Dead)return "gameover";
            return boss?"bosscombat":"gameplay";
        }
        public void Initialize(GameController controller){
            game=controller;sources=new AudioSource[2];
            for(int i=0;i<2;i++){sources[i]=gameObject.AddComponent<AudioSource>();sources[i].playOnAwake=false;sources[i].spatialBlend=0;sources[i].volume=0;}
            foreach(var name in new[]{"menu","gameplay","upgrades","bosscombat","victory","gameover"}){
                var clip=Resources.Load<AudioClip>("Music/"+name);if(clip)clips[name]=clip;else Debug.LogError("Missing music: "+name);
            }
        }
        public void ResetEncounter(){bossEngaged=false;}
        void Update(){
            if(!game||sources==null)return;
            var boss=game.enemies.FirstOrDefault(e=>e&&e.IsBoss&&e.hp>0);
            if(!boss)bossEngaged=false;
            else if(game.mode==ScreenMode.Play&&game.playerBody&&Mathf.Abs(boss.transform.position.x-game.playerBody.position.x)<13)bossEngaged=true;
            string desired=Pick(game.mode,bossEngaged);
            if(desired!=CurrentTrack&&clips.TryGetValue(desired,out var clip)){
                active=1-active;var incoming=sources[active];incoming.Stop();incoming.clip=clip;
                incoming.loop=desired!="victory"&&desired!="gameover";incoming.volume=0;incoming.Play();blend=0;CurrentTrack=desired;
            }
            blend=Mathf.Min(1,blend+Time.unscaledDeltaTime/.7f);
            float volume=!game.soundOn?0:game.mode==ScreenMode.Pause||game.mode==ScreenMode.Puzzle||game.mode==ScreenMode.PuzzleResult?.1f:.23f;
            sources[active].volume=volume*blend;sources[1-active].volume=volume*(1-blend);
            if(blend>=1&&sources[1-active].isPlaying)sources[1-active].Stop();
        }
    }
}
