using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class NumberSequenceGame : MonoBehaviour
{
    [Header("Настройки UI")]
    public Button[] allButtons = new Button[25]; // Все 25 кнопок по порядку в иерархии
    public TextMeshProUGUI statusText; // Текст для отображения статуса ("Нажми 1", "Ошибка", "Победа")
    public GameObject prizeObject; // Объект, меняющий цвет при успехе

    [Header("Настройки Игры")]
    public int targetCount = 16; // До какого числа нажимать (1-16)
    public float clickCooldown = 0.3f; // Защита от двойного клика в VR

    private int currentTarget = 1;
    private float lastClickTime = 0f;
    private bool isGameOver = false;

    void Start()
    {
        SetupGame();
    }

    void SetupGame()
    {
        if (allButtons.Length < 25)
        {
            Debug.LogError("В массиве allButtons должно быть 25 кнопок!");
            return;
        }

        currentTarget = 1;
        isGameOver = false;
        UpdateStatusText($"Нажми: {currentTarget}");

        // 1. Создаем список чисел от 1 до 25
        List<int> numbers = new List<int>();
        for (int i = 1; i <= 25; i++)
        {
            numbers.Add(i);
        }

        // 2. Перемешиваем их (алгоритм Фишера-Йетса)
        for (int i = numbers.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = numbers[i];
            numbers[i] = numbers[j];
            numbers[j] = temp;
        }

        // 3. Раздаем числа кнопкам и настраиваем клики
        for (int i = 0; i < allButtons.Length; i++)
        {
            if (allButtons[i] == null) continue;

            // Берем перемешанное число для этой кнопки
            int assignedNumber = numbers[i];

            // Ищем текстовый компонент внутри кнопки и записываем туда число
            TextMeshProUGUI btnText = allButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = assignedNumber.ToString();
            }
            else
            {
                Debug.LogWarning($"На кнопке {allButtons[i].name} не найден компонент TextMeshProUGUI!");
            }

            // Очищаем старые события OnClick
            allButtons[i].onClick.RemoveAllListeners();

            // Добавляем слушатель. ВАЖНО: мы передаем assignedNumber, а не индекс i!
            int numberToPass = assignedNumber;
            allButtons[i].onClick.AddListener(() => OnButtonPressed(numberToPass));
        }
    }

    public void OnButtonPressed(int pressedNumber)
    {
        if (isGameOver) return;

        // Защита от двойного клика
        if (Time.time - lastClickTime < clickCooldown) return;
        lastClickTime = Time.time;

        // Проверяем, ту ли цифру нажали
        if (pressedNumber == currentTarget)
        {
            Debug.Log($"Верно! Нажата цифра {pressedNumber}");

            // Визуальная обратная связь
            Button clickedBtn = allButtons.FirstOrDefault(b => b.GetComponentInChildren<TextMeshProUGUI>().text == pressedNumber.ToString());
            if (clickedBtn != null)
            {
                StartCoroutine(FlashButtonGreen(clickedBtn));
            }

            currentTarget++;

            // Если дошли до 16 — победа
            if (currentTarget > targetCount)
            {
                WinGame();
            }
            else
            {
                UpdateStatusText($"Нажми: {currentTarget}");
            }
        }
        else
        {
            // Ошибка
            Debug.LogWarning($"Ошибка! Нажали {pressedNumber}, а ждали {currentTarget}. Начинаем заново.");
            UpdateStatusText($"ОШИБКА!");

            Invoke(nameof(ResetGame), 1.5f);
            isGameOver = true;
        }
    }

    private void ResetGame()
    {
        SetupGame(); // Полностью перезапускаем игру с новым перемешиванием
    }

    private void WinGame()
    {
        isGameOver = true;
        UpdateStatusText("УСПЕХ!");
        if (prizeObject != null)
        {
            Renderer rend = prizeObject.GetComponent<Renderer>();
            if (rend != null) rend.material.color = Color.green;
        }
        Debug.Log("Игра пройдена! Нажаты все цифры от 1 до 16.");
    }

    private void UpdateStatusText(string text)
    {
        if (statusText != null)
            statusText.text = text;
    }

    private System.Collections.IEnumerator FlashButtonGreen(Button btn)
    {
        if (btn == null || btn.image == null) yield break;

        Color originalColor = btn.image.color;
        btn.image.color = Color.green;
        yield return new WaitForSeconds(0.2f);
        btn.image.color = originalColor;
    }
}