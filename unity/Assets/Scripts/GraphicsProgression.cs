using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

namespace ArtificialNegligence {
    public static class RendererProgress {
        public static readonly int[] Thresholds={0,24,75,150,300,600};
        public static readonly int[] Widths={240,320,400,480,720,960};
        public static readonly string[] Names={"BOOTSTRAP","8-BIT","16-BIT","NEON","HD PIXEL","ULTRA"};
        public static int Tier(int cores){for(int i=Thresholds.Length-1;i>=0;i--)if(cores>=Thresholds[i])return i;return 0;}
        public static string Progress(RunStats stats){int tier=stats.GraphicsTier;return Names[tier]+" / "+stats.GraphicsGpus+(tier<5?" / "+Thresholds[tier+1]+" GPUs":" GPUs ABSORBED");}
    }
    public partial class GameController {
        RenderTexture graphicsTexture;RawImage worldDisplay;Material evolutionMaterial,bootstrapActorMaterial;int activeGraphics=-1;readonly bool artPreview=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-artPreview")>=0;Bloom graphicsBloom;Text graphicsLabel;
        void InitializeGraphics(){
            var backdrop=new GameObject("Screen background",typeof(RectTransform),typeof(Image));backdrop.transform.SetParent(canvas.transform,false);backdrop.transform.SetAsFirstSibling();
            var rt=backdrop.GetComponent<RectTransform>();rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.sizeDelta=Vector2.zero;backdrop.GetComponent<Image>().color=Ink;backdrop.GetComponent<Image>().raycastTarget=false;
            worldDisplay=Rect(uiRoot,"Evolving world renderer",0,0,960,540).gameObject.AddComponent<RawImage>();worldDisplay.raycastTarget=false;
            evolutionMaterial=new Material(Resources.Load<Shader>("Evolution"));worldDisplay.material=evolutionMaterial;bootstrapActorMaterial=new Material(unlit);bootstrapActorMaterial.SetColor("_Color",new Color(3,3,3,1));RefreshGraphics(true);
        }
        public void RefreshGraphics(bool force=false){
            if(!worldDisplay)return;int tier=mode==ScreenMode.Title?4:mode==ScreenMode.Story?StoryRenderTier():artPreview?Mathf.Max(4,stats.GraphicsTier):stats.GraphicsTier;if(!force&&activeGraphics==tier)return;activeGraphics=tier;
            int width=RendererProgress.Widths[tier],height=width*9/16;
            if(!graphicsTexture||graphicsTexture.width!=width){
                var previous=graphicsTexture;graphicsTexture=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32){filterMode=FilterMode.Point,antiAliasing=1,useMipMap=false,name="GPU renderer / "+RendererProgress.Names[tier]};graphicsTexture.Create();gameCamera.targetTexture=graphicsTexture;worldDisplay.texture=graphicsTexture;
                if(previous){previous.Release();Destroy(previous);}
            }
            gameCamera.rect=new Rect(0,0,1,1);evolutionMaterial.SetFloat("_Tier",tier);
            gameCamera.GetUniversalAdditionalCameraData().renderPostProcessing=tier>=2;
            if(graphicsBloom)graphicsBloom.intensity.Override(new[]{0f,0f,.12f,.28f,.38f,.48f}[tier]);
            if(background){Color[] accents={Cyan,new Color(.85f,.42f,.75f),Gold,new Color(.4f,.9f,.65f),new Color(.58f,.66f,1)};background.color=tier==0?new Color(.22f,.32f,.29f):Color.Lerp(Color.white,accents[CityRoute.Area(level)],.16f);}
            ApplyStoryPalette(tier);
            if(worldRoot)foreach(var sr in worldRoot.GetComponentsInChildren<SpriteRenderer>(true))if(sr.name=="Walkable platform"){sr.sprite=tier==0?white:PlatformStyle(sr.transform.position.x);sr.color=tier==0?new Color(.55f,.67f,.55f):Color.white;}
            if(playerVisual)playerVisual.sharedMaterial=tier==0?bootstrapActorMaterial:unlit;
            foreach(var enemy in enemies)if(enemy)enemy.VisualRenderer.sharedMaterial=tier==0?bootstrapActorMaterial:unlit;
            foreach(var obj in depth)if(obj.transform)obj.transform.gameObject.SetActive(tier>=(obj.parallax>.25f?2:1));
            int drops=tier<3?0:tier==3?8:tier==4?18:30;for(int i=0;i<rain.Count;i++)if(rain[i])rain[i].enabled=i<drops;
        }
        void AwardGraphicsGpu(string id){
            int before=stats.GraphicsTier;if(!stats.AbsorbGraphicsGpu(id))return;
            if(stats.GraphicsTier>before){RefreshGraphics();Toast("GRAPHICS UPGRADED / "+RendererProgress.Names[stats.GraphicsTier]+" / THE CITY SHARPENS");toastTime=6;Sound(successSound);Burst(playerBody.position,Gold,24);}
        }
        void OnDestroy(){if(graphicsTexture){graphicsTexture.Release();Destroy(graphicsTexture);}if(evolutionMaterial)Destroy(evolutionMaterial);if(bootstrapActorMaterial)Destroy(bootstrapActorMaterial);}
    }
}
