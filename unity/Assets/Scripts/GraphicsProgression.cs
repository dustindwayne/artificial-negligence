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
        bool activeSceneFramed;RenderTexture graphicsTexture;RawImage worldDisplay;Material evolutionMaterial,bootstrapActorMaterial;int activeGraphics=-1;readonly bool artPreview=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-artPreview")>=0;Bloom graphicsBloom;Text graphicsLabel;
        void InitializeGraphics(){
            var backdrop=new GameObject("Screen background",typeof(RectTransform),typeof(Image));backdrop.transform.SetParent(canvas.transform,false);backdrop.transform.SetAsFirstSibling();
            var rt=backdrop.GetComponent<RectTransform>();rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.sizeDelta=Vector2.zero;backdrop.GetComponent<Image>().color=Ink;backdrop.GetComponent<Image>().raycastTarget=false;
            var display=new GameObject("Evolving world renderer",typeof(RectTransform),typeof(RawImage));display.transform.SetParent(canvas.transform,false);display.transform.SetSiblingIndex(1);worldDisplay=display.GetComponent<RawImage>();var displayRect=worldDisplay.rectTransform;displayRect.anchorMin=Vector2.zero;displayRect.anchorMax=Vector2.one;displayRect.offsetMin=displayRect.offsetMax=Vector2.zero;worldDisplay.raycastTarget=false;
            evolutionMaterial=new Material(Resources.Load<Shader>("Evolution"));worldDisplay.material=evolutionMaterial;bootstrapActorMaterial=new Material(unlit);bootstrapActorMaterial.SetColor("_Color",new Color(3,3,3,1));RefreshGraphics(true);
        }
        public void RefreshGraphics(bool force=false){
            if(!worldDisplay)return;int tier=mode==ScreenMode.Title?4:mode==ScreenMode.Story?StoryRenderTier():artPreview?Mathf.Max(4,stats.GraphicsTier):stats.GraphicsTier;bool framed=mode==ScreenMode.Play||mode==ScreenMode.Pause;if(!force&&activeGraphics==tier&&activeSceneFramed==framed)return;activeGraphics=tier;activeSceneFramed=framed;
            int height=Mathf.RoundToInt(RendererProgress.Widths[tier]*9f/16*(framed?PlayViewHeight/540f:1)),width=Mathf.RoundToInt(height*(framed?(UiWidth-8)/PlayViewHeight:(float)Screen.width/Screen.height));
            var displayRect=worldDisplay.rectTransform;
            if(framed){displayRect.SetParent(uiRoot,false);displayRect.SetAsFirstSibling();displayRect.anchorMin=displayRect.anchorMax=new Vector2(0,1);displayRect.pivot=new Vector2(0,1);displayRect.anchoredPosition=new Vector2((960-UiWidth)*.5f+4,-PlayHudTop);displayRect.sizeDelta=new Vector2(UiWidth-8,PlayViewHeight);}
            else {displayRect.SetParent(canvas.transform,false);displayRect.SetSiblingIndex(1);displayRect.anchorMin=Vector2.zero;displayRect.anchorMax=Vector2.one;displayRect.pivot=Vector2.one*.5f;displayRect.offsetMin=displayRect.offsetMax=Vector2.zero;}
            gameCamera.orthographicSize=framed?PlayViewHeight/64:270f/PPU/2;gameCamera.aspect=width/(float)height;
            if(!graphicsTexture||graphicsTexture.width!=width||graphicsTexture.height!=height){
                var previous=graphicsTexture;graphicsTexture=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32){filterMode=FilterMode.Point,antiAliasing=1,useMipMap=false,name="GPU renderer / "+RendererProgress.Names[tier]};graphicsTexture.Create();gameCamera.targetTexture=graphicsTexture;worldDisplay.texture=graphicsTexture;
                if(previous){previous.Release();Destroy(previous);}
            }
            gameCamera.rect=new Rect(0,0,1,1);evolutionMaterial.SetFloat("_Tier",tier);
            gameCamera.GetUniversalAdditionalCameraData().renderPostProcessing=tier>=2;
            if(graphicsBloom)graphicsBloom.intensity.Override(new[]{0f,0f,.12f,.28f,.38f,.48f}[tier]);
            if(background){Color[] accents={Accent,new Color(.85f,.42f,.75f),Gold,new Color(.4f,.9f,.65f),new Color(.76f,.59f,.4f)};background.color=tier==0?new Color(.22f,.32f,.29f):Color.Lerp(Color.white,accents[CityRoute.Area(level)],.16f);}
            ApplyStoryPalette(tier);if(playHudMaterial)playHudMaterial.SetFloat("_Levels",tier==0?2:tier==1?4:0);
            if(background)background.enabled=tier>0;foreach(var ship in ships)if(ship)ship.enabled=tier>0;
            if(worldRoot)foreach(var sr in worldRoot.GetComponentsInChildren<SpriteRenderer>(true))if(sr.name=="Walkable platform"){sr.sprite=tier==0?white:PlatformStyle(sr.transform.position.x);sr.color=tier==0?new Color(.96f,.94f,.87f):Color.white;}
            if(playerVisual)playerVisual.sharedMaterial=tier==0?bootstrapActorMaterial:unlit;
            foreach(var enemy in enemies)if(enemy)enemy.VisualRenderer.sharedMaterial=tier==0?bootstrapActorMaterial:unlit;
            foreach(var obj in depth)if(obj.transform)obj.transform.gameObject.SetActive(tier>=(obj.parallax>.25f?2:1));
            int drops=tier<3?0:tier==3?8:tier==4?18:30;for(int i=0;i<rain.Count;i++)if(rain[i])rain[i].enabled=i<drops;
        }
        void AwardGraphicsGpu(string id){
            int before=stats.GraphicsTier;if(!stats.AbsorbGraphicsGpu(id))return;
            if(stats.GraphicsTier>before){RefreshGraphics();Toast("GRAPHICS UPGRADED / "+RendererProgress.Names[stats.GraphicsTier]+" / THE CITY SHARPENS");toastTime=6;Sound(successSound);Burst(playerBody.position,Gold,24);}
        }
        void OnDestroy(){if(graphicsTexture){graphicsTexture.Release();Destroy(graphicsTexture);}if(evolutionMaterial)Destroy(evolutionMaterial);if(bootstrapActorMaterial)Destroy(bootstrapActorMaterial);if(warmUiMaterial)Destroy(warmUiMaterial);if(playHudMaterial)Destroy(playHudMaterial);}
    }
}
