Shader "ArtificialNegligence/WorldEvolution" {
 Properties { _MainTex("World",2D)="white"{} _Tier("Graphics tier",Float)=0 }
 SubShader {
  Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}
  Cull Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   sampler2D _MainTex;float _Tier;
   v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
   fixed4 frag(v2f i):SV_Target {
    float4 c=tex2D(_MainTex,i.uv)*i.color;
    if(_Tier<.5){
     float luminance=pow(saturate(dot(c.rgb,float3(.299,.587,.114))),.7);
     float shade=floor(luminance*3+.5)/3;
     float3 high=float3(.72,.93,.84);
     // Danger and objective accents stay distinct, even in the earliest renderer.
     if(c.r>.35&&c.r>c.g*1.45&&c.b>.15)high=float3(1,.42,.7);
     else if(c.r>.55&&c.g>.4&&c.b<.45)high=float3(1,.84,.3);
     c.rgb=lerp(float3(.005,.035,.055),high,shade);
    }else if(_Tier<1.5)c.rgb=floor(c.rgb*float3(7,7,3)+.5)/float3(7,7,3);
    else if(_Tier<2.5)c.rgb=floor(c.rgb*float3(31,63,31)+.5)/float3(31,63,31);
    else if(_Tier<3.5)c.rgb=floor(c.rgb*63+.5)/63;
    else if(_Tier<4.5)c.rgb=floor(c.rgb*127+.5)/127;
    return c;
   }
   ENDCG
  }
 }
}
