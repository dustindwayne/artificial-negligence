using UnityEngine;
using UnityEngine.UI;
namespace ArtificialNegligence {
 public partial class GameController {
  Sprite pixelArtworkPanel,pixelArtworkButton,pixelArtworkWindow;
  Sprite ArtworkFrame(bool compact){
   if(!compact)return InfoFrame();if(pixelArtworkButton)return pixelArtworkButton;
   var t=Resources.Load<Texture2D>("Art/pixel-tech-button-v2");float sx=t.width/2172f,sy=t.height/724f;return pixelArtworkButton=Sprite.Create(t,new Rect(34*sx,82*sy,2104*sx,560*sy),Vector2.one*.5f,2400*sx,0,SpriteMeshType.FullRect,new Vector4(180*sx,120*sy,180*sx,120*sy));
  }
  Sprite pixelInfoFrame;
  Sprite InfoFrame(){if(pixelInfoFrame)return pixelInfoFrame;var t=Resources.Load<Texture2D>("Art/pixel-tech-hud");return pixelInfoFrame=Sprite.Create(t,new Rect(31,111,1712,682),Vector2.one*.5f,1300,0,SpriteMeshType.FullRect,new Vector4(260,145,260,145));}
  Sprite WindowFrame(){if(pixelArtworkWindow)return pixelArtworkWindow;var t=Resources.Load<Texture2D>("Art/pixel-tech-window-v2");return pixelArtworkWindow=Sprite.Create(t,new Rect(30,26,1476,971),Vector2.one*.5f,500,0,SpriteMeshType.FullRect,new Vector4(125,135,125,300));}
  const float PlayHudTop=56, PlayHudBottom=464, PlayViewHeight=PlayHudBottom-PlayHudTop;
  void BuildHudFrame(){
   Color platform=PlatformColors[CityRoute.Area(level)];
   Color backing=Color.Lerp(new Color32(23,27,26,255),platform,.14f);
   hudRailColor=Color.Lerp(new Color32(101,112,91,255),platform,.18f);
   hudButtonFace=Color.Lerp(new Color32(48,56,49,255),platform,.2f);
   hudTrayFace=Color.Lerp(new Color32(29,37,32,255),platform,.2f);
   Image(screenRoot,"HUD upper backing",0,0,960,PlayHudTop,backing);
   Image(screenRoot,"HUD lower backing",0,PlayHudBottom,960,540-PlayHudBottom,backing);
   hudTop=Rect(screenRoot,"Top bar",0,0,960,PlayHudTop);
   hudBottom=Rect(screenRoot,"Control bar",0,PlayHudBottom,960,540-PlayHudBottom);

   // The two-pixel lip echoes the level's walkable surfaces.
   Color lip=Color.Lerp(hudRailColor,HudMuted,.34f);
   Image(hudTop,"Platform trim",0,PlayHudTop-6,UiWidth,4,hudTrayFace);
   Image(hudBottom,"Platform trim",0,2,UiWidth,5,hudTrayFace);
   for(float x=16;x+2<UiWidth;x+=16){
    Image(hudTop,"Deck joint",x,PlayHudTop-5,2,3,hudButtonFace);
    Image(hudBottom,"Deck joint",x,4,2,3,hudButtonFace);
   }
   Image(hudTop,"Platform lip",0,PlayHudTop-2,UiWidth,1,lip);
   Image(hudTop,"Platform seam",0,PlayHudTop-1,UiWidth,1,hudRailColor);
   Image(hudBottom,"Platform lip",0,0,UiWidth,1,lip);
   Image(hudBottom,"Platform seam",0,1,UiWidth,1,hudRailColor);
  }
 }
}
