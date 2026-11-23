using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class BatterySlotManager : MonoBehaviour
{
    [Header("Ссылки на сокеты")]
    [Tooltip("Перетащите сюда все 4 объекта с компонентом XR Socket Interactor")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor[] sockets = new UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor[4];

    [Header("UI")]
    [Tooltip("Текст на канвасе для отображения количества батареек")]
    public TextMeshProUGUI counterText;

    [Header("Объект для победы")]
    [Tooltip("Объект, который станет зеленым, когда вставят все 4 батарейки")]
    public GameObject winObject;

    private int currentBatteryCount = 0;
    private bool isVictory = false;

    void Start()
    {
        // Подписываемся на события каждого сокета
        foreach (var socket in sockets)
        {
            if (socket != null)
            {
                socket.selectEntered.AddListener(OnSocketFilled);
                socket.selectExited.AddListener(OnSocketEmptied);
            }
        }

        UpdateUI();
    }

    // Вызывается, когда в сокет вставляют предмет
    private void OnSocketFilled(SelectEnterEventArgs args)
    {
        currentBatteryCount++;
        Debug.Log($"Батарейка вставлена! Всего: {currentBatteryCount}");
        UpdateUI();
        CheckWinCondition();
    }

    // Вызывается, когда предмет вытаскивают из сокета
    private void OnSocketEmptied(SelectExitEventArgs args)
    {
        currentBatteryCount--;
        // Защита от ухода в минус (на всякий случай)
        if (currentBatteryCount < 0) currentBatteryCount = 0;

        Debug.Log($"Батарейка извлечена! Осталось: {currentBatteryCount}");

        // Если мы уже победили, но вытащили батарейку — сбрасываем победу
        if (isVictory)
        {
            isVictory = false;
            if (winObject != null)
            {
                Renderer rend = winObject.GetComponent<Renderer>();
                if (rend != null) rend.material.color = Color.white; // Возвращаем обычный цвет
            }
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (counterText != null)
        {
            counterText.text = $"Батарейки: {currentBatteryCount} / 4";
        }
    }

    private void CheckWinCondition()
    {
        // Если вставлено 4 батарейки и мы еще не победили
        if (currentBatteryCount >= 4 && !isVictory)
        {
            isVictory = true;
            Debug.Log("ВСЕ БАТАРЕЙКИ ВСТАВЛЕНЫ! УСПЕХ!");

            if (winObject != null)
            {
                Renderer rend = winObject.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.color = Color.green; // Меняем цвет на зеленый
                }
            }

            if (counterText != null)
            {
                counterText.text = "ПИТАНИЕ ПОДКЛЮЧЕНО!";
            }
        }
    }
}