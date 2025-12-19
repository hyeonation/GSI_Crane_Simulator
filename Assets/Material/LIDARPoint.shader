Shader "Custom/LiDAR_Point_BuiltIn"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0, 1, 0, 1)
        _PointSize ("Point Size", Range(0.01, 1.0)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2g
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            struct g2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            float _PointSize;
            fixed4 _Color;
            sampler2D _MainTex;

            // Vertex Shader: 위치만 전달
            v2g vert (appdata v)
            {
                v2g o;
                o.vertex = v.vertex;
                o.color = v.color;
                return o;
            }

            // Geometry Shader: 점 1개를 사각형(Triangle Strip)으로 변환
            [maxvertexcount(4)]
            void geom(point v2g p[1], inout TriangleStream<g2f> triStream)
            {
                float3 up = float3(0, 1, 0);
                float3 right = float3(1, 0, 0);
                
                // 카메라를 바라보게 하려면(Billboard) 아래 주석 해제 후 up/right 계산
                // float3 look = _WorldSpaceCameraPos - mul(unity_ObjectToWorld, p[0].vertex).xyz;
                // look = normalize(look);
                // right = normalize(cross(float3(0,1,0), look));
                // up = cross(look, right);

                float halfS = 0.5f * _PointSize;

                float4 v[4];
                v[0] = float4(p[0].vertex.xyz + right * -halfS + up * -halfS, 1.0f);
                v[1] = float4(p[0].vertex.xyz + right * halfS + up * -halfS, 1.0f);
                v[2] = float4(p[0].vertex.xyz + right * -halfS + up * halfS, 1.0f);
                v[3] = float4(p[0].vertex.xyz + right * halfS + up * halfS, 1.0f);

                g2f outP;
                outP.color = p[0].color;

                // 1번 정점
                outP.vertex = UnityObjectToClipPos(v[0]);
                outP.uv = float2(0, 0);
                triStream.Append(outP);

                // 2번 정점
                outP.vertex = UnityObjectToClipPos(v[1]);
                outP.uv = float2(1, 0);
                triStream.Append(outP);

                // 3번 정점
                outP.vertex = UnityObjectToClipPos(v[2]);
                outP.uv = float2(0, 1);
                triStream.Append(outP);

                // 4번 정점
                outP.vertex = UnityObjectToClipPos(v[3]);
                outP.uv = float2(1, 1);
                triStream.Append(outP);

                triStream.RestartStrip();
            }

            fixed4 frag (g2f i) : SV_Target
            {
                // 원형으로 깎고 싶으면 아래 주석 해제
                // float2 c = i.uv - 0.5;
                // if(length(c) > 0.5) discard;

                return _Color;
            }
            ENDCG
        }
    }
}