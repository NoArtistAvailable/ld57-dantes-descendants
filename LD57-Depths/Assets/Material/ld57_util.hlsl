void GetSpriteColor_float(out float4 Color){
#ifndef SHADERGRAPH_PREVIEW
    Color = unity_SpriteColor;
#else
    Color = float4(1,1,1,1);
#endif
}
void GetSpriteColor_half(out half4 Color){
#ifndef SHADERGRAPH_PREVIEW
    Color = unity_SpriteColor;
#else
    Color = half4(1,1,1,1);
#endif
}

void FaceCamera_float(float3 Position, out float3 Out){
    #if SHADERGRAPH_PREVIEW
        Out = Position;
    #else
        float3 worldPos = mul(UNITY_MATRIX_M, float4(0,0,0,1)).xyz; // Object origin
        float3 forward = normalize(_WorldSpaceCameraPos - worldPos);
        forward.y = 0;
        forward = normalize(forward);
        
        float3 right = normalize(cross(float3(0,1,0), forward));
        float3 up = float3(0,1,0);
        
        float3x3 rotationMatrix = float3x3(right, up, forward);
        Out = mul(Position, rotationMatrix);
    #endif
}
void FaceCamera_half(half3 Position, out half3 Out){
    #if SHADERGRAPH_PREVIEW
        Out = Position;
    #else
        half3 worldPos = mul(UNITY_MATRIX_M, half4(0,0,0,1)).xyz; // Object origin
        half3 forward = normalize(_WorldSpaceCameraPos - worldPos);
        forward.y = 0;
        forward = normalize(forward);
        
        half3 right = normalize(cross(half3(0,1,0), forward));
        half3 up = half3(0,1,0);
        
        half3x3 rotationMatrix = half3x3(right, up, forward);
        Out = mul(Position, rotationMatrix);
    #endif
}

float map(float s, float a1, float a2, float b1, float b2)
{
    return b1 + (s-a1)*(b2-b1)/(a2-a1);
}

void CheckFlip_float(float2 uv, float3 flipX, out float2 OUT){
    if(flipX.x >= 1){
        uv.x = map(uv.x, flipX.y, flipX.z, flipX.z, flipX.y);
    }
    OUT = uv;
}

