Shader "Unlit/Draw"
{
    Properties
    {
        _Color("Test Color",color) = (1,1,1,1)
        _Radius("Radius", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            fixed4 _Color;
            float _Radius;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calcular distancia desde el centro (0.5,0.5)
                float2 center = float2(0.5, 0.5);
                float2 uvs = i.uv;

                uvs *= 3;
                uvs -= frac(1 * float2(_Time.y,0));
                float dist = distance(uvs, center);

                // Si está dentro del radio, pintamos el color, sino transparente
                if (dist <= _Radius)
                    return _Color;
                else
                    return fixed4(1,1,1,0); // transparente
            }
            ENDCG
        }
    }
}
