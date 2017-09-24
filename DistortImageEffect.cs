using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityStandardAssets.ImageEffects
{
    public class DistortImageEffect : PostProcessBase
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

        public SpriteRenderer[]     m_renderers;
        private CommandBuffer       m_buffer;
        private int                 m_maskID;

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

        void Awake()
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
    }
}
