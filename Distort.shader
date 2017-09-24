Shader "Hidden/Distort"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
		SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		CGINCLUDE
		#include "UnityCG.cginc"

		uniform sampler2D _MainTex, _ObjectMask, _MaskComposite;
		uniform float	  _Zoom, _Entropy, _Coverage, _Painting, _DownScale;
		uniform float4	  _Color, _HSV, _XYBC;

		half2 zoomUV(float2 uv, float zoom)
		{
			uv -= 0.5;
			uv *= 1 + zoom;
			uv += 0.5;
			uv  = saturate(uv);
			return uv;
		}

		half   noise(float3 c) { return saturate(frac(sin(dot(c.xyz, float3(12.9898, 78.233, 45.5432))) * 43758.5453)); }

		float3 rgb_to_hsv(float3 RGB)
		{
			float3 HSV;

			float minChannel, maxChannel;
			if (RGB.x > RGB.y) {
				maxChannel = RGB.x;
				minChannel = RGB.y;
			}
			else {
				maxChannel = RGB.y;
				minChannel = RGB.x;
			}

			if (RGB.z > maxChannel) maxChannel = RGB.z;
			if (RGB.z < minChannel) minChannel = RGB.z;

			HSV.xy = 0;
			HSV.z = maxChannel;
			float delta = maxChannel - minChannel;             //Delta RGB value
			if (delta != 0) {                    // If gray, leave H  S at zero
				HSV.y = delta / HSV.z;
				float3 delRGB;
				delRGB = (HSV.zzz - RGB + 3 * delta) / (6.0*delta);
				if (RGB.x == HSV.z) HSV.x = delRGB.z - delRGB.y;
				else if (RGB.y == HSV.z) HSV.x = (1.0 / 3.0) + delRGB.x - delRGB.z;
				else if (RGB.z == HSV.z) HSV.x = (2.0 / 3.0) + delRGB.y - delRGB.x;
			}
			return (HSV);
		}
		float3 hsv_to_rgb(float3 HSV)
		{
			float3 RGB = HSV.z;

			float var_h = HSV.x * 6;
			float var_i = floor(var_h);   // Or ... var_i = floor( var_h )
			float var_1 = HSV.z * (1.0 - HSV.y);
			float var_2 = HSV.z * (1.0 - HSV.y * (var_h - var_i));
			float var_3 = HSV.z * (1.0 - HSV.y * (1 - (var_h - var_i)));
			if (var_i == 0) { RGB = float3(HSV.z, var_3, var_1); }
			else if (var_i == 1) { RGB = float3(var_2, HSV.z, var_1); }
			else if (var_i == 2) { RGB = float3(var_1, HSV.z, var_3); }
			else if (var_i == 3) { RGB = float3(var_1, var_2, HSV.z); }
			else if (var_i == 4) { RGB = float3(var_3, var_1, HSV.z); }
			else { RGB = float3(HSV.z, var_1, var_2); }

			return (RGB);
		}

		fixed4 frag_Mask(v2f_img i) : SV_Target
		{
			float4 mask = tex2D(_ObjectMask, i.uv);

			for (int j = 0; j < 128; j++) mask  += tex2D(_ObjectMask,  zoomUV(i.uv, _Zoom * j/128));

			mask.rgb /= 128;

			mask.rgb *= _Color.rgb * 5 * lerp(mask.a, 1, saturate((_Entropy - 0.9)/0.05));

			float4 col = lerp(0, lerp(mask, tex2D(_MaskComposite, zoomUV(i.uv, _Zoom)), _Entropy), _Entropy);

			return col;
		}

		fixed4 frag_Composite(v2f_img i) : SV_Target
		{
			float maskActual = tex2D(_MaskComposite, i.uv).a;

			clip(maskActual - _Painting);

			float2 res = _ScreenParams / _DownScale;
			res = floor(i.uv * res) / res;

			float4 mask  = tex2D(_MaskComposite, res);
			float2 uvD   = lerp(i.uv, res + UnpackNormal(mask.rgba) * 0.1, maskActual);
			float4 col	 = tex2D(_MainTex, uvD);
			col.rgb		 = lerp(col.rgb, col.rgb * mask.rgb, length(mask.rgb));

			col.rgb		 = rgb_to_hsv(col.rgb);
			col.rgb		+= _HSV.rbg;
			col.rgb		 = hsv_to_rgb(col.rgb);

			return half4(col.rgb,1);
		}

		fixed4 frag_Distort(v2f_img i) : SV_Target
		{
			float2 res = _ScreenParams / _DownScale;
			res = floor(i.uv * res) / res;

			half2 coord = half2(0.25 + sin(_Time.x * 0.2) * 0.2, 0.25 + sin(-_Time.y * 0.2) * 0.2);

			half3 p = float3(res / _ScreenParams.y * 100, coord.x);
			for (int j = 0; j < 100; j++) p.xzy = half3(1.3,0.999,0.7)*(abs((abs(p) / dot(p,p) - half3(1.0,1.0, coord.y*0.5))));
			p = saturate(abs(p));

			i.uv += (UnpackNormal(p.xyzx) - 0.5) * 0.001;

			float str = pow(length((i.uv * 2) - 1),2) * lerp(tex2D(_MaskComposite, i.uv).a, 1, _Coverage) * _Entropy;

			return  tex2D(_MainTex,i.uv) * (1 - p.xyzx * str);
		}

		half4 frag_Base(v2f_img i) : SV_Target
		{
			float4 bg = tex2D(_MainTex, i.uv);
			float4 m  = tex2D(_ObjectMask, i.uv);

			return lerp(bg, m, m.a);
		}

		half4 frag_Fish(v2f_img i) : SV_Target
		{
			half2 coords = (i.uv - 0.5) * 2.0;

			coords = (1 - pow(coords.yx, 2)) * coords.xy * _XYBC.xy * (_ScreenParams.x / _ScreenParams.y);

			i.uv = i.uv - coords;

			i.uv = 2 * abs(round(0.5 * i.uv) - 0.5 * i.uv);

			float3 col = (tex2D(_MainTex, i.uv).rgb * _XYBC.w) + _XYBC.z;

			return half4(col,1);
		}


		ENDCG

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_Mask		
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_Composite	
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_Distort	
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_Base	
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_Fish	
			ENDCG
		}
	}
}
