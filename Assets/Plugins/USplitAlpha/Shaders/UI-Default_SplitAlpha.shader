Shader "SplitAlpha/UI/Default"
{
Properties
{
    _MainTex ("Sprite Texture", 2D) = "white" {}
    _AlphaTex("External Texture", 2D) = "white" {}
    _Color ("Tint", Color) = (1,1,1,1)

    _StencilComp ("Stencil Comparison", Float) = 8
    _Stencil ("Stencil ID", Float) = 0
    _StencilOp ("Stencil Operation", Float) = 0
    _StencilWriteMask ("Stencil Write Mask", Float) = 255
    _StencilReadMask ("Stencil Read Mask", Float) = 255

    _ColorMask ("Color Mask", Float) = 15

    [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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

    Stencil
    {
        Ref [_Stencil]
        Comp [_StencilComp]
        Pass [_StencilOp]
        ReadMask [_StencilReadMask]
        WriteMask [_StencilWriteMask]
    }

    Cull Off
    Lighting Off
    ZWrite Off
    ZTest [unity_GUIZTestMode]
    Blend SrcAlpha OneMinusSrcAlpha
    ColorMask [_ColorMask]

    Pass
    {
    CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0

        #include "UnityCG.cginc"
        #include "UnityUI.cginc"

        #pragma multi_compile __ UNITY_UI_ALPHACLIP

        fixed4 _Color;
        fixed4 _TextureSampleAdd;
        float4 _ClipRect;
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
            float2 texcoord  : TEXCOORD0;
            float4 worldPosition : TEXCOORD1;
        };

        v2f vert(appdata_t v)
        {
            v2f o;
            o.worldPosition = v.vertex;
            o.pos = UnityObjectToClipPos(o.worldPosition);

            o.texcoord = v.texcoord;

            #ifdef UNITY_HALF_TEXEL_OFFSET
            o.pos.xy += (_ScreenParams.zw-1.0) * float2(-1,1) * o.pos.w;
            #endif

            o.color = v.color * _Color;
            return o;
        }

        fixed4 frag(v2f i) : SV_Target
        {
            fixed4 color = UnityGetUIDiffuseColor(i.texcoord, _MainTex, _AlphaTex, _TextureSampleAdd) * i.color;

            color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);

            #ifdef UNITY_UI_ALPHACLIP
            clip (color.a - 0.001);
            #endif

            return color;
        }
    ENDCG
    }
}
}
