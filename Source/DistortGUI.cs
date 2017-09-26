using UnityEngine;
using UnityEngine.UI;

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
    public DistortImageEffect.SnapShot snapshot0;
    public DistortImageEffect.SnapShot snapshot1;
    public DistortImageEffect.SnapShot snapshot2;

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
    public void SwitchSnapshot(int i)
    {
        switch(i)
        {
            case 0: distort.ApplySnapShot(snapshot0); break;
            case 1: distort.ApplySnapShot(snapshot1); break;
            case 2: distort.ApplySnapShot(snapshot2); break;
        }

        slider_downscale.value          = distort.downScale;
        slider_zoom.value               = -distort.zoom;
        slider_fractalCoverage.value    = distort.fractalCoverage;
        slider_painting.value           = distort.painting;
        slider_entropy.value            = distort.entropy;
        slider_entropyOscA.value        = distort.entropyOscillation;
        slider_entropyOscS.value        = distort.entropyOscillationSpeed;
        slider_fisheye.value            = -distort.fisheye_x;
    }
}
