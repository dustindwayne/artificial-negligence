using System;
using System.Linq;
using UnityEngine;
namespace ArtificialNegligence {
    // Frames are fitted as a group: transparent gutters, shared scale and feet baseline.
    [Serializable] public class AtlasFrame {public int x,y,width,height;public RectInt Rect {get{return new RectInt(x,y,width,height);}}}
    [Serializable] public class AtlasLayout {public AtlasFrame[] frames;}
    public static class SpriteAtlas {
        public static Sprite[] Load(string name,int columns,int rows,int width,int height,float ppu,bool grounded,int gutter=2){
            var source=Resources.Load<Texture2D>("Art/"+name);if(!source)throw new Exception("Missing atlas: "+name);
            var layout=Resources.Load<TextAsset>("Art/"+name+"-layout");var mask=Resources.Load<TextAsset>("Art/"+name+"-mask");
            return Fit(source,columns,rows,width,height,ppu,grounded,layout?JsonUtility.FromJson<AtlasLayout>(layout.text).frames.Select(f=>f.Rect).ToArray():null,mask?mask.bytes:null,gutter);
        }
        public static Sprite[] Fit(Texture2D source,int columns,int rows,int width,int height,float ppu,bool grounded,RectInt[] measured=null,byte[] mask=null,int gutter=2){
            var pixels=source.GetPixels32();var result=new Sprite[columns*rows];var bounds=new RectInt[result.Length];
            for(int row=0;row<rows;row++)for(int col=0;col<columns;col++){
                int x0=col*source.width/columns,x1=(col+1)*source.width/columns,y0=source.height-(row+1)*source.height/rows,y1=source.height-row*source.height/rows;
                int minX=x1,minY=y1,maxX=x0-1,maxY=y0-1;
                for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)if(pixels[y*source.width+x].a>24){minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);minY=Math.Min(minY,y);maxY=Math.Max(maxY,y);}
                if(maxX<minX)throw new Exception("Empty atlas frame "+source.name+" / "+row+":"+col);
                bounds[row*columns+col]=new RectInt(minX,minY,maxX-minX+1,maxY-minY+1);
            }
            if(measured!=null){if(measured.Length!=result.Length||mask==null||mask.Length!=pixels.Length)throw new Exception("Atlas metadata mismatch "+source.name);bounds=measured;}
            int group=(source.name=="looper"||source.name=="wonder-wrath"||source.name=="davey-goats"||source.name=="brad-jammin"||source.name=="mikey-redd"||source.name=="prison-mike")?8:source.name=="items-v3"?1:columns;
            for(int first=0;first<result.Length;first+=group){
                float scale=float.MaxValue;for(int col=0;col<group;col++){var b=bounds[first+col];scale=Math.Min(scale,Math.Min((width-gutter*2f)/b.width,(height-gutter*2f)/b.height));}
                for(int col=0;col<group;col++){
                    var b=bounds[first+col];int w=Math.Max(1,Mathf.RoundToInt(b.width*scale)),h=Math.Max(1,Mathf.RoundToInt(b.height*scale));
                    int offsetX=(width-w)/2,offsetY=grounded?gutter:(height-h)/2;var output=new Color32[width*height];
                    for(int y=0;y<h;y++)for(int x=0;x<w;x++){int sx=b.x+Math.Min(b.width-1,(int)(x/scale)),sy=b.y+Math.Min(b.height-1,(int)(y/scale));int at=sy*source.width+sx;if(mask==null||mask[at]==first+col+1)output[(offsetY+y)*width+offsetX+x]=pixels[at];}
                    var texture=new Texture2D(width,height,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,name=source.name+"/"+(first+col)};texture.SetPixels32(output);texture.Apply();
                    result[first+col]=Sprite.Create(texture,new Rect(0,0,width,height),Vector2.one*.5f,ppu,0,SpriteMeshType.FullRect);result[first+col].name=texture.name;
                }
            }
            return result;
        }
    }
    public partial class GameController {
        Sprite[] looperFrames,wrathFrames;Sprite[][] styledCharacters=new Sprite[RunStats.Characters.Length*4][];
        public void InitializeSpriteSheets(){
            looperFrames=SpriteAtlas.Load("looper",4,2,64,64,PPU*2,true,4);
            wrathFrames=SpriteAtlas.Load("ai-wrath",4,2,64,64,32,true,4);
            enemyFrames=SpriteAtlas.Load("enemies-v3",4,9,192,120,60,true,3);
            var agents=SpriteAtlas.Load("sinister-agents",4,3,64,64,32,true,4);enemyFrames=enemyFrames.Concat(agents).Concat(new[]{wrathFrames[0],wrathFrames[1],wrathFrames[2],wrathFrames[5]}).ToArray();
            itemFrames=SpriteAtlas.Load("items-v3",4,4,64,64,PPU*2,false,4);
            shipFrames=SpriteAtlas.Load("ai_ships",4,1,128,64,PPU,false);InitializeWonderWrath();InitializeGuests();ApplyCharacter();
        }
        public void ApplyCharacter(){
            ApplyGuestUnlocks();stats.EnsureCollection();if(stats.character==4&&wonderWrathUnlocked&&wonderWrathEquipped){playerFrames=wonderWrathFrames;if(playerVisual)playerVisual.sprite=playerFrames[0];return;}int id=stats.character*4+stats.outfit;
            if(styledCharacters[id]==null){
                var frames=new Sprite[8];Color[] accents={Color.white,Cyan,Gold,new Color(.75f,.4f,1)};
                for(int i=0;i<8;i++){
                    var original=CharacterFrame(stats.character,i);if(stats.outfit==0){frames[i]=original;continue;}
                    var src=original.texture;var colors=src.GetPixels32();
                    for(int p=0;p<colors.Length;p++){Color c=colors[p];float hue,sat,value;Color.RGBToHSV(c,out hue,out sat,out value);
                        // Recolor cloth accent pixels; keep warm skin, cream details and outlines intact.
                        if(c.a>.1f&&sat>.3f&&value>.25f&&hue>.35f&&hue<.82f){Color target=accents[stats.outfit]*value;target.a=c.a;colors[p]=target;}}
                    var t=new Texture2D(src.width,src.height,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};t.SetPixels32(colors);t.Apply();frames[i]=Sprite.Create(t,new Rect(0,0,src.width,src.height),Vector2.one*.5f,original.pixelsPerUnit);
                }styledCharacters[id]=frames;
            }playerFrames=styledCharacters[id];if(playerVisual)playerVisual.sprite=playerFrames[0];
        }
        public Sprite CharacterFrame(int character,int frame){return character==8?prisonMikeFrames[frame]:character==7?mikeyReddFrames[frame]:character==5?daveyGoatsFrames[frame]:character==6?bradJamminFrames[frame]:character==4?(wonderWrathUnlocked&&wonderWrathEquipped?wonderWrathFrames[frame]:wrathFrames[frame]):looperFrames[frame];}
        public Sprite EquipmentIcon(int slot,int family){return itemFrames[slot==0?4:slot==1?new[]{5,8,9,10}[family]:new[]{6,11,12,13}[family]];}
    }
}
