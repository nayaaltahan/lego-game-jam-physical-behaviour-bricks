Shader "LEGO/Grid"
{
    Properties
    {
        [MainColor] _BaseColor("Color", Color) = (0.5,0.5,0.5,1)
        _GridColor("Grid Color", Color) = (0.5,0.5,0.5,1)
        [MainTexture] _BaseMap("Grid Texture", 2D) = "white" {}

        [HideInInspector] _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5

        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
    }

    SubShader
    {
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" "IgnoreProjector" = "True" "DisableBatching"="True"}
        LOD 300

        Pass
        {
            Name "StandardLit"
            Tags{"LightMode" = "UniversalForward"}

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            #pragma multi_compile_instancing

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"

            half4 _GridColor;

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv           : TEXCOORD0;
                float2 uvLM         : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv                       : TEXCOORD0;
                float2 uvLM                     : TEXCOORD1;
                float4 positionWSAndFogFactor   : TEXCOORD2;
                half3  normalWS                 : TEXCOORD3;

#ifdef _MAIN_LIGHT_SHADOWS
                float4 shadowCoord              : TEXCOORD6;
#endif
                float4 positionCS               : SV_POSITION;
            };

            Varyings LitPassVertex(Attributes input)
            {
                Varyings output;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

                VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                float fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.uvLM = input.uvLM.xy * unity_LightmapST.xy + unity_LightmapST.zw;

                output.positionWSAndFogFactor = float4(vertexInput.positionWS, fogFactor);
                output.normalWS = vertexNormalInput.normalWS;

#ifdef _MAIN_LIGHT_SHADOWS
                output.shadowCoord = GetShadowCoord(vertexInput);
#endif
                output.positionCS = vertexInput.positionCS;
                return output;
            }

            half4 LitPassFragment(Varyings input) : SV_Target
            {
                SurfaceData surfaceData;
                InitializeStandardLitSurfaceData(input.uv, surfaceData);

                // Do custom grid albedo.
                float4 unscaledWorldRight = normalize(unity_ObjectToWorld._m00_m10_m20_m30);
                float4 unscaledWorldUp = normalize(unity_ObjectToWorld._m01_m11_m21_m31);
                float4 unscaledWorldForward = normalize(unity_ObjectToWorld._m02_m12_m22_m32);
                float4 worldOrigin = unity_ObjectToWorld._m03_m13_m23_m33;
                float4x4 worldToUnscaledObject = float4x4(unscaledWorldRight, unscaledWorldUp, unscaledWorldForward, worldOrigin);

                half4 c = SampleAlbedoAlpha(mul(worldToUnscaledObject, (float4(input.positionWSAndFogFactor.xyz, 1.0) - worldOrigin)).xz / 6.4, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
                c = lerp(_BaseColor, _GridColor, c.a);
                c = lerp(_BaseColor, c, pow(clamp(dot(input.normalWS, unscaledWorldUp.xyz), 0, 1), 10));

                surfaceData.albedo = c.rgb;

                half3 normalWS = input.normalWS;
                normalWS = normalize(normalWS);

#ifdef LIGHTMAP_ON
                half3 bakedGI = SampleLightmap(input.uvLM, normalWS);
#else
                half3 bakedGI = SampleSH(normalWS);
#endif

                float3 positionWS = input.positionWSAndFogFactor.xyz;
                half3 viewDirectionWS = SafeNormalize(GetCameraPositionWS() - positionWS);

                BRDFData brdfData;
                InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);

#ifdef _MAIN_LIGHT_SHADOWS
                Light mainLight = GetMainLight(input.shadowCoord);
#else
                Light mainLight = GetMainLight();
#endif

                half3 color = GlobalIllumination(brdfData, bakedGI, surfaceData.occlusion, normalWS, viewDirectionWS);

                color += LightingPhysicallyBased(brdfData, mainLight, normalWS, viewDirectionWS);

#ifdef _ADDITIONAL_LIGHTS

                int additionalLightsCount = GetAdditionalLightsCount();
                for (int i = 0; i < additionalLightsCount; ++i)
                {
                    Light light = GetAdditionalLight(i, positionWS);

                    color += LightingPhysicallyBased(brdfData, light, normalWS, viewDirectionWS);
                }
#endif
                color += surfaceData.emission;

                float fogFactor = input.positionWSAndFogFactor.w;

                color = MixFog(color, fogFactor);
                return half4(color, surfaceData.alpha);
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"

        UsePass "Universal Render Pipeline/Lit/DepthOnly"

        UsePass "Universal Render Pipeline/Lit/Meta"
    }
}