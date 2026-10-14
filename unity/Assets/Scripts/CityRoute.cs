using System;
using System.Collections.Generic;
using System.Linq;
namespace ArtificialNegligence {
    public static class CityRoute {
        public const int Stages=30;
        public static readonly string[] Areas={"STREET SETTLEMENT","MARKET ROOFTOPS","TRANSIT WORKS","DATA GARDENS","CORPORATE SPIRE"};
        static readonly string[] Routes={"Backstreet ascent","Relay rooftops","Maintenance crossing","Upper city passage","Security perimeter","Boss uplink"};
        public static int Area(int stage){return (stage%Stages)/6;}
        public static bool Boss(int stage){return stage%6==5;}
        public static string Name(int stage){return Areas[Area(stage)]+" / "+Routes[stage%6];}
        public static List<LevelRow> Generate(int stage){
            var rng=new Random(17731+stage*997);var rows=new List<LevelRow>();
            Action<string,float,float,float,float> add=(kind,x,y,w,h)=>rows.Add(new LevelRow{level=stage,kind=kind,x=x,y=y,w=w,h=h});
            int count=Boss(stage)?8:7+stage%3;float cursor=0;
            for(int i=0;i<count;i++){
                int width=rng.Next(260,341),roofWidth=rng.Next(90,131),roofX=rng.Next(48,71),roofY=rng.Next(176,184);
                if(i==count-1)width=380;
                add("platform",cursor,218,width,40);
                add("platform",cursor+roofX,roofY,roofWidth,9);
                // Upper roofs overlap the first landing and remain within a base jump's vertical reach.
                int upperX=roofX+roofWidth-42,upperY=roofY-rng.Next(30,37);
                add("platform",cursor+upperX,upperY,85,9);
                add("gpu",cursor+roofX+roofWidth/2-6,roofY-27,0,0);
                add("gpu",cursor+upperX+38,upperY-27,0,0);
                add("gpu",cursor+width-55,193,0,0);
                add("box",cursor+24,200,0,0);add("box",cursor+width-90,200,0,0);
                if(i>0)add("enemy",cursor+width-110,200,(i+stage)%6,0);
                if(i%3==1)add("terminal",cursor+18,198,0,0);
                if(i%3==0)add("checkpoint",cursor+12,198,0,0);
                if(i>1&&i<count-1&&i%2==0)add("hazard",cursor+width-150,209,22,8);
                if(i%3==2)add("lift",cursor+width-105,166,56,9);
                cursor+=width+(i==count-1?0:rng.Next(34,47));
            }
            return rows;
        }
        public static float Width(List<LevelRow> rows){return rows.Where(r=>r.kind=="platform").Max(r=>r.x+r.w);}
    }
}
