using UnityEngine;

public class FloatingCrystal : MonoBehaviour
{
    public float rotationSpeed = 10f;
    public float floatAmplitude = 0.5f;
    public float floatSpeed = 1f;

    private Vector3 startPos;
    private float offset;

    void Start()
    {
        startPos = transform.position;
        offset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        // Вращение
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.right, rotationSpeed * 0.5f * Time.deltaTime, Space.World);

        // Плавное парение
        float y = Mathf.Sin(Time.time * floatSpeed + offset) * floatAmplitude;
        transform.position = startPos + Vector3.up * y;
    }
}