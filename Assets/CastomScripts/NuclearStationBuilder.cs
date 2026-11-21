using UnityEngine;
using System.Collections.Generic;

public class NuclearStationBuilder : MonoBehaviour
{
    [Header("Размеры помещения")]
    public Vector3 roomSize = new Vector3(24f, 15f, 24f);
    public float wallThickness = 0.3f;

    [Header("Реактор")]
    public float reactorRadius = 2.5f;
    public float reactorHeight = 10f;

    [Header("Освещение")]
    public Color emergencyColor = new Color(1f, 0.15f, 0.1f);
    public Color reactorGlowColor = new Color(0.3f, 0.9f, 1f);
    public Color warningLightColor = new Color(1f, 0.6f, 0f);

    private Transform stationRoot;
    private List<Light> emergencyLights = new List<Light>();
    private List<Light> reactorLights = new List<Light>();

    void Start()
    {
        CreateRoot();
        SetupEnvironment();
        CreateFloorAndCeiling();
        CreateWalls();
        //CreateReactor();
       //CreateControlPanels();
        CreatePipes();
        CreateEmergencyLights();
        CreateSteamEffects();
        CreateCatwalk();
    }

    void CreateRoot()
    {
        GameObject root = new GameObject("NuclearStation");
        stationRoot = root.transform;
    }

    Shader GetShader()
    {
        Shader urp = Shader.Find("Universal Render Pipeline/Lit");
        return urp != null ? urp : Shader.Find("Standard");
    }

