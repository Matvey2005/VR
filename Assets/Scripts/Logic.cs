using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class Logic : MonoBehaviour
{
    [Header("Объекты для проверки (4 шт.)")]
    [SerializeField] private GameObject obj1;
    [SerializeField] private GameObject obj2;
    [SerializeField] private GameObject obj3;
    [SerializeField] private GameObject obj4;
    [SerializeField] public TextMeshProUGUI statusText;

    [Header("Звук серены")]
    public AudioSource audioSource; // Перетащите сюда ваш Audio Source в инспекторе
    public AudioSource audioSource2; // Перетащите сюда ваш Audio Source в инспекторе
    public AudioClip newClip;
    public AudioClip newClip1;
    public Canvas canvas;

    [Header("Настройки")]
    [SerializeField] private float timerDuration = 300f; // 5 минут в секундах
    [SerializeField] private bool logEverySecond = false; // спамить в консоль раз в секунду

    private List<GameObject> targetObjects = new List<GameObject>();

    private int greenCount = 0;
    private float timeRemaining;
    private bool isRunning = true;
    private bool isFinished = false;

    private float logTimer = 0f;

    private void Start()
    {
        targetObjects.Add(obj1);
        targetObjects.Add(obj2);
        targetObjects.Add(obj3);
        targetObjects.Add(obj4);

        timeRemaining = timerDuration;

        Debug.Log($"[Logic] Старт. Таймер: {FormatTime(timeRemaining)}. Объектов для проверки: {CountValidObjects()}");
    }

    private void Update()
    {
        if (!isRunning || isFinished) return;

        // Таймер
        timeRemaining -= Time.deltaTime;

        // Лог раз в секунду (опционально)
        if (logEverySecond)
        {
            logTimer += Time.deltaTime;
            if (logTimer >= 1f)
            {
                logTimer = 0f;
                statusText.text = $"{FormatTime(timeRemaining)}"; // ИСПРАВЛЕНО: добавлена ;
                Debug.Log($"[Logic] Осталось: {FormatTime(timeRemaining)} | Зелёных: {greenCount}/4");
            }
        }

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isRunning = false;
            canvas.enabled = true;
            audioSource2.enabled = false;
            audioSource.clip = newClip1;
            audioSource.loop = false;
            audioSource.Play();

            Debug.LogWarning($"[Logic] Время вышло! Итоговый счёт зелёных: {greenCount}/4");
        }

        // Подсчёт зелёных объектов
        int currentGreen = 0;
        for (int i = 0; i < targetObjects.Count; i++)
        {
            GameObject obj = targetObjects[i];
            if (obj == null) continue;

            Renderer rend = obj.GetComponent<Renderer>();
            if (rend != null && IsGreen(rend.material.color))
            {
                currentGreen++;
            }
        }

        // Обновляем счётчик только если изменилось
        if (currentGreen != greenCount)
        {
            greenCount = currentGreen;
            Debug.Log($"[Logic] Счётчик изменился → {greenCount}/4");
        }

        // Если все 4 зелёные — останавливаемся
        if (greenCount >= 4)
        {
            isFinished = true;
            isRunning = false;
            statusText.text = "Успех!"; // ИСПРАВЛЕНО: добавлена ;
            audioSource.clip = newClip;
            audioSource.loop = false;
            audioSource.Play();
            Debug.Log($"[Logic] ВСЕ 4 ОБЪЕКТА ЗЕЛЁНЫЕ! Скрипт остановлен. Осталось времени: {FormatTime(timeRemaining)}");
        }
    }

    private bool IsGreen(Color color)
    {
        return color.g > 0.5f && color.g > color.r && color.g > color.b;
    }

    private int CountValidObjects()
    {
        int count = 0;
        foreach (var obj in targetObjects)
            if (obj != null) count++;
        return count;
    }

    private string FormatTime(float t)
    {
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    // Публичные методы на случай внешнего управления
    public int GetGreenCount() => greenCount;
    public float GetTimeRemaining() => timeRemaining;
    public bool IsFinished() => isFinished;
} // ИСПРАВЛЕНО: добавлена закрывающая скобка класса