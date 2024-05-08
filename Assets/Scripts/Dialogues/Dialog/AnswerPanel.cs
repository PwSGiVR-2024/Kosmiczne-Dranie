using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace dsystem
{
    public class AnswerPanel : MonoBehaviour
    {
        [SerializeField] private Button answerButtonPrefab;
        [SerializeField] private Transform parentTransform;

        private List<Button> buttons = new List<Button>();
        private List<TextMeshProUGUI> buttonTexts = new List<TextMeshProUGUI>();
        /// Instancjonowanie przycisków odpowiedzi na podstawie maksymalnej liczby przycisków odpowiedzi
        public void SetUpButtons(int maxAmountOfAnswerButtons)
        {
            for (int i = 0; i < maxAmountOfAnswerButtons; i++)
            {
                Button answerButton = Instantiate(answerButtonPrefab, parentTransform);

                buttons.Add(answerButton);
                buttonTexts.Add(answerButton.GetComponentInChildren<TextMeshProUGUI>());
            }
        }

        /// Zwracanie przycisku na podstawie indeksu
        public Button GetButtonByIndex(int index)
        {
            return buttons[index];
        }
        /// Zwracanie tekstu przycisku na podstawie indeksu
        public TextMeshProUGUI GetButtonTextByIndex(int index)
        {
            return buttonTexts[index];
        }
        /// Ustawienie UnityAction na zdarzenie onClick przycisku na podstawie indeksu
        public void AddButtonOnClickListener(int index, UnityAction action)
        {
            buttons[index].onClick.AddListener(action);
        }
        /// W³¹czenie okreœlonej liczby przycisków
        public void EnableCertainAmountOfButtons(int amount)
        {
            if (buttons.Count == 0)
            {
                Debug.LogWarning("Proszê przypisaæ listê przycisków!");
                return;
            }

            for (int i = 0; i < amount; i++)
            {
                buttons[i].gameObject.SetActive(true);
            }
        }
        /// Wy³¹czenie wszystkich przycisków
        public void DisalbleAllButtons()
        {
            foreach (Button button in buttons)
            {
                button.gameObject.SetActive(false);
            }
        }
    }
}