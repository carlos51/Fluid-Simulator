Shader "Custom/Particles" {
    SubShader {
        Tags { "RenderType" = "Transparent" 
                "Queue" = "Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            

            #include "UnityCG.cginc"
            

            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                //fixed4 uv2      : TEXCOORD1;
            }; 

            struct MeshProperties {
                float4x4 mat;
                float4 color;
            };

            StructuredBuffer<MeshProperties> _Properties;

            v2f vert(appdata_t i, uint instanceID: SV_InstanceID) {
                v2f o;

                float4 pos = mul(_Properties[instanceID].mat, i.vertex);
                o.vertex = UnityObjectToClipPos(pos);
                o.color = _Properties[instanceID].color;
                o.uv = i.uv;


                return o;
            }

            fixed4 frag(v2f i) : SV_Target {

                
                // 1. Definir el centro del círculo en espacio UV
                float2 centro = float2(0.5, 0.5);
    
                // 2. Definir el radio (0.5 llegaría hasta los bordes)
                float radio = 0.4; 

                // 3. Calcular la distancia del píxel actual al centro
                float dist = distance(i.uv, centro);

                // 4. Lógica de visibilidad (Básico: Todo o nada)
                // Usamos el color de la instancia (i.color) pero forzamos el Alpha
                fixed4 finalCol = i.color;

                if (dist < radio) {
                    // Dentro del círculo: Mantenemos el Alpha original (o lo forzamos a 1)
                    finalCol.a = i.color.a; 
                } else {
                    // Fuera del círculo: Totalmente transparente
                    finalCol.a = 0.0;
                }

                return finalCol;
            }

            ENDCG
        }
    }
}