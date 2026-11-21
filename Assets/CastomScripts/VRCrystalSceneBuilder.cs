using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class VRCrystalSceneBuilder : MonoBehaviour
{
    [Header("Настройки сцены")]
    public int crystalCount = 30;
    public float spawnRadius = 15f;
    public float crystalHeight = 8f;
    public Color ambientColor = new Color(0.05f, 0.08f, 0.15f);

    private List<GameObject> crystals = new List<GameObject>();

    void Start()
    {
        SetupLighting();
        SetupFog();
        CreateFloor();
        CreateCrystals();
        CreateStars();
        CreateCenterpiece();
    }

    void SetupLighting()
    {
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = ambientColor;
        RenderSettings.ambientEquatorColor = new Color(0.1f, 0.05f, 0.2f);
        RenderSettings.ambientGroundColor = new Color(0.02f, 0.02f, 0.05f);

        // Направленный свет (луна)
        GameObject moonLight = new GameObject("MoonLight");
        Light moon = moonLight.AddComponent<Light>();
        moon.type = LightType.Directional;
        moon.color = new Color(0.4f, 0.6f, 1f);
        moon.intensity = 0.4f;
        moonLight.transform.rotation = Quaternion.Euler(35f, -30f, 0);

        // Точечный свет в центре
        GameObject centerLight = new GameObject("CenterLight");
        Light point = centerLight.AddComponent<Light>();
        point.type = LightType.Point;
        point.color = new Color(0.3f, 0.8f, 1f);
        point.intensity = 5f;
        point.range = 25f;
        centerLight.transform.position = new Vector3(0, 3f, 0);

        // Мерцание света
        centerLight.AddComponent<LightFlicker>();
    }

    void SetupFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.03f, 0.05f, 0.12f);
        RenderSettings.fogDensity = 0.025f;
    }

    void CreateFloor()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "ReflectiveFloor";
        floor.transform.localScale = new Vector3(10, 1, 10);

        // Материал с отражениями
        Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
        Shader builtinShader = Shader.Find("Standard");
        Shader shader = urpShader != null ? urpShader : builtinShader;

        Material mat = new Material(shader);
        mat.color = new Color(0.02f, 0.03f, 0.06f);
        mat.SetFloat("_Metallic", 0.9f);
        mat.SetFloat("_Smoothness", 0.95f);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.95f);
        floor.GetComponent<Renderer>().material = mat;
    }

    void CreateCrystals()
    {
        Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
        Shader builtinShader = Shader.Find("Standard");
        Shader shader = urpShader != null ? urpShader : builtinShader;

        for (int i = 0; i < crystalCount; i++)
        {
            // Случайная позиция по кругу
            float angle = i * Mathf.PI * 2f / crystalCount + Random.Range(-0.1f, 0.1f);
            float radius = Random.Range(3f, spawnRadius);
            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * radius,
                Random.Range(0.5f, crystalHeight),
                Mathf.Sin(angle) * radius
            );

            // Создаём кристалл (октаэдр через сферу, сжатую)
            GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crystal.name = $"Crystal_{i}";
            crystal.transform.position = pos;
            crystal.transform.rotation = Random.rotation;
            crystal.transform.localScale = new Vector3(
                Random.Range(0.3f, 0.7f),
                Random.Range(0.8f, 2f),
                Random.Range(0.3f, 0.7f)
            );

            // Материал кристалла (эмиссивный)
            Material mat = new Material(shader);
            Color crystalColor = Color.Lerp(
                new Color(0.2f, 0.7f, 1f),
                new Color(0.8f, 0.3f, 1f),
                Random.value
            );
            mat.color = crystalColor * 0.3f;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", crystalColor * 2f);
            mat.SetFloat("_Metallic", 0.8f);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.9f);
            crystal.GetComponent<Renderer>().material = mat;

            // Убираем коллайдер (или оставляем для взаимодействия)
            Destroy(crystal.GetComponent<Collider>());

            // Анимация
            FloatingCrystal floatScript = crystal.AddComponent<FloatingCrystal>();
            floatScript.rotationSpeed = Random.Range(5f, 20f);
            floatScript.floatAmplitude = Random.Range(0.2f, 0.6f);
            floatScript.floatSpeed = Random.Range(0.5f, 1.5f);

            crystals.Add(crystal);
        }
    }

    void CreateStars()
    {
        GameObject stars = new GameObject("Stars");
        ParticleSystem ps = stars.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime = 999f;
        main.startSpeed = 0f;
        main.startSize = 0.05f;
        main.maxParticles = 500;
        main.startColor = new Color(0.8f, 0.9f, 1f);

        var emission = ps.emission;
        emission.rateOverTime = 0;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 60f;
        shape.radiusThickness = 0.1f;

        // Эмитируем звёзды сразу
        ps.Emit(500);

        // Материал частиц
        var renderer = stars.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = new Color(0.8f, 0.9f, 1f);
    }

    void CreateCenterpiece()
    {
        // Большой центральный кристалл
        GameObject center = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        center.name = "CenterCrystal";
        center.transform.position = new Vector3(0, 3f, 0);
        center.transform.localScale = Vector3.one * 2f;

        Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
        Shader builtinShader = Shader.Find("Standard");
        Shader shader = urpShader != null ? urpShader : builtinShader;

        Material mat = new Material(shader);
        mat.color = new Color(0.3f, 0.9f, 1f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(0.2f, 0.8f, 1.5f));
        center.GetComponent<Renderer>().material = mat;

        Destroy(center.GetComponent<Collider>());

        center.AddComponent<FloatingCrystal>().rotationSpeed = 10f;
    }
}
