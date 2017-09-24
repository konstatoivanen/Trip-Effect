using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
using UnityEngine.UI;

public class TripController : MonoBehaviour
{
    [Range(0,1f)]
    public float interpolant;
    public Slider slider;

    [System.Serializable]
    public class SnapShot
    {
        [Range(1, 32)]
        public float downScale;
        [Range(-1f, 1f)]
        public float zoom;
        [Range(0, 2f)]
        public float fractalCoverage;
        [Range(0, 1f)]
        public float painting;
        [Range(0, 2f)]
        public float entropy;
        public float entropyOsscillation;
        public float entropyOscillationSpeed;

        [Space(10)]
        public float hue;
        public float saturation;
        public float brightness;

        [Space(10)]
        public float fisheye_x;
        public float fisheye_y;
        public float contrast;
        public float luminance;

        public SnapShot(float downScale0, float zoom0, float fractalCoverage0, float painting0, float entropy0, float entropyOsc0, float entropyOscSpd0, float hue0, float sat0, float bright0, float fishx0, float fishy0, float contrast0, float lum0)
        {
            downScale                   = downScale0;
            zoom                        = zoom0;      
            fractalCoverage             = fractalCoverage0;      
            painting                    = painting0;       
            entropy                     = entropy0;
            entropyOsscillation         = entropyOsc0;
            entropyOscillationSpeed     = entropyOscSpd0;     
            hue                         = hue0;
            saturation                  = sat0;
            brightness                  = bright0;      
            fisheye_x                   = fishx0;
            fisheye_y                   = fishy0;
            contrast                    = contrast0;
            luminance                   = lum0;
        }

        public static SnapShot operator +(SnapShot a, SnapShot b)
        {
            return new SnapShot(a.downScale + b.downScale, a.zoom + b.zoom, a.fractalCoverage + b.fractalCoverage, a.painting + b.painting, a.entropy + b.entropy, a.entropyOsscillation + b.entropyOsscillation, a.entropyOscillationSpeed + b.entropyOscillationSpeed, a.hue + b.hue, a.saturation + b.saturation, a.brightness + b.brightness, a.fisheye_x + b.fisheye_x, a.fisheye_y + b.fisheye_y, a.contrast + b.contrast, a.luminance + b.luminance);
        }

        public static SnapShot operator -(SnapShot a, SnapShot b)
        {
            return new SnapShot(a.downScale - b.downScale, a.zoom - b.zoom, a.fractalCoverage - b.fractalCoverage, a.painting - b.painting, a.entropy - b.entropy, a.entropyOsscillation - b.entropyOsscillation, a.entropyOscillationSpeed - b.entropyOscillationSpeed, a.hue - b.hue, a.saturation - b.saturation, a.brightness - b.brightness, a.fisheye_x - b.fisheye_x, a.fisheye_y - b.fisheye_y, a.contrast - b.contrast, a.luminance - b.luminance);
        }

        public static SnapShot operator -(SnapShot a)
        {
            return new SnapShot(-a.downScale, -a.zoom, -a.fractalCoverage, -a.painting, -a.entropy, -a.entropyOsscillation, -a.entropyOscillationSpeed, -a.hue, -a.saturation, -a.brightness, -a.fisheye_x, -a.fisheye_y, -a.contrast, -a.luminance);
        }

        public static SnapShot operator *(SnapShot a, float d)
        {
            return new SnapShot(a.downScale * d, a.zoom * d, a.fractalCoverage * d, a.painting * d, a.entropy * d, a.entropyOsscillation * d, a.entropyOscillationSpeed * d, a.hue * d, a.saturation * d, a.brightness * d, a.fisheye_x * d, a.fisheye_y * d, a.contrast * d, a.luminance * d);
        }

        public static SnapShot operator *(float d, SnapShot a)
        {
            return new SnapShot(d * a.downScale, d * a.zoom, d * a.fractalCoverage, d * a.painting, a.entropy, d * a.entropyOsscillation, d * a.entropyOscillationSpeed, d * a.hue, d * a.saturation, d * a.brightness, d * a.fisheye_x, d * a.fisheye_y, d * a.contrast, d * a.luminance);
        }

        public static SnapShot operator /(SnapShot a, float d)
        {
            return new SnapShot(a.downScale / d, a.zoom / d, a.fractalCoverage / d, a.painting / d, a.entropy / d, a.entropyOsscillation / d, a.entropyOscillationSpeed / d, a.hue / d, a.saturation / d, a.brightness / d, a.fisheye_x / d, a.fisheye_y / d, a.contrast / d, a.luminance / d);
        }

        public static SnapShot Lerp(SnapShot a, SnapShot b, float t)
        {
            t = Mathf.Clamp01(t);
            return a + (b - a) * Mathf.Clamp01(t);// new SnapShot(a.downScale + (b.downScale - a.downScale) * t, a.zoom + (b.zoom - a.zoom) * t, a.fractalCoverage + (b.fractalCoverage - a.fractalCoverage) * t, a.painting + (b.painting - a.painting) * t, a.entropy + (b.entropy - a.entropy) * t, a.entropyOsscillation + (b.entropyOsscillation - a.entropyOsscillation) * t, a.entropyOscillationSpeed + (b.entropyOscillationSpeed - a.entropyOscillationSpeed) * t, a.hue + (b.hue - a.hue) * t, a.saturation + (b.saturation - a.saturation) * t, a.brightness + (b.brightness - a.brightness) * t, a.fisheye_x + (b.fisheye_x - a.fisheye_x) * t, a.fisheye_y + (b.fisheye_y - a.fisheye_y) * t, a.contrast + (b.contrast - a.contrast) * t, a.luminance + (b.luminance - a.luminance) * t);
        }
    }
    public SnapShot[] snapshots;

    private DistortImageEffect m_distort;
    private float              m_interpolant;
    private bool               m_isActive;

	void Start ()
    {
        m_distort = GetComponent<DistortImageEffect>();
	}
	
	void Update ()
    {
        interpolant = slider.value;

        int length = snapshots.Length - 1;
        int i0  = Mathf.Clamp(Mathf.FloorToInt(interpolant * length), 0, snapshots.Length -1);
        int i1  = Mathf.Clamp(Mathf.CeilToInt(interpolant * length), 0 , snapshots.Length -1);

        m_interpolant = Mathf.InverseLerp(i0, i1, interpolant * length);


        ApplySnapShot(SnapShot.Lerp(snapshots[i0], snapshots[i1], m_interpolant));

	}
    void ApplySnapShot(SnapShot s)
    {
        m_distort.downScale                 = s.downScale;
        m_distort.zoom                      = s.zoom;
        m_distort.fractalCoverage           = s.fractalCoverage;
        m_distort.painting                  = s.painting;
        m_distort.entropy                   = s.entropy;
        m_distort.entropyOscillation        = s.entropyOsscillation;
        m_distort.entropyOscillationSpeed   = s.entropyOscillationSpeed;
        m_distort.hue                       = s.hue;
        m_distort.saturation                = s.saturation;
        m_distort.brightness                = s.brightness;
        m_distort.fisheye_x                 = s.fisheye_x;
        m_distort.fisheye_y                 = s.fisheye_y;
        m_distort.contrast                  = s.contrast;
        m_distort.luminance                 = s.luminance;
    }
}