    Material MakeMaterial(Color color, float metallic = 0.8f, float smoothness = 0.7f, Color? emission = null)
    {
        Material mat = new Material(GetShader());
        mat.color = color;
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
        if (emission.HasValue)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission.Value);
        }
        return mat;
    }

    void SetupEnvironment()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.02f, 0.03f, 0.04f);
        RenderSettings.fogDensity = 0.03f;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.1f, 0.12f, 0.15f);
        RenderSettings.ambientEquatorColor = new Color(0.05f, 0.06f, 0.08f);
        RenderSettings.ambientGroundColor = new Color(0.02f, 0.02f, 0.03f);
    }

    void CreateFloorAndCeiling()
    {
        Material floorMat = MakeMaterial(new Color(0.08f, 0.09f, 0.1f), 0.9f, 0.85f);
        Material ceilingMat = MakeMaterial(new Color(0.05f, 0.05f, 0.06f), 0.7f, 0.5f);

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(stationRoot);
        floor.transform.position = new Vector3(0, -0.1f, 0);
        floor.transform.localScale = new Vector3(roomSize.x, 0.2f, roomSize.z);
        floor.GetComponent<Renderer>().material = floorMat;

        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "Ceiling";
        ceiling.transform.SetParent(stationRoot);
        ceiling.transform.position = new Vector3(0, roomSize.y, 0);
        ceiling.transform.localScale = new Vector3(roomSize.x, 0.2f, roomSize.z);
        ceiling.GetComponent<Renderer>().material = ceilingMat;
    }

    void CreateWalls()
    {
        Material wallMat = MakeMaterial(new Color(0.12f, 0.13f, 0.15f), 0.6f, 0.4f);

        // 4 стены
        Vector3[] positions = {
            new Vector3(0, roomSize.y / 2f, roomSize.z / 2f),
            new Vector3(0, roomSize.y / 2f, -roomSize.z / 2f),
            new Vector3(roomSize.x / 2f, roomSize.y / 2f, 0),
            new Vector3(-roomSize.x / 2f, roomSize.y / 2f, 0)
        };

        Vector3[] scales = {
            new Vector3(roomSize.x, roomSize.y, wallThickness),
            new Vector3(roomSize.x, roomSize.y, wallThickness),
            new Vector3(wallThickness, roomSize.y, roomSize.z),
            new Vector3(wallThickness, roomSize.y, roomSize.z)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = $"Wall_{i}";
            wall.transform.SetParent(stationRoot);
            wall.transform.position = positions[i];
            wall.transform.localScale = scales[i];
            wall.GetComponent<Renderer>().material = wallMat;

            // Добавляем панели на стенах
            AddWallPanels(wall.transform, scales[i]);
        }
    }

    void AddWallPanels(Transform wall, Vector3 wallScale)
    {
        Material panelMat = MakeMaterial(new Color(0.15f, 0.16f, 0.18f), 0.8f, 0.6f);

        // Горизонтальные полосы-панели
        int rows = 4;
        for (int r = 0; r < rows; r++)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "Panel";
            panel.transform.SetParent(wall);
            panel.transform.localScale = new Vector3(0.98f, 0.15f, 1.05f);
            panel.transform.localPosition = new Vector3(0, -0.4f + r * 0.4f, 0);

            Material m = MakeMaterial(new Color(0.16f, 0.17f, 0.19f), 0.85f, 0.7f);
            panel.GetComponent<Renderer>().material = m;
        }
    }

    void CreateReactor()
    {
        // Основание реактора
        GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseObj.name = "ReactorBase";
        baseObj.transform.SetParent(stationRoot);
        baseObj.transform.position = new Vector3(0, 0.3f, 0);
        baseObj.transform.localScale = new Vector3(reactorRadius * 2.4f, 0.3f, reactorRadius * 2.4f);
        baseObj.GetComponent<Renderer>().material = MakeMaterial(new Color(0.1f, 0.1f, 0.12f), 0.9f, 0.7f);

        // Корпус реактора
        GameObject reactor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        reactor.name = "ReactorCore";
        reactor.transform.SetParent(stationRoot);
        reactor.transform.position = new Vector3(0, reactorHeight / 2f + 0.6f, 0);
        reactor.transform.localScale = new Vector3(reactorRadius * 2f, reactorHeight / 2f, reactorRadius * 2f);

        Material reactorMat = MakeMaterial(
            new Color(0.15f, 0.2f, 0.25f),
            0.95f, 0.9f,
            reactorGlowColor * 0.3f
        );
        reactor.GetComponent<Renderer>().material = reactorMat;

        // Светящееся ядро внутри
        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        core.name = "ReactorGlow";
        core.transform.SetParent(reactor.transform);
        core.transform.localPosition = Vector3.zero;
        core.transform.localScale = Vector3.one * 0.5f;
        core.GetComponent<Renderer>().material = MakeMaterial(
            reactorGlowColor, 0f, 1f, reactorGlowColor * 5f
        );
        Destroy(core.GetComponent<Collider>());
        core.AddComponent<FloatingCrystal>().rotationSpeed = 20f;

        // Свет от реактора
        GameObject reactorLightObj = new GameObject("ReactorLight");
        reactorLightObj.transform.SetParent(reactor.transform);
        reactorLightObj.transform.localPosition = Vector3.zero;
        Light reactorLight = reactorLightObj.AddComponent<Light>();
        reactorLight.type = LightType.Point;
        reactorLight.color = reactorGlowColor;
        reactorLight.intensity = 8f;
        reactorLight.range = 25f;
        reactorLights.Add(reactorLight);

        // Кольца вокруг реактора
        for (int i = 0; i < 3; i++)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = $"ReactorRing_{i}";
            ring.transform.SetParent(reactor.transform);
            ring.transform.localPosition = new Vector3(0, -0.3f + i * 0.3f, 0);
            ring.transform.localScale = new Vector3(1.1f, 0.02f, 1.1f);
            ring.GetComponent<Renderer>().material = MakeMaterial(
                new Color(0.4f, 0.4f, 0.45f), 0.95f, 1f,
                warningLightColor * 0.5f
            );
            Destroy(ring.GetComponent<Collider>());
        }

        // Трубы от реактора к стенам
        CreateReactorPipes();
    }

    void CreateReactorPipes()
    {
        Material pipeMat = MakeMaterial(new Color(0.3f, 0.3f, 0.32f), 0.9f, 0.7f);

        // 4 трубы в стороны
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f;
            GameObject pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pipe.name = $"Pipe_{i}";
            pipe.transform.SetParent(stationRoot);
            pipe.transform.position = new Vector3(
                Mathf.Sin(angle * Mathf.Deg2Rad) * 6f,
                reactorHeight + 0.6f,
                Mathf.Cos(angle * Mathf.Deg2Rad) * 6f
            );
            pipe.transform.rotation = Quaternion.Euler(0, 0, 90f);
            pipe.transform.rotation = Quaternion.Euler(
                Mathf.Cos(angle * Mathf.Deg2Rad) * 90f,
                angle,
                Mathf.Sin(angle * Mathf.Deg2Rad) * 90f
            );
            pipe.transform.localScale = new Vector3(0.4f, 6f, 0.4f);
            pipe.GetComponent<Renderer>().material = pipeMat;
        }
    }

    void CreateControlPanels()
    {
        Material consoleMat = MakeMaterial(new Color(0.08f, 0.09f, 0.1f), 0.7f, 0.5f);
        Material screenMat = MakeMaterial(Color.black, 0.3f, 0.9f, new Color(0.1f, 0.6f, 0.3f) * 1.5f);
        Material buttonMat = MakeMaterial(new Color(0.9f, 0.1f, 0.1f), 0.3f, 0.7f, new Color(1f, 0.1f, 0.1f) * 2f);

        // Расставляем пульты по кругу
        int consoleCount = 6;
        for (int i = 0; i < consoleCount; i++)
        {
            float angle = i * (360f / consoleCount);
            Vector3 pos = new Vector3(
                Mathf.Sin(angle * Mathf.Deg2Rad) * 5f,
                0,
                Mathf.Cos(angle * Mathf.Deg2Rad) * 5f
            );

            GameObject console = GameObject.CreatePrimitive(PrimitiveType.Cube);
            console.name = $"Console_{i}";
            console.transform.SetParent(stationRoot);
            console.transform.position = pos + Vector3.up * 0.5f;
            console.transform.rotation = Quaternion.Euler(0, angle, 0);
            console.transform.localScale = new Vector3(2f, 1f, 0.8f);
            console.GetComponent<Renderer>().material = consoleMat;

            // Экран на пульте
            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screen.name = "Screen";
            screen.transform.SetParent(console.transform);
            screen.transform.localPosition = new Vector3(0, 0.55f, -0.1f);
            screen.transform.localRotation = Quaternion.Euler(-30f, 0, 0);
            screen.transform.localScale = new Vector3(0.8f, 0.4f, 0.05f);
            screen.GetComponent<Renderer>().material = screenMat;
            Destroy(screen.GetComponent<Collider>());

            // Кнопки
            for (int b = 0; b < 4; b++)
            {
                GameObject btn = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                btn.name = $"Button_{b}";
                btn.transform.SetParent(console.transform);
                btn.transform.localPosition = new Vector3(
                    -0.6f + b * 0.4f,
                    0.53f,
                    0.2f
                );
                btn.transform.localRotation = Quaternion.Euler(90, 0, 0);
                btn.transform.localScale = new Vector3(0.08f, 0.03f, 0.08f);
                btn.GetComponent<Renderer>().material = buttonMat;
                Destroy(btn.GetComponent<Collider>());
            }

            // Подсветка пульта
            GameObject consoleLight = new GameObject("ConsoleLight");
            consoleLight.transform.SetParent(console.transform);
            consoleLight.transform.localPosition = new Vector3(0, 0.8f, -0.3f);
            Light cl = consoleLight.AddComponent<Light>();
            cl.type = LightType.Point;
            cl.color = new Color(0.2f, 0.7f, 0.5f);
            cl.intensity = 1.5f;
            cl.range = 4f;
        }
    }

    void CreatePipes()
    {
        Material pipeMat = MakeMaterial(new Color(0.35f, 0.35f, 0.38f), 0.9f, 0.75f);

        // Вертикальные трубы в углах
        Vector3[] corners = {
            new Vector3(roomSize.x/2f - 1f, roomSize.y/2f, roomSize.z/2f - 1f),
            new Vector3(-roomSize.x/2f + 1f, roomSize.y/2f, roomSize.z/2f - 1f),
            new Vector3(roomSize.x/2f - 1f, roomSize.y/2f, -roomSize.z/2f + 1f),
            new Vector3(-roomSize.x/2f + 1f, roomSize.y/2f, -roomSize.z/2f + 1f),
        };

        foreach (Vector3 corner in corners)
        {
            GameObject pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pipe.name = "VerticalPipe";
            pipe.transform.SetParent(stationRoot);
            pipe.transform.position = corner;
            pipe.transform.localScale = new Vector3(0.3f, roomSize.y / 2f, 0.3f);
            pipe.GetComponent<Renderer>().material = pipeMat;
        }

        // Горизонтальные трубы под потолком
        for (int i = -1; i <= 1; i += 2)
        {
            GameObject hPipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hPipe.name = "HorizontalPipe";
            hPipe.transform.SetParent(stationRoot);
            hPipe.transform.position = new Vector3(0, roomSize.y - 1f, i * (roomSize.z / 2f - 1f));
            hPipe.transform.rotation = Quaternion.Euler(0, 0, 90);
            hPipe.transform.localScale = new Vector3(0.25f, roomSize.x / 2f, 0.25f);
            hPipe.GetComponent<Renderer>().material = pipeMat;
        }
    }

    void CreateEmergencyLights()
    {
        // Аварийные лампы на стенах
        Vector3[] positions = {
            new Vector3(0, roomSize.y - 1.5f, roomSize.z/2f - 0.5f),
            new Vector3(0, roomSize.y - 1.5f, -roomSize.z/2f + 0.5f),
            new Vector3(roomSize.x/2f - 0.5f, roomSize.y - 1.5f, 0),
            new Vector3(-roomSize.x/2f + 0.5f, roomSize.y - 1.5f, 0),
        };

        foreach (Vector3 pos in positions)
        {
            GameObject lampBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lampBase.name = "EmergencyLampBase";
            lampBase.transform.SetParent(stationRoot);
            lampBase.transform.position = pos;
            lampBase.transform.localScale = new Vector3(0.8f, 0.3f, 0.3f);
            lampBase.GetComponent<Renderer>().material = MakeMaterial(new Color(0.1f, 0.1f, 0.1f), 0.9f, 0.5f);

            GameObject lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lamp.name = "EmergencyLamp";
            lamp.transform.SetParent(lampBase.transform);
            lamp.transform.localPosition = Vector3.zero;
            lamp.transform.localScale = new Vector3(0.7f, 2.5f, 2.5f);
            lamp.GetComponent<Renderer>().material = MakeMaterial(
                emergencyColor, 0f, 1f, emergencyColor * 8f
            );
            Destroy(lamp.GetComponent<Collider>());

            GameObject lightObj = new GameObject("EmergencyLight");
            lightObj.transform.SetParent(lampBase.transform);
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = emergencyColor;
            light.intensity = 4f;
            light.range = 12f;
            emergencyLights.Add(light);
        }
    }

    void CreateSteamEffects()
    {
        GameObject steamObj = new GameObject("Steam");
        steamObj.transform.SetParent(stationRoot);
        steamObj.transform.position = new Vector3(reactorRadius + 1f, 0.5f, 0);

        ParticleSystem ps = steamObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 3f;
        main.startSpeed = 1.5f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 1f, 1f, 0.1f),
            new Color(0.8f, 0.9f, 1f, 0.25f)
        );
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 15f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 20f;
        shape.radius = 0.3f;

        var sizeOverLife = ps.sizeOverLifetime;
        sizeOverLife.enabled = true;
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.3f),
            new Keyframe(1f, 1.5f)
        ));

        var colorOverLife = ps.colorOverLifetime;
        colorOverLife.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.3f, 0.3f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLife.color = grad;

        var renderer = steamObj.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = new Color(0.9f, 0.95f, 1f);
    }

    void CreateCatwalk()
    {
        Material catwalkMat = MakeMaterial(new Color(0.15f, 0.15f, 0.16f), 0.9f, 0.5f);
        Material railMat = MakeMaterial(new Color(0.4f, 0.4f, 0.42f), 0.95f, 0.8f);

        // Балкон вокруг реактора
        int segments = 24;
        float radius = 5f;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * 360f / segments;
            Vector3 pos = new Vector3(
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius,
                3f,
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius
            );

            GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seg.name = "CatwalkSegment";
            seg.transform.SetParent(stationRoot);
            seg.transform.position = pos;
            seg.transform.rotation = Quaternion.Euler(0, angle, 0);
            seg.transform.localScale = new Vector3(1.5f, 0.1f, 0.8f);
            seg.GetComponent<Renderer>().material = catwalkMat;

            // Перила
            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "Rail";
            rail.transform.SetParent(seg.transform);
            rail.transform.localPosition = new Vector3(0, 0.5f, -0.35f);
            rail.transform.localScale = new Vector3(1f, 0.05f, 0.05f);
            rail.GetComponent<Renderer>().material = railMat;
            Destroy(rail.GetComponent<Collider>());

            GameObject rail2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail2.name = "RailTop";
            rail2.transform.SetParent(seg.transform);
            rail2.transform.localPosition = new Vector3(0, 1f, -0.35f);
            rail2.transform.localScale = new Vector3(1f, 0.05f, 0.05f);
            rail2.GetComponent<Renderer>().material = railMat;
            Destroy(rail2.GetComponent<Collider>());
        }
    }

    void Update()
    {
        // Пульсация аварийного света
        float pulse = 0.5f + Mathf.PerlinNoise(Time.time * 2f, 0f) * 0.5f;
        foreach (Light l in emergencyLights)
        {
            if (l != null)
                l.intensity = Mathf.Lerp(2f, 6f, pulse);
        }

        // Пульсация реактора
        foreach (Light l in reactorLights)
        {
            if (l != null)
                l.intensity = Mathf.Lerp(6f, 10f, Mathf.PerlinNoise(Time.time * 0.5f, 0f));
        }
    }
}