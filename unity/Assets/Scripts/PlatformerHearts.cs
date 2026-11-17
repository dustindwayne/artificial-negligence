using UnityEngine;
namespace ArtificialNegligence {
 public partial class GameController {
  Sprite fullHeart,emptyHeart;
  Sprite HeartSprite(bool filled){
   if(filled&&fullHeart)return fullHeart;if(!filled&&emptyHeart)return emptyHeart;
   string[] shape={"01100110","11111111","11111111","11111111","01111110","00111100","00011000","00000000"};
   var texture=new Texture2D(8,8,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};
   for(int y=0;y<8;y++)for(int x=0;x<8;x++){
    bool inside=shape[y][x]=='1';bool border=inside&&(x==0||x==7||y==0||y==7||shape[y][Mathf.Max(0,x-1)]=='0'||shape[y][Mathf.Min(7,x+1)]=='0'||shape[Mathf.Max(0,y-1)][x]=='0'||shape[Mathf.Min(7,y+1)][x]=='0');
    texture.SetPixel(x,7-y,inside&&(filled||border)?Color.white:Color.clear);
   }texture.Apply();var sprite=Sprite.Create(texture,new Rect(0,0,8,8),Vector2.one*.5f,8,0,SpriteMeshType.FullRect);if(filled)fullHeart=sprite;else emptyHeart=sprite;return sprite;
  }
 }
}
