using UnityEngine;
using UnityEngine.UI;
namespace ArtificialNegligence {
 public partial class GameController {
  readonly Sprite[] pixelControls=new Sprite[8];
  readonly Sprite[] hudGlyphs=new Sprite[8];
  Sprite HudGlyph(int cell){
   if(hudGlyphs[cell])return hudGlyphs[cell];var texture=new Texture2D(32,32,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};var pixels=new Color32[1024];
   for(int y=0;y<32;y++)for(int x=0;x<32;x++){
    int xx=cell==0?31-x:x,yy=cell==4?31-y:y;bool on=false;
    if(cell<=1)on=xx>=3&&xx<=18&&y>=12&&y<=19||xx>=15&&xx<=27&&Mathf.Abs(y-15.5f)<=27-xx;
    if(cell==2||cell==4)on=x>=13&&x<=18&&yy>=9&&yy<=22||yy>=18&&yy<=27&&Mathf.Abs(x-15.5f)<=27-yy||yy>=4&&yy<=6&&x>=6&&x<=25;
    if(cell==3)on=EnumerableChevron(x,y,3)||EnumerableChevron(x,y,15);
    if(cell==5)on=Mathf.Abs(x+y-31)<2&&x>=7&&x<=25||Mathf.Abs(x-y-5)<2&&x>=9&&x<=18&&y>=5&&y<=14||x>=5&&x<=8&&y>=23&&y<=26;
    if(cell==6)on=(x>=8&&x<=23&&(y==8||y==23))||(y>=8&&y<=23&&(x==8||x==23))||x>=13&&x<=18&&y>=13&&y<=18||((x==4||x==27)&&y>=10&&y<=21&&y%4<2)||((y==4||y==27)&&x>=10&&x<=21&&x%4<2);
    if(on)pixels[y*32+x]=new Color32(255,255,255,255);
   }
   texture.SetPixels32(pixels);texture.Apply();return hudGlyphs[cell]=Sprite.Create(texture,new Rect(0,0,32,32),Vector2.one*.5f,100);
  }
  static bool EnumerableChevron(int x,int y,int start){return y>=7&&y<=24&&Mathf.Abs(x-(start+8-Mathf.Abs(y-15.5f)))<2.5f;}
  void ApplyHudControl(Button button,int cell){
   if(cell==6)SkinHudButton(button);
   else {
    button.transform.GetChild(1).GetComponent<Image>().enabled=false;
    button.GetComponent<ButtonFeedback>().preserveArtwork=true;
   }
   button.transform.GetChild(2).gameObject.SetActive(false);
   var area=button.GetComponent<RectTransform>().rect;
   Image(button.transform,"Control glyph",(area.width-32)*.5f,(area.height-32)*.5f,32,32,cell==5||cell==6?HudGold:HudText,HudGlyph(cell));
  }
  Sprite ControlArtwork(int cell){
   if(pixelControls[cell])return pixelControls[cell];var texture=Resources.Load<Texture2D>("Art/pixel-tech-controls");var pixels=texture.GetPixels32();int x0=cell%4*texture.width/4,x1=(cell%4+1)*texture.width/4,y0=(1-cell/4)*texture.height/2,y1=(2-cell/4)*texture.height/2;int left=x1,right=x0,bottom=y1,top=y0;
   for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)if(pixels[y*texture.width+x].a>=200){left=Mathf.Min(left,x);right=Mathf.Max(right,x);bottom=Mathf.Min(bottom,y);top=Mathf.Max(top,y);}
   var rect=new Rect(Mathf.Max(x0,left-3),Mathf.Max(y0,bottom-3),Mathf.Min(x1,right+4)-Mathf.Max(x0,left-3),Mathf.Min(y1,top+4)-Mathf.Max(y0,bottom-3));return pixelControls[cell]=Sprite.Create(texture,rect,Vector2.one*.5f,100);
  }
  void ApplyControlArtwork(Button button,int cell){
   var fill=button.transform.GetChild(0).GetComponent<Image>();fill.color=Color.clear;
   var art=button.transform.GetChild(1).GetComponent<Image>();art.sprite=ControlArtwork(cell);art.type=UnityEngine.UI.Image.Type.Simple;art.preserveAspect=true;art.color=Color.white;button.transform.GetChild(2).gameObject.SetActive(false);var label=button.GetComponentInChildren<Text>();if(label)label.text="";
  }
 }
}
