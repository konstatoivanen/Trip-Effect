using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class DistortGUI : MonoBehaviour
{
    public Slider slider_downscale;
    public Slider slider_zoom;
    public Slider slider_fractalCoverage;
    public Slider slider_painting;
    public Slider slider_entropy;
    public Slider slider_entropyOscA;
    public Slider slider_entropyOscS;
    public Slider slider_fisheye;
    public Slider slider_characterPosition;
    public Transform character;
    public DistortImageEffect distort;


	void Update ()
    {
        distort.downScale               =  slider_downscale.value;
        distort.zoom                    = -slider_zoom.value;
        distort.fractalCoverage         =  slider_fractalCoverage.value;
        distort.painting                =  slider_painting.value; 
        distort.entropy                 =  slider_entropy.value;
        distort.entropyOscillation      =  slider_entropyOscA.value;
        distort.entropyOscillationSpeed =  slider_entropyOscS.value;
        distort.fisheye_x               = -slider_fisheye.value;
        distort.fisheye_y               = -slider_fisheye.value;
        character.position              = new Vector3(slider_characterPosition.value, 0, -1);
    }
}
