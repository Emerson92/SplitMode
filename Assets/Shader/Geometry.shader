Shader "My/DoublePass"
{

	Properties
	{
			_Diffuse("Diffuse", Color) = (1,1,1,1)
			_DissolveColor("Dissolve Color", Color) = (0,0,0,0)
			_MainTex("Base 2D", 2D) = "white"{}
			_ColorFactor("ColorFactor", Range(0,1)) = 0.7
			_DissolveThreshold("DissolveThreshold", Float) = 0
			_BoxScale("BoxScale",Range(0,1)) = 0.001
			_BoxColor("BoxColor",Color) = (1,1,1,1)
	}

		SubShader
			{
			Tags{ "RenderType" = "Opaque" }

			Pass{
				Cull Off
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag	

				#include "Lighting.cginc"
				uniform fixed4 _Diffuse;
				uniform fixed4 _DissolveColor;
				uniform sampler2D _MainTex;
				uniform float4 _MainTex_ST;
				uniform float _ColorFactor;
				uniform float _DissolveThreshold;

				struct v2f
				{
					float4 pos : SV_POSITION;
					float3 worldNormal : TEXCOORD0;
					float2 uv : TEXCOORD1;
					float4 objPos : TEXCOORD2;
				};

				v2f vert(appdata_base v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.worldNormal = mul(v.normal, (float3x3)unity_WorldToObject);
					o.objPos = v.vertex;
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					float factor = _DissolveThreshold - i.objPos.y;
					clip(factor);

					fixed3 worldNormal = normalize(i.worldNormal);
					fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);
					fixed3 lambert = saturate(dot(worldNormal, worldLightDir));
					fixed3 albedo = lambert * _Diffuse.xyz * _LightColor0.xyz + UNITY_LIGHTMODEL_AMBIENT.xyz;
					fixed3 color = tex2D(_MainTex, i.uv).rgb * albedo;

					if (factor < _ColorFactor)
					{
						return _DissolveColor;
					}
					return fixed4(color, 1);
				}
				ENDCG
			}

			Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }

			Pass {
				  Blend SrcAlpha OneMinusSrcAlpha
				  Cull Off

				  CGPROGRAM
				  #pragma target 4.0
				  #pragma vertex vert
				  #pragma geometry geo
				  #pragma fragment frag
				  #pragma multi_compile_fog
				  #include "UnityCG.cginc"

						struct appdata
						{
							float4 vertex : POSITION;
							float2 uv : TEXCOORD0;
							float3 normal : NORMAL;
						};

						struct v2g
						{
							float4 vertex : POSITION;
							float3 normal : NORMAL;
							float2 uv : TEXCOORD0;
							float4 objPos: TEXCOORD1;
						};

						struct g2f
						{
							float2 uv : TEXCOORD0;
							float4 vertex : SV_POSITION;
						};

				  sampler2D _MainTex;
				  float4 _MainTex_ST;
				  float _BoxScale;
				  fixed4 _BoxColor;
				  float _ColorFactor;
				  float _DissolveThreshold;
				  float _Alpha;

				  v2g vert(appdata v) {
					v2g o;
					o.vertex = v.vertex;
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.objPos = v.vertex;
					return o;
				  }


				  #define ADD_VERT(v) \
			        o.vertex = UnityObjectToClipPos(v); \
			        TriStream.Append(o);

				  #define ADD_TRI(p0, p1, p2) \
			        ADD_VERT(p0) ADD_VERT(p1) \
			        ADD_VERT(p2) \
			        TriStream.RestartStrip();


				  [maxvertexcount(36)]
				  void geo(triangle v2g v[3], inout TriangleStream<g2f> TriStream) {

					float4 vertex = (v[0].vertex + v[1].vertex + v[2].vertex) / 3;
					float2 uv = (v[0].uv + v[1].uv + v[2].uv) / 3;

					float3 edgeA = v[1].vertex - v[0].vertex;
					float3 edgeB = v[2].vertex - v[0].vertex;
					float3 normalFace = normalize(cross(edgeA, edgeB));



					float factor = _DissolveThreshold - vertex.y;
					//if(factor < 0) return;
					if (factor < _ColorFactor) {

						//这里存粹是为效果而设置的，让它看上去是对的。
						vertex.xyz += normalFace * clamp(-0.5f + vertex.y + _Time.y * 0.2f,0,5);

						g2f o;
						o.uv = uv;
						float scale = _BoxScale;

						float4 v0 = float4(1, 1, 1,1) * scale + float4(vertex.xyz,0);
						float4 v1 = float4(1, 1,-1,1) * scale + float4(vertex.xyz,0);
						float4 v2 = float4(1,-1, 1,1) * scale + float4(vertex.xyz,0);
						float4 v3 = float4(1,-1,-1,1) * scale + float4(vertex.xyz,0);
						float4 v4 = float4(-1, 1, 1,1) * scale + float4(vertex.xyz,0);
						float4 v5 = float4(-1, 1,-1,1) * scale + float4(vertex.xyz,0);
						float4 v6 = float4(-1,-1, 1,1) * scale + float4(vertex.xyz,0);
						float4 v7 = float4(-1,-1,-1,1) * scale + float4(vertex.xyz,0);


						ADD_TRI(v0, v2, v3);
						ADD_TRI(v3, v1, v0);
						ADD_TRI(v5, v7, v6);
						ADD_TRI(v6, v4, v5);

						ADD_TRI(v4, v0, v1);
						ADD_TRI(v1, v5, v4);
						ADD_TRI(v7, v3, v2);
						ADD_TRI(v2, v6, v7);

						ADD_TRI(v6, v2, v0);
						ADD_TRI(v0, v4, v6);
						ADD_TRI(v5, v1, v3);
						ADD_TRI(v3, v7, v5);
						}
					  }

					  fixed4 frag(g2f i) : SV_Target {
						float4 col = _BoxColor;
						col.a = _Alpha;
						return col;
					  }
					  ENDCG
				}
			}
}