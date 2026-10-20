using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace ArtificialNegligence {
    public partial class GameController {
        float dashTime,dashCooldown,trailClock,followVelocity,landingTimer;bool dashUsed,wasGrounded;Vector2 checkpointPosition;
        class DepthObject {public Transform transform;public Vector3 start;public float parallax;}
        readonly List<DepthObject> depth=new List<DepthObject>();readonly List<SpriteRenderer> rain=new List<SpriteRenderer>();
        readonly List<SpriteRenderer> checkpoints=new List<SpriteRenderer>();readonly List<SpriteRenderer> hazards=new List<SpriteRenderer>();
        public void StartStage(int index){if(index>stats.cityUnlocked)return;stats.health=stats.maxHealth;LoadLevel(index);SaveProgress();SetMode(ScreenMode.Play);ShowStageStory(index);}
        public void Dash(){if(grounded&&(lowStance||duckHeld||Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow))){Slide();return;}if(mode!=ScreenMode.Play||dashCooldown>0||dashUsed)return;dashTime=.14f;dashCooldown=1.1f;dashUsed=true;flash=.25f;Burst(playerBody.position,Cyan,8);}
        public void BuildSceneDepth(){
            depth.Clear();rain.Clear();checkpoints.Clear();hazards.Clear();checkpointPosition=PixelCenter(30,201);dashTime=dashCooldown=followVelocity=0;dashUsed=false;wasGrounded=false;
            var random=new System.Random(814+level*127);int area=CityRoute.Area(level);
            Color[] accents={Cyan,new Color(.85f,.42f,.75f),Gold,new Color(.4f,.9f,.65f),new Color(.58f,.66f,1)};Color accent=Color.Lerp(accents[area],accents[(area+level%6)%5],(level%6)*.055f);background.color=Color.Lerp(Color.white,accent,.2f);
            for(int layer=0;layer<3;layer++)for(float x=-100;x<worldLength+200;x+=random.Next(100,170)){
                float width=random.Next(42,94),height=random.Next(65,180);var building=SpriteObject("City depth / "+layer,white,PixelCenter(x+width/2,215-height/2),-95+layer*12,false);building.transform.localScale=new Vector3(width/PPU,height/PPU,1);building.color=new Color(.02f+layer*.008f,.075f+layer*.014f,.105f+layer*.015f,.35f+layer*.1f);
                depth.Add(new DepthObject{transform=building.transform,start=building.transform.position,parallax=.12f+layer*.09f});
                for(int row=0;row<3;row++){
                    var windows=SpriteObject("Window lights",white,PixelCenter(x+width/2,200-height+row*18),-94+layer*12,false);windows.color=new Color(accent.r,accent.g,accent.b,.11f+layer*.05f);windows.transform.localScale=new Vector3(width/PPU*.65f,2/PPU,1);depth.Add(new DepthObject{transform=windows.transform,start=windows.transform.position,parallax=.12f+layer*.09f});
                }
            }
            foreach(var floor in rows.Where(r=>r.kind=="platform"&&r.y==218)){
                var front=SpriteObject("Foreground foundation",platformSprite,PixelCenter(floor.x+floor.w/2,248),18,false);front.drawMode=SpriteDrawMode.Tiled;front.size=new Vector2(floor.w/PPU,20/PPU);front.color=new Color(.04f,.11f,.14f);
                for(int i=0;i<3;i++){
                    float x=floor.x+25+i*(floor.w-50)/3;var pole=SpriteObject("Street conduit",white,PixelCenter(x,235),-5,false);pole.transform.localScale=new Vector3(4/PPU,50/PPU,1);pole.color=new Color(.075f,.22f,.25f);
                    var lamp=SpriteObject("Street marker",glowSprite,PixelCenter(x,212),-4,false);lamp.transform.localScale=Vector3.one*.45f;lamp.color=new Color(accent.r,accent.g,accent.b,.32f);
                }
            }
            foreach(var roof in rows.Where(r=>r.kind=="platform"&&r.y<218)){
                float x=roof.x+roof.w*.18f;
                var prop=SpriteObject(area==3?"Garden canopy":"Rooftop relay",white,PixelCenter(x,roof.y-10),-3,false);prop.color=Color.Lerp(accent,Ink,.55f);prop.transform.localScale=new Vector3((area==3?19:9)/PPU,18/PPU,1);
                var sign=SpriteObject("District accent strip",white,PixelCenter(x,roof.y-17),-2,false);sign.color=new Color(accent.r,accent.g,accent.b,.55f);sign.transform.localScale=new Vector3((area==3?23:15)/PPU,3/PPU,1);
            }
            foreach(var row in rows.Where(r=>r.kind=="checkpoint")){
                var marker=SpriteObject("Checkpoint beacon",itemFrames[2],PixelCenter(row.x+8,205),4,false);marker.transform.localScale=Vector3.one*.55f;marker.color=new Color(.45f,.6f,.7f);checkpoints.Add(marker);
            }
            foreach(var row in rows.Where(r=>r.kind=="hazard")){
                var sr=SpriteObject("Telegraphed power vent",white,PixelCenter(row.x+row.w/2,211),4,false);sr.transform.localScale=new Vector3(row.w/PPU,.18f,1);sr.color=Pink;hazards.Add(sr);
                var danger=SpriteObject("Vent warning",glowSprite,sr.transform.position,3,false,sr.transform);danger.transform.localScale=new Vector3(1.2f,.6f,1);danger.color=new Color(1,.2f,.4f,.5f);
            }
            foreach(var row in rows.Where(r=>r.kind=="lift")){
                var sr=SpriteObject("Optional maintenance lift",platformSprite,PixelCenter(row.x+row.w/2,row.y+row.h/2),1,false);sr.drawMode=SpriteDrawMode.Tiled;sr.size=new Vector2(row.w/PPU,row.h/PPU);sr.color=accent;sr.gameObject.layer=8;
                var body=sr.gameObject.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;var box=sr.gameObject.AddComponent<BoxCollider2D>();box.size=sr.size;box.usedByEffector=true;sr.gameObject.AddComponent<PlatformEffector2D>().useOneWay=true;sr.gameObject.AddComponent<MaintenanceLift>();
            }
            for(int i=0;i<30;i++){var drop=SpriteObject("Rain streak",white,Vector2.zero,-10,false);drop.transform.localScale=new Vector3(1/PPU,random.Next(3,9)/PPU,1);drop.color=new Color(.55f,.8f,.9f,.16f);rain.Add(drop);}
            RefreshGraphics(true);
        }
        void TickSceneDepth(){
            if(!worldRoot||!gameCamera)return;float cameraX=gameCamera.transform.position.x;
            foreach(var obj in depth)if(obj.transform)obj.transform.position=obj.start+Vector3.right*(cameraX-15)*obj.parallax;
            for(int i=0;i<rain.Count;i++){rain[i].transform.position=new Vector3(cameraX-16+(i*7.137f-Time.time*1.8f)%33,-2-(i*1.117f+Time.time*5)%12,0);}
            foreach(var hazard in hazards)if(hazard)hazard.color=(Time.time%3<1.1f)?new Color(Pink.r,Pink.g,Pink.b,.32f):Pink;
        }
        void TickPlatformer(float dt){
            dashTime=Mathf.Max(0,dashTime-dt);dashCooldown=Mathf.Max(0,dashCooldown-dt);
            landingTimer=Mathf.Max(0,landingTimer-dt);if(grounded){dashUsed=false;if(!wasGrounded){landingTimer=.14f;Burst(playerBody.position+Vector2.down*.65f,Cream,5);}}wasGrounded=grounded;
            trailClock-=dt;if(dashTime>0&&trailClock<=0){Burst(playerBody.position,Cyan,2);trailClock=.04f;}
            foreach(var cp in checkpoints){if(cp&&Mathf.Abs(cp.transform.position.x-playerBody.position.x)<.7f&&Mathf.Abs(cp.transform.position.y-playerBody.position.y)<1&&cp.color!=Cyan){checkpointPosition=new Vector2(cp.transform.position.x,-201/PPU);cp.color=Cyan;Toast("CHECKPOINT SYNCED");CheckpointDialogue();}}
            if(Time.time%3>=1.1f)foreach(var hazard in hazards)if(hazard&&Mathf.Abs(hazard.transform.position.x-playerBody.position.x)<.75f&&Mathf.Abs(hazard.transform.position.y-playerBody.position.y)<.8f)Damage(1,Mathf.Sign(playerBody.position.x-hazard.transform.position.x),true);
        }
        void BuildCityMap(){
            Image(screenRoot,"Dim city",0,0,960,540,new Color(.003f,.01f,.02f,.87f));var window=Panel(screenRoot,"City route",18,15,924,511,Cyan);
            int page=stats.cityUnlocked/30,offset=page*30;
            Label(window,ActTitle(stats.cityUnlocked),24,15,667,45,36,Cream);Button(window,"MENU",773,15,124,43,Cyan,()=>SetMode(ScreenMode.Title));
            Label(window,StoryObjective(stats.cityUnlocked),24,65,865,29,19,Gold);
            // Street blocks, a canal and a continuous winding route form an actual city overview.
            Image(window,"Canal",620,104,21,305,new Color(.1f,.45f,.55f,.14f));
            var mapRandom=new System.Random(741);for(int r=0;r<8;r++)for(int c=0;c<12;c++){
                float bx=285+c*48,by=105+r*38;var block=Image(window,"City block",bx,by,36,25,new Color(.02f,.13f,.17f,.8f));
                for(int w=0;w<3;w++)Image(window,"Block window",bx+5+w*9,by+6,3,3,new Color(.16f,.43f,.46f,mapRandom.Next(2)==0?.2f:.6f));
            }
            var nodes=new Vector2[30];for(int i=0;i<30;i++){int row=i/6,col=i%6;nodes[i]=new Vector2(305+(row%2==0?col:5-col)*96,126+row*57+(col%2==0?-6:6));}
            for(int i=0;i<29;i++){var from=nodes[i]+new Vector2(31,20);var to=nodes[i+1]+new Vector2(31,20);Vector2 delta=to-from;
                var road=Image(window,"Route connection",from.x,from.y,delta.magnitude,3,i+offset<stats.cityUnlocked?Cyan:new Color(.1f,.35f,.38f));road.rectTransform.pivot=new Vector2(0,.5f);road.rectTransform.localRotation=Quaternion.Euler(0,0,-Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);
            }
            for(int row=0;row<5;row++){
                Label(window,CityRoute.Areas[row],24,114+row*57,253,42,22,Cyan);
                for(int col=0;col<6;col++){
                    int n=row*6+col,index=offset+n;var node=nodes[n];bool open=index<=stats.cityUnlocked;
                    Color accent=!open?(CityRoute.Boss(index)?new Color(.42f,.2f,.31f):new Color(.15f,.31f,.35f)):CityRoute.Boss(index)?Pink:index==stats.cityUnlocked?Gold:Cyan;
                    var button=Button(window,IsVirtualStage(index)?"AI":CityRoute.Boss(index)?"BOSS":(index+1).ToString(),node.x,node.y,63,40,accent,()=>StartStage(index));button.GetComponentInChildren<Text>().fontSize=22;button.interactable=open;
                    if(index==stats.cityUnlocked)Image(window,"Survivor on city route",node.x+18,node.y-27,28,28,Color.white,playerFrames[0]);
                    if(stats.stageStars!=null&&stats.stageStars[index%30]>0)Label(window,new string('*',stats.stageStars[index%30]),node.x,node.y+39,63,16,16,Gold,TextAnchor.MiddleCenter);
                }
            }
            Label(window,"UNLOCKED "+(stats.cityUnlocked+1)+" / LV "+stats.rank+" / "+stats.points+" POINTS",24,435,570,35,23,Cream);
            Label(window,RendererProgress.Progress(stats)+" / ABSORB NEW CORES TO UPGRADE",24,472,690,25,20,Cyan);
            Button(window,"SURVIVOR",24,391,218,38,Cyan,()=>{characterReturn=ScreenMode.CityMap;SetMode(ScreenMode.Characters);});Button(window,"CONTINUE",698,445,197,47,Gold,()=>StartStage(stats.cityUnlocked));
        }
        void BuildCircuit(RectTransform window){
            var puzzle=activeTerminal.data.puzzle;
            Label(window,"MATCH THE TARGET OUTPUTS",32,105,809,34,27,Cream);
            Label(window,"TARGET: "+PuzzleFactory.CircuitOutputs(puzzle.circuitTarget),32,147,809,37,29,Gold);
            Label(window,"OUTPUT: "+PuzzleFactory.CircuitOutputs(puzzle.circuitInputs),32,189,809,37,29,Cyan);
            for(int i=0;i<4;i++){int bit=i;bool on=(puzzle.circuitInputs&(1<<i))!=0;Button(window,((char)('A'+i))+" / "+(on?"ON":"OFF"),35+i*208,240,188,60,on?Gold:Cyan,()=>{puzzle.circuitInputs^=1<<bit;BuildScreen();});}
            Label(window,"1 = A     2 = A XOR B     3 = B XOR C     4 = C XOR D",35,314,811,42,24,Cream);
            Label(window,"XOR lights up when its two inputs differ. One submission.",35,357,811,36,22,Cyan);
            Button(window,"SUBMIT CIRCUIT",240,414,396,44,Gold,()=>AnswerPuzzle(puzzle.circuitInputs));
        }
        void BuildPuzzleResult(RectTransform window){
            var data=activeTerminal.data;var puzzle=data.puzzle;
            Label(window,data.solved?"CIRCUIT RESTORED / POWERUP GRANTED":"ACCESS DENIED / NO POWERUP",35,111,806,60,31,data.solved?Gold:Pink,TextAnchor.MiddleCenter);
            Label(window,puzzle.circuit?"Correct outputs: "+PuzzleFactory.CircuitOutputs(puzzle.circuitTarget):"Answer: "+puzzle.answer,35,181,806,59,28,Cream,TextAnchor.MiddleCenter);
            Label(window,puzzle.explanation,70,260,736,101,27,Cyan,TextAnchor.MiddleCenter);
            Button(window,"BACK TO THE CITY",238,407,400,51,Cyan,()=>SetMode(ScreenMode.Play));
        }
    }
    public class MaintenanceLift:MonoBehaviour {
        Rigidbody2D body;Vector2 start;void Awake(){body=GetComponent<Rigidbody2D>();start=body.position;}
        void FixedUpdate(){if(GameController.Instance.mode==ScreenMode.Play)body.MovePosition(start+Vector2.up*Mathf.Sin(Time.time*1.1f)*.55f);}
    }
}
