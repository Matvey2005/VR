using UnityEngine;
using TMPro;

public class TerminalCodeManager : MonoBehaviour
{
    [Header("Канвас 1: Генерация кода")]
    public TextMeshProUGUI codeDisplay; // Текст на первом канвасе

    [Header("Канвас 2: Ввод")]
    public TextMeshProUGUI inputDisplay; // Текст на втором канвасе

    [Header("Настройки")]
    public int codeLength = 5; // Длина кода (5 цифр)
    public GameObject prizeObject; // Объект, меняющий цвет при успехе
    public float clickCooldown = 0.3f; // Защита от двойного нажатия (в секундах)

    private string secretCode = "";
    private string playerInput = "";
    private float lastClickTime = 0f;
    private bool isCompleted = false; // Блокировка после успеха

    void Start()
    {
        GenerateNewCode();
    }

    // --- ГЕНЕРАЦИЯ КОДА ---
    public void GenerateNewCode()
    {
        isCompleted = false;
        secretCode = "";
        playerInput = "";

        if (inputDisplay != null) inputDisplay.text = "";
        if (codeDisplay != null) codeDisplay.text = "КОД: ";

        for (int i = 0; i < codeLength; i++)
        {
            // Генерируем случайную цифру от 0 до 9
            secretCode += Random.Range(0, 10).ToString();
        }

        // Выводим код на первом канвасе (с пробелами для читаемости)
        if (codeDisplay != null)
        {
            codeDisplay.text = "КОД: " + string.Join(" ", secretCode.ToCharArray());
        }

        Debug.Log("Сгенерирован код: " + secretCode);
    }

    // --- ОБРАБОТКА НАЖАТИЙ (вызывается из OnClick кнопок) ---
    public void OnDigitPressed(int digit)
    {
        Debug.Log(digit);
        if (isCompleted) return;

        // Защита от двойного клика от VR-контроллера
        if (Time.time - lastClickTime < clickCooldown) return;
        lastClickTime = Time.time;

        // Если поле уже заполнено (5 символов) и нажата еще одна кнопка — очищаем
        if (playerInput.Length >= codeLength)
        {
            playerInput = "";
        }

        playerInput += digit.ToString();

        // Обновляем текст на втором канвасе
        if (inputDisplay != null) inputDisplay.text = playerInput;

        // Проверяем совпадение после КАЖДОГО нажатия
        CheckCode();
    }

    // --- ОЧИСТКА (опционально, можно привязать к отдельной кнопке) ---
    public void OnClearPressed()
    {
        playerInput = "";
        if (inputDisplay != null) inputDisplay.text = "";
    }

    // --- ПРОВЕРКА КОДА ---
    private void CheckCode()
    {
        // Если введено меньше символов, чем нужно — проверять рано
        if (playerInput.Length < codeLength)
        {
            // Но если игрок уже ошибся на этом шаге — можно сразу сбросить
            // Сравниваем только введенную часть с началом секретного кода
            //if (!secretCode.StartsWith(playerInput))
            //{
            //    Debug.Log("Ошибка на шаге: " + playerInput.Length);
            //    ResetInput();
            //}
            return;
        }

        // Если длина совпала с кодом — проверяем полностью
        if (playerInput == secretCode)
        {
            Debug.Log("УСПЕХ! Код верный.");
            isCompleted = true;

            if (inputDisplay != null) inputDisplay.text = "УСПЕХ!";

            if (prizeObject != null)
            {
                Renderer rend = prizeObject.GetComponent<Renderer>();
                if (rend != null) rend.material.color = Color.green;
            }
        }
        else
        {
            Debug.Log("Ошибка. Код неверный.");
            ResetInput();
            
        }
    }

    private void ResetInput()
    {
        playerInput = "";
        if (inputDisplay != null) inputDisplay.text = "";
    }
}