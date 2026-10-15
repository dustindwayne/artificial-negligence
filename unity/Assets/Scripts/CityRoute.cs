using System;
using System.Collections.Generic;
using System.Linq;
namespace ArtificialNegligence {
    public static class CityRoute {
        public const int Stages=30;
        public static readonly string[] Areas={"STREET SETTLEMENT","MARKET ROOFTOPS","TRANSIT WORKS","DATA GARDENS","CORPORATE SPIRE"};
        static readonly string[] Routes={"Salvage streets","Rooftop sprint","Swarm crossing","Skyline ascent","Armored perimeter","Boss uplink"};
        public static int Area(int stage){return (stage%Stages)/6;}
        public static bool Boss(int stage){return stage%6==5;}
        public static string Name(int stage){return Areas[Area(stage)]+" / "+Routes[stage%6];}
        public static List<LevelRow> Generate(int stage){
            var rng=new Random(17731+stage*997);var rows=new List<LevelRow>();
            Action<string,float,float,float,float> add=(kind,x,y,w,h)=>rows.Add(new LevelRow{level=stage,kind=kind,x=x,y=y,w=w,h=h});
            int motif=stage%6;int count=Boss(stage)?8:7+(stage/6+motif)%4;float cursor=0;
            for(int i=0;i<count;i++){
                int width=rng.Next(motif==1?245:270,motif==0?371:341),roofWidth=rng.Next(motif==1?64:80,motif==4?150:132),roofX=rng.Next(34,65),roofY=rng.Next(motif==3?172:motif==1?174:178,motif==4?195:190);
                if(i==count-1)width=380;
                add("platform",cursor,218,width,40);
                add("platform",cursor+roofX,roofY,roofWidth,9);
                // Upper roofs overlap the first landing and remain within a base jump's vertical reach.
                int upperX=roofX+roofWidth-rng.Next(30,48),upperY=roofY-rng.Next(25,39);
                int upperWidth=rng.Next(60,96);add("platform",cursor+upperX,upperY,upperWidth,motif==2?13:9);
                if(motif==3&&i%2==0){int highX=upperX+upperWidth-34,highY=upperY-28;add("platform",cursor+highX,highY,64,8);add("gpu",cursor+highX+25,highY-28,0,0);}
                add("gpu",cursor+roofX+roofWidth/2-6,roofY-27,0,0);
                add("gpu",cursor+upperX+38,upperY-47,0,0);
                add("gpu",cursor+width-(i%3==1?140:55),i%2==0?169:193,0,0);
                if(i%3==1){add("crawl",cursor+width-90,188,64,18);add("gpu",cursor+width-64,207,0,1);}
                add("box",cursor+(i==0?100:24),200,0,0);if(motif==0||i%2==0)add("box",cursor+width-90,200,0,0);
                int enemy=motif==2?(i%2==0?4:0):motif==4?(i%3==0?3:2):motif==3?(i%2==0?5:1):(i+stage/6)%Math.Min(6,3+stage);
                if(i>0&&(i+stage)%3==1)enemy=9+(stage+i-1)%3;
                if(stage%30==4&&i==count-1)enemy=12;
                if(i>0&&(motif!=0||i%2==1))add("enemy",cursor+width-110,200,enemy,0);
                if(i%3==1||motif==0&&i%3==2)add("terminal",cursor+18,198,0,0);
                if(i%3==0)add("checkpoint",cursor+12,198,0,0);
                if(i>1&&i<count-1&&(i%2==0||motif==1))add("hazard",cursor+width-150,209,22,8);
                if(i%3==2)add("lift",cursor+width-105,166,56,9);
                int gap=i==count-1?0:rng.Next(motif==1?40:28,47);
                if(gap>0&&i%2==0)add("gpu",cursor+width+gap/2-6,166,0,0);
                cursor+=width+gap;
            }
            return rows;
        }
        public static float Width(List<LevelRow> rows){return rows.Where(r=>r.kind=="platform").Max(r=>r.x+r.w);}
    }
}
