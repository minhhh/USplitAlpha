Shader "SplitAlpha/Sprites/Default" 
{
Properties
{
    _MainTex ("Sprite Texture", 2D) = "white" {}
    _AlphaTex ("External Alpha", 2D) = "white" {}
    _Color ("Tint", Color) = (1,1,1,1)
    [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
}

SubShader
{
    Tags
    { 
        "Queue"="Transparent" 
        "IgnoreProjector"="True" 
        "RenderType"="Transparent" 
        "PreviewType"="Plane"
        "CanUseSpriteAtlas"="True"
    }

    Cull Off
    Lighting Off
    ZWrite Off
    Blend One OneMinusSrcAlpha

    Pass
    {
    CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        #pragma multi_compile _ PIXELSNAP_ON

        #include "UnityCG.cginc"

        fixed4 _Color;
        sampler2D _MainTex;
        sampler2D _AlphaTex;

        struct appdata_t
        {
            float4 vertex   : POSITION;
            float4 color    : COLOR;
            float2 texcoord : TEXCOORD0;
        };

        struct v2f
        {
            float4 pos   : SV_POSITION;
            fixed4 color    : COLOR;
            float2 texcoord : TEXCOORD0;
        };

        v2f vert(appdata_t v)
        {
            v2f o;

            o.pos = UnityObjectToClipPos(v.vertex);
            o.texcoord = v.texcoord;
            o.color = v.color * _Color;

            #ifdef PIXELSNAP_ON
            o.pos = UnityPixelSnap (o.pos);
            #endif

            return o;
        }

        fixed4 frag(v2f i) : SV_Target
        {
            fixed4 c = tex2D (_MainTex, i.texcoord);
            fixed4 alpha = tex2D (_AlphaTex, i.texcoord);
            c.a = alpha.r;
            c *= i.color;
            c.rgb *= c.a;
            return c;
        }
    ENDCG
    }
} 
}
