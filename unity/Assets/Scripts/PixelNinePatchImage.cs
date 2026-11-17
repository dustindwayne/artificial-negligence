using UnityEngine;
using UnityEngine.UI;
namespace ArtificialNegligence {
 // Fixed pixel corners, repeating edges, and one stretched blank center.
 public sealed class PixelNinePatchImage:Image {
  protected override void OnPopulateMesh(VertexHelper mesh){
   if(!sprite||sprite.border==Vector4.zero){base.OnPopulateMesh(mesh);return;}
   mesh.Clear();var rect=GetPixelAdjustedRect();var source=sprite.textureRect;var border=sprite.border;float unit=sprite.pixelsPerUnit/(canvas?canvas.referencePixelsPerUnit:100);
   float left=Mathf.Round(border.x/unit),right=Mathf.Round(border.z/unit),bottom=Mathf.Round(border.y/unit),top=Mathf.Round(border.w/unit);float shrinkX=Mathf.Min(1,rect.width/Mathf.Max(1,left+right)),shrinkY=Mathf.Min(1,rect.height/Mathf.Max(1,bottom+top));left*=shrinkX;right*=shrinkX;bottom*=shrinkY;top*=shrinkY;
   float[] dx={rect.xMin,rect.xMin+left,rect.xMax-right,rect.xMax},dy={rect.yMin,rect.yMin+bottom,rect.yMax-top,rect.yMax};float[] sx={source.xMin,source.xMin+border.x,source.xMax-border.z,source.xMax},sy={source.yMin,source.yMin+border.y,source.yMax-border.w,source.yMax};
   for(int y=0;y<3;y++)for(int x=0;x<3;x++){
    float width=dx[x+1]-dx[x],height=dy[y+1]-dy[y];if(width<=0||height<=0)continue;
    bool tileX=x==1&&y!=1,tileY=y==1&&x!=1;float stepX=tileX?Mathf.Max(1,Mathf.Round((sx[x+1]-sx[x])/unit)):width,stepY=tileY?Mathf.Max(1,Mathf.Round((sy[y+1]-sy[y])/unit)):height;
    for(float yy=0;yy<height;yy+=stepY)for(float xx=0;xx<width;xx+=stepX){float w=Mathf.Min(stepX,width-xx),h=Mathf.Min(stepY,height-yy);Quad(mesh,new Rect(dx[x]+xx,dy[y]+yy,w,h),new Rect(sx[x],sy[y],(sx[x+1]-sx[x])*(tileX?w/stepX:1),(sy[y+1]-sy[y])*(tileY?h/stepY:1)));}
   }
  }
  void Quad(VertexHelper mesh,Rect dest,Rect uv){int at=mesh.currentVertCount;float tw=sprite.texture.width,th=sprite.texture.height;mesh.AddVert(new Vector3(dest.xMin,dest.yMin),color,new Vector2(uv.xMin/tw,uv.yMin/th));mesh.AddVert(new Vector3(dest.xMin,dest.yMax),color,new Vector2(uv.xMin/tw,uv.yMax/th));mesh.AddVert(new Vector3(dest.xMax,dest.yMax),color,new Vector2(uv.xMax/tw,uv.yMax/th));mesh.AddVert(new Vector3(dest.xMax,dest.yMin),color,new Vector2(uv.xMax/tw,uv.yMin/th));mesh.AddTriangle(at,at+1,at+2);mesh.AddTriangle(at+2,at+3,at);}
 }
}
