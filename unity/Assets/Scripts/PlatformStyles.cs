using UnityEngine;
namespace ArtificialNegligence {
 public partial class GameController {
  readonly Sprite[] platformStyles=new Sprite[5];
  Sprite PlatformStyle(float x){
   int style=(CityRoute.Area(level)+Mathf.Abs(Mathf.FloorToInt(x*PPU/90)))%5;
   if(platformStyles[style])return platformStyles[style];
   var texture=new Texture2D(24,24,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};
   Color[] bases={new Color(.065f,.12f,.14f),new Color(.14f,.09f,.15f),new Color(.09f,.13f,.17f),new Color(.08f,.15f,.13f),new Color(.16f,.13f,.08f)};
   for(int y=0;y<24;y++)for(int px=0;px<24;px++){
    Color c=bases[style];bool seam=false,detail=false;
    if(style==0){seam=y%8==0||(px+(y/8%2)*12)%24==0;detail=(px==3||px==20)&&(y==3||y==20);}
    else if(style==1){seam=px%6==0||y%12==0;detail=px%6==3&&y%12==6;}
    else if(style==2){seam=y%6==0||px%6==0;detail=(px+y)%12==0;}
    else if(style==3){seam=y%12==0||(px+(y/12%2)*8)%16==0;detail=y%12==2&&px%8<3;}
    else{seam=y%8==0;detail=y>18&&(px+y)%12<4;}
    if(seam)c*=.55f;if(detail)c=Color.Lerp(c,new Color(.48f,.62f,.6f),style==4?.65f:.4f);if(y==23)c=new Color(.35f,.5f,.51f);c.a=1;texture.SetPixel(px,y,c);
   }texture.Apply();platformStyles[style]=Sprite.Create(texture,new Rect(0,0,24,24),Vector2.one*.5f,PPU,0,SpriteMeshType.FullRect);return platformStyles[style];
  }
 }
}
