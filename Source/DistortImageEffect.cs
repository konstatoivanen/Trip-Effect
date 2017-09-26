using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

    public class DistortImageEffect : MonoBehaviour
    {
        [Range(1, 32)]
        public float downScale;
        [Range(-1f,1f)]
        public float zoom;
        [Range(0, 2f)]
        public float fractalCoverage;
        [Range(0,1f)]
        public float painting;
        [Range(0,2f)]
        public float entropy;
        public float entropyOscillation;
        public float entropyOscillationSpeed;
        public float gradientSpeed;
        public Gradient gradient;
        
        [Space(10)]
        public float hue;
        public float saturation;
        public float brightness;

        [Space(10)]
        public float fisheye_x;
        public float fisheye_y;
        public float contrast;
        public float luminance;

        [Space(10)]
        public LayerMask            layerMask;
        public LayerMask            inverseMask;

        public SpriteRenderer[]     m_renderers;
        private CommandBuffer       m_buffer;
        private int                 m_maskID;

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

        private RenderTexture   accumTexture;
        private int             h_maskComposite;
        private int             h_zoom;
        private int             h_entropy;
        private int             h_color;
        private int             h_hsv;
        private int             h_xybc;
        private int             h_coverage;
        private int             h_painting;
        private int             h_downScale;

        protected Shader shader;
        private Material m_Material;

        protected Material material
        {
            get
            {
                if (m_Material == null)
                {
                    m_Material = new Material(shader);
                    m_Material.hideFlags = HideFlags.HideAndDontSave;
                }
                return m_Material;
            }
        }
        protected void OnEnable()
        {
            if (!SystemInfo.supportsImageEffects)
            {
                enabled = false;
                return;
            }
            if(!shader) shader = Shader.Find("Hidden/Distort");

            if (!shader || !shader.isSupported)
            {
                enabled = false;
                return;
            }
        }
        protected void OnDisable()
        {
            if (m_Material)
            {
                DestroyImmediate(m_Material);
            }
        }

        void Start()
        {
            shader              = Shader.Find("Hidden/Distort");
            h_maskComposite     = Shader.PropertyToID("_MaskComposite");
            h_zoom              = Shader.PropertyToID("_Zoom");
            h_entropy           = Shader.PropertyToID("_Entropy");
            h_color             = Shader.PropertyToID("_Color");
            h_hsv               = Shader.PropertyToID("_HSV");
            h_xybc              = Shader.PropertyToID("_XYBC");
            h_coverage          = Shader.PropertyToID("_Coverage");
            h_painting          = Shader.PropertyToID("_Painting");
            h_painting          = Shader.PropertyToID("_Painting");
            h_downScale         = Shader.PropertyToID("_DownScale");

            RebuildBuffer();

            GetComponent<Camera>().cullingMask = inverseMask;
        }
        void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (accumTexture == null || accumTexture.width != src.width || accumTexture.height != src.height)
            {
                DestroyImmediate(accumTexture);
                accumTexture = new RenderTexture(src.descriptor);
                accumTexture.hideFlags = HideFlags.HideAndDontSave;
                Graphics.Blit(src, accumTexture);
            }

            Graphics.Blit(src, accumTexture, material, 0);

            material.SetTexture(h_maskComposite, accumTexture);
            material.SetFloat(h_zoom, zoom);
            material.SetFloat(h_entropy, entropy + Mathf.Sin(Time.time * entropyOscillationSpeed) * entropyOscillation);
            material.SetColor(h_color, gradient.Evaluate(Mathf.Repeat(Time.time * gradientSpeed,1)));
            material.SetVector(h_hsv, new Vector4(hue, brightness, saturation));
            material.SetVector(h_xybc, new Vector4(fisheye_x, fisheye_y, luminance,1- contrast));
            material.SetFloat(h_coverage, fractalCoverage);
            material.SetFloat(h_painting, painting);
            material.SetFloat(h_downScale, downScale);

            RenderTexture rt0 = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.Default);

            Graphics.Blit(src, rt0, material, 1);

            RenderTexture rt1 = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.Default);

            Graphics.Blit(rt0, rt1, material, 2);

            rt0.DiscardContents();

            Graphics.Blit(rt1, rt0, material, 3);
            Graphics.Blit(rt0, dst, material, 4);

            RenderTexture.ReleaseTemporary(rt0);
            RenderTexture.ReleaseTemporary(rt1);
        }
        private void RebuildBuffer()
        {
            List<SpriteRenderer> temp0 = new List<SpriteRenderer>();
            List<SpriteRenderer> temp1 = new List<SpriteRenderer>();
            temp0.AddRange(FindObjectsOfType<SpriteRenderer>());

            for (int i = 0; i < temp0.Count; i++) if (layerMask == (layerMask | (1 << temp0[i].gameObject.layer))) temp1.Add(temp0[i]);
            m_renderers = temp1.ToArray();

            if(m_buffer != null)
            {
                try
                {
                    gameObject.GetComponent<Camera>().RemoveCommandBuffer(CameraEvent.BeforeImageEffects, m_buffer);
                    m_buffer.Dispose();
                    m_buffer = null;
                }
                catch
                {
                    enabled = false;
                    Debug.LogWarning("The renderer doesnt have a camera attached");
                }
            }

            m_buffer = new CommandBuffer();
            m_buffer.name = "Objects To Mask";

            m_maskID = Shader.PropertyToID("_ObjectMask");
            m_buffer.GetTemporaryRT(m_maskID, Screen.currentResolution.width, Screen.currentResolution.height);
            m_buffer.SetRenderTarget(m_maskID);
            m_buffer.ClearRenderTarget(true, true, Color.clear);

            for(int i = 0; i < m_renderers.Length; i++)
            {
                m_buffer.DrawRenderer(m_renderers[i], m_renderers[i].material);
            }

            m_buffer.SetGlobalTexture(m_maskID, m_maskID);

            m_buffer.ReleaseTemporaryRT(m_maskID);

            try
            {
                gameObject.GetComponent<Camera>().AddCommandBuffer(CameraEvent.BeforeImageEffects, m_buffer);
            }
            catch
            {
                enabled = false;
                Debug.LogWarning("The renderer doesnt have a camera attached");
            }
        }

        public void ApplySnapShot(SnapShot s)
        {
            downScale                 = s.downScale;
            zoom                      = s.zoom;
            fractalCoverage           = s.fractalCoverage;
            painting                  = s.painting;
            entropy                   = s.entropy;
            entropyOscillation        = s.entropyOsscillation;
            entropyOscillationSpeed   = s.entropyOscillationSpeed;
            hue                       = s.hue;
            saturation                = s.saturation;
            brightness                = s.brightness;
            fisheye_x                 = s.fisheye_x;
            fisheye_y                 = s.fisheye_y;
            contrast                  = s.contrast;
            luminance                 = s.luminance;
        }
    }
