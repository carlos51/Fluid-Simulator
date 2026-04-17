Shader "Unlit/AllParticlesOneQuad"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            StructuredBuffer<float2> _Positions;
            int _ParticleCount;
            float _Radius;
            float4 _Color;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert(uint id : SV_VertexID)
            {
                v2f o;

                // Fullscreen quad con 2 triangulos (6 vertices)
                float2 verts[6] = {
                    float2(-1,-1), float2(1,-1), float2(1,1),
                    float2(-1,-1), float2(1,1), float2(-1,1)
                };

                float2 pos = verts[id];
                o.pos = float4(pos, 0, 1);
                o.uv  = pos; // en espacio NDC [-1,1]
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // coordenadas mundo en 2D (aqui puedes adaptar tu escala)
                float2 worldPos = i.uv; 

                // revisa todas las particulas
                for (int k = 0; k < _ParticleCount; k++)
                {
                    float2 p = _Positions[k];
                    if (length(worldPos - p) < _Radius)
                        return _Color; // dentro del circulo
                }

                discard; // fuera de cualquier particula -> transparente
                return 0;
            }
            ENDCG
        }
    }
}
