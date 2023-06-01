using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadingUi : MonoBehaviour
{ 
    // set these in the inspector
    [Tooltip("Reference to Image component on child panel")]
    public Image fadeImage;
    public Camera managerCam;
    [Tooltip("Color to use during scene transition")]
    public Color fadeColor = Color.black;

    [Range(1, 100), Tooltip("Rate of fade in / out: higher is faster")]
    public byte stepRate = 1;

    float step;

    void OnValidate()
    {
        if (fadeImage == null)
            fadeImage = GetComponentInChildren<Image>();
    }

    void Start()
    {
        // Convert user-friendly setting value to working value
        step = stepRate * 0.001f;
    }

    /// <summary>
    /// Calculates FadeIn / FadeOut time.
    /// </summary>
    /// <returns>Duration in seconds</returns>
    public float GetDuration()
    {
        float frames = 1 / step;
        float frameRate = Time.deltaTime;
        float duration = frames * frameRate * 0.1f;
        return duration;
    }

    public IEnumerator FadeIn()
    {
        float alpha = fadeImage.color.a;
        managerCam.enabled = true;
        while (alpha < 1)
        {
            yield return null;
            alpha += step;
            fadeColor.a = alpha;
            fadeImage.color = fadeColor;
        }
        
    }

    public IEnumerator FadeOut()
    {
        float alpha = fadeImage.color.a;
        
        while (alpha > 0)
        {
            yield return null;
            alpha -= step;
            fadeColor.a = alpha;
            fadeImage.color = fadeColor;
        }
        
        managerCam.enabled = false;
    }
}
