using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    public float minIntensity = 3f;
    public float maxIntensity = 6f;
    public float speed = 2f;

    private Light lightSource;

    void Start()
    {
        lightSource = GetComponent<Light>();
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * speed, 0f);
        lightSource.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}