Shader "ArtificialNegligence/MapWater" {
 Properties {_MainTex("Water mask",2D)="white"{} _FlowTime("Flow time",Float)=0 _ClipRect("Clip rect",Vector)=(-32767,-32767,32767,32767)}
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 #include "UnityUI.cginc"
 struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float4 world:TEXCOORD1;};
 sampler2D _MainTex;float _FlowTime;float4 _ClipRect;
 v2f vert(appdata v){v2f o;o.world=v.vertex;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
 fixed4 frag(v2f i):SV_Target {float mask=tex2D(_MainTex,i.uv).a;float wave=.5+.5*sin(i.uv.y*260+sin(i.uv.x*70-_FlowTime*.35)*1.4+_FlowTime*1.3);float glint=pow(saturate(wave),12)*(.5+.5*sin(i.uv.x*125+_FlowTime*.9));return float4(.78,.75,.61,mask*(.055+glint*.44)*UnityGet2DClipping(i.world.xy,_ClipRect));}
 ENDCG}
 }
}
