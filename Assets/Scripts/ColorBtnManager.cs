using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ColorBtnManager : MonoBehaviour
{
    [Header("Настройки UI")]
    public Button[] buttons;
    public Image[] buttonImages;
    public Color normalColor = Color.gray;
    public Color highlightColor = Color.white;

    [Header("Настройки Игры")]
    public float highlightDuration = 0.5f;
    public float delayBetweenHighlights = 0.2f;
    public GameObject prizeObject;
    public int winLength = 5; // <-- Сколько шагов нужно пройти для победы

    private List<int> sequence = new List<int>();
    private int playerIndex = 0;
    private bool isShowingSequence = false;
    private bool isPlayerTurn = false;
    private bool waitingForRelease = false;

    void Start()
    {
        ResetButtonColors();
        StartNewRound();
    }

    void StartNewRound()
    {
        sequence.Add(Random.Range(0, buttons.Length));
        playerIndex = 0;
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        isShowingSequence = true;
        isPlayerTurn = false;

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < sequence.Count; i++)
        {
            int buttonIndex = sequence[i];
            buttonImages[buttonIndex].color = highlightColor;
            yield return new WaitForSeconds(highlightDuration);
            buttonImages[buttonIndex].color = normalColor;
            yield return new WaitForSeconds(delayBetweenHighlights);
        }

        isShowingSequence = false;
        isPlayerTurn = true;
        Debug.Log("Твой ход! Длина последовательности: " + sequence.Count);
    }

    // ВАЖНО: Этот метод должен быть привязан к OnClick() кнопок
    public void OnButtonPressed(int buttonIndex)
    {
        // Если мы ждем отпускания кнопки, игнорируем всё
        if (waitingForRelease) return;

        if (!isPlayerTurn || isShowingSequence)
        {
            Debug.Log("Ввод заблокирован.");
            return;
        }

        Debug.Log($"Игрок нажал: {buttonIndex}. Ожидалось: {sequence[playerIndex]}");

        if (buttonIndex == sequence[playerIndex])
        {
            StartCoroutine(FlashButton(buttonIndex));
            playerIndex++;

            // Включаем блокировку до конца кадра, чтобы избежать двойного клика
            waitingForRelease = true;
            StartCoroutine(ReleaseLock());

            if (playerIndex >= sequence.Count)
            {
                isPlayerTurn = false;

                if (sequence.Count >= winLength)
                {
                    StartCoroutine(WinGameRoutine());
                }
                else
                {
                    StartCoroutine(NextRoundDelay());
                }
            }
        }
        else
        {
            Debug.LogError($"ОШИБКА! Игрок нажал {buttonIndex}, а ждали {sequence[playerIndex]}. Рестарт.");
            sequence.Clear();
            StartCoroutine(RestartGame());
        }
    }

    IEnumerator NextRoundDelay()
    {
        yield return new WaitForSeconds(1f);
        StartNewRound();
    }

    IEnumerator WinGameRoutine()
    {
        Debug.Log("ПОБЕДА!");
        yield return new WaitForSeconds(1f);

        if (prizeObject != null)
        {
            Renderer rend = prizeObject.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = new Color(0f, 1f, 0f); // Чистый зеленый
            }
        }

        // Можно добавить рестарт после победы
        // yield return new WaitForSeconds(3f);
        // sequence.Clear();
        // StartNewRound();
    }

    IEnumerator RestartGame()
    {
        isPlayerTurn = false;
        isShowingSequence = true;
        yield return new WaitForSeconds(1f);
        ResetButtonColors();
        StartNewRound();
    }

    IEnumerator FlashButton(int index)
    {
        buttonImages[index].color = highlightColor;
        yield return new WaitForSeconds(0.15f);
        buttonImages[index].color = normalColor;
    }

    void ResetButtonColors()
    {
        foreach (var img in buttonImages)
        {
            img.color = normalColor;
        }
    }

    IEnumerator ReleaseLock()
    {
        // Ждем один кадр, чтобы двойной клик от XR не прошел
        yield return null;
        // Можно подождать чуть дольше, если контроллер шлет клики с задержкой
        yield return new WaitForSeconds(0.1f);
        waitingForRelease = false;
    }
}
