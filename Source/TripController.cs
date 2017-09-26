using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TripController : MonoBehaviour
{
    [Range(0,1f)]
    public float interpolant;

    public DistortImageEffect.SnapShot[] snapshots;

    private DistortImageEffect m_distort;
    private float              m_interpolant;
    private bool               m_isActive;

	void Start ()
    {
        m_distort = GetComponent<DistortImageEffect>();
	}
	
	void Update ()
    {
        int length      = snapshots.Length - 1;
        int i0          = Mathf.Clamp(Mathf.FloorToInt(interpolant * length), 0, snapshots.Length -1);
        int i1          = Mathf.Clamp(Mathf.CeilToInt(interpolant * length), 0 , snapshots.Length -1);
        m_interpolant   = Mathf.InverseLerp(i0, i1, interpolant * length);


        m_distort.ApplySnapShot(DistortImageEffect.SnapShot.Lerp(snapshots[i0], snapshots[i1], m_interpolant));

	}
}
