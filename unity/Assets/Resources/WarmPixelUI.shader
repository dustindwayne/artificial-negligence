Shader "ArtificialNegligence/WarmPixelUI" {
 Properties { [PerRendererData] _MainTex("Sprite",2D)="white"{} _Color("Tint",Color)=(1,1,1,1) _Levels("Palette levels",Float)=0 _StencilComp("Stencil Comparison",Float)=8 _Stencil("Stencil",Float)=0 _StencilOp("Stencil Operation",Float)=0 _StencilWriteMask("Stencil Write",Float)=255 _StencilReadMask("Stencil Read",Float)=255 _ColorMask("Color Mask",Float)=15 }
 SubShader { Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True"} Stencil {Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask]} Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
 #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
 #include "UnityCG.cginc"
 #include "UnityUI.cginc"
 struct appdata {float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};struct v2f {float4 vertex:SV_POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;float4 world:TEXCOORD1;};sampler2D _MainTex;fixed4 _Color;float4 _ClipRect;float _Levels;
 v2f vert(appdata v){v2f o;o.world=v.vertex;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;return o;}
 fixed4 frag(v2f i):SV_Target {fixed4 c=tex2D(_MainTex,i.uv);float l=dot(c.rgb,float3(.2126,.7152,.0722));if(_Levels>1)l=round(l*(_Levels-1))/(_Levels-1);float3 warm=l*float3(1,.91,.76);float amber=step(c.b*1.4,c.r)*step(c.b*1.25,c.g)*step(.12,c.r);c.rgb=lerp(warm,max(c.rgb,warm*.75),amber*(_Levels>1?0:.65));c*=i.color;
 #ifdef UNITY_UI_CLIP_RECT
 c.a*=UnityGet2DClipping(i.world.xy,_ClipRect);
 #endif
 #ifdef UNITY_UI_ALPHACLIP
 clip(c.a-.001);
 #endif
 return c;}
 ENDCG }
 }
}
