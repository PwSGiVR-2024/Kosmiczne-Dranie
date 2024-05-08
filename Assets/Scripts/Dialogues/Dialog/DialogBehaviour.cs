using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace dsystem
{
    public class DialogBehaviour : MonoBehaviour
    {
        [SerializeField] private float dialogCharDelay;
        [SerializeField] private List<KeyCode> nextSentenceKeyCodes;
        [SerializeField] private bool isCanSkippingText = true;

        [Space(10)]
        [SerializeField] private UnityEvent onDialogStarted;
        [SerializeField] private UnityEvent onDialogFinished;

        private DialogNodeGraph currentNodeGraph;
        private Node currentNode;

        private int maxAmountOfAnswerButtons;

        private bool isDialogStarted;
        private bool isCurrentSentenceSkipped;

        public bool IsCanSkippingText
        {
            get
            {
                return isCanSkippingText;
            }
            set
            {
                isCanSkippingText = value;
            }
        }

        public event Action OnSentenceNodeActive;

        public event Action<string, string, Sprite> OnSentenceNodeActiveWithParameter;

        public event Action OnAnswerNodeActive;

        public event Action<int, AnswerNode> OnAnswerButtonSetUp;

        public event Action<int> OnMaxAmountOfAnswerButtonsCalculated;

        public event Action<int> OnAnswerNodeActiveWithParameter;

        public event Action<int, string> OnAnswerNodeSetUp;

        public event Action OnDialogTextCharWrote;

        public event Action<string> OnDialogTextSkipped;

        public DialogExternalFunctionsHandler ExternalFunctionsHandler { get; private set; }

        private void Awake()
        {
            ExternalFunctionsHandler = new DialogExternalFunctionsHandler();
        }

        private void Update()
        {
            HandleSentenceSkipping();
        }

        /// Ustawienie op�nienia mi�dzy znakami w dialogu (parametr float dialogCharDelay)
        public void SetCharDelay(float value)
        {
            dialogCharDelay = value;
        }

        /// Ustawienie klawiszy do przej�cia do nast�pnego zdania
        public void SetNextSentenceKeyCodes(List<KeyCode> keyCodes)
        {
            nextSentenceKeyCodes = keyCodes;
        }

        /// Rozpocz�cie dialogu
        public void StartDialog(DialogNodeGraph dialogNodeGraph)
        {
            isDialogStarted = true;

            if (dialogNodeGraph.nodesList == null)
            {
                Debug.LogWarning("Lista w�z��w w grafie dialogowym jest pusta");
                return;
            }

            onDialogStarted?.Invoke();

            currentNodeGraph = dialogNodeGraph;

            DefineFirstNode(dialogNodeGraph);
            CalculateMaxAmountOfAnswerButtons();
            HandleDialogGraphCurrentNode(currentNode);
        }

        /// Ta metoda jest przeznaczona dla wygody. Wywo�uje metod� BindExternalFunction z klasy DialogExternalFunctionsHandler
        public void BindExternalFunction(string funcName, Action function)
        {
            ExternalFunctionsHandler.BindExternalFunction(funcName, function);
        }

        /// Dodanie nas�uchiwacza do zdarzenia UnityEvent OnDialogFinished
        public void AddListenerToDialogFinishedEvent(UnityAction action)
        {
            onDialogFinished.AddListener(action);
        }

        /// Ustawienie bie��cego w�z�a na Node i wywo�anie metody HandleDialogGraphCurrentNode
        public void SetCurrentNodeAndHandleDialogGraph(Node node)
        {
            currentNode = node;
            HandleDialogGraphCurrentNode(this.currentNode);
        }

        /// Obs�uga bie��cego w�z�a dialogowego
        private void HandleDialogGraphCurrentNode(Node currentNode)
        {
            StopAllCoroutines();

            if (currentNode.GetType() == typeof(SentenceNode))
            {
                HandleSentenceNode(currentNode);
            }
            else if (currentNode.GetType() == typeof(AnswerNode))
            {
                HandleAnswerNode(currentNode);
            }
        }

        /// Obs�uga w�z�a zdania
        private void HandleSentenceNode(Node currentNode)
        {
            SentenceNode sentenceNode = (SentenceNode)currentNode;

            isCurrentSentenceSkipped = false;

            OnSentenceNodeActive?.Invoke();
            OnSentenceNodeActiveWithParameter?.Invoke(sentenceNode.GetSentenceCharacterName(), sentenceNode.GetSentenceText(),
                sentenceNode.GetCharacterSprite());

            if (sentenceNode.IsExternalFunc())
            {
                ExternalFunctionsHandler.CallExternalFunction(sentenceNode.GetExternalFunctionName());
            }

            WriteDialogText(sentenceNode.GetSentenceText());
        }

        /// Obs�uga w�z�a odpowiedzi
        private void HandleAnswerNode(Node currentNode)
        {
            AnswerNode answerNode = (AnswerNode)currentNode;

            int amountOfActiveButtons = 0;

            OnAnswerNodeActive?.Invoke();

            for (int i = 0; i < answerNode.childSentenceNodes.Count; i++)
            {
                if (answerNode.childSentenceNodes[i] != null)
                {
                    OnAnswerNodeSetUp?.Invoke(i, answerNode.answers[i]);
                    OnAnswerButtonSetUp?.Invoke(i, answerNode);

                    amountOfActiveButtons++;
                }
                else
                {
                    break;
                }
            }

            if (amountOfActiveButtons == 0)
            {
                isDialogStarted = false;

                onDialogFinished?.Invoke();
                return;
            }

            OnAnswerNodeActiveWithParameter?.Invoke(amountOfActiveButtons);
        }

        /// Znalezienie pierwszego w�z�a, kt�ry nie ma w�z�a nadrz�dnego, ale ma potomny
        private void DefineFirstNode(DialogNodeGraph dialogNodeGraph)
        {
            if (dialogNodeGraph.nodesList.Count == 0)
            {
                Debug.LogWarning("Lista w�z��w w DialogNodeGraph jest pusta");
                return;
            }

            foreach (Node node in dialogNodeGraph.nodesList)
            {
                currentNode = node;

                if (node.GetType() == typeof(SentenceNode))
                {
                    SentenceNode sentenceNode = (SentenceNode)node;

                    if (sentenceNode.parentNode == null && sentenceNode.childNode != null)
                    {
                        currentNode = sentenceNode;
                        return;
                    }
                }
            }

            currentNode = dialogNodeGraph.nodesList[0];
        }

        /// Napisanie tekstu dialogowego
        private void WriteDialogText(string text)
        {
            StartCoroutine(WriteDialogTextRoutine(text));
        }

        /// Kolejka napisania tekstu dialogowego
        private IEnumerator WriteDialogTextRoutine(string text)
        {
            foreach (char textChar in text)
            {
                if (isCurrentSentenceSkipped)
                {
                    OnDialogTextSkipped?.Invoke(text);
                    break;
                }

                OnDialogTextCharWrote?.Invoke();

                yield return new WaitForSeconds(dialogCharDelay);
            }

            yield return new WaitUntil(CheckNextSentenceKeyCodes);

            CheckForDialogNextNode();
        }

        /// Sprawdzenie, czy nast�pny w�ze� dialogowy ma w�ze� potomny
        private void CheckForDialogNextNode()
        {
            if (currentNode.GetType() == typeof(SentenceNode))
            {
                SentenceNode sentenceNode = (SentenceNode)currentNode;

                if (sentenceNode.childNode != null)
                {
                    currentNode = sentenceNode.childNode;
                    HandleDialogGraphCurrentNode(currentNode);
                }
                else
                {
                    isDialogStarted = false;

                    onDialogFinished?.Invoke();
                }
            }
        }

        /// Obliczenie maksymalnej liczby przycisk�w odpowiedzi
        private void CalculateMaxAmountOfAnswerButtons()
        {
            foreach (Node node in currentNodeGraph.nodesList)
            {
                if (node.GetType() == typeof(AnswerNode))
                {
                    AnswerNode answerNode = (AnswerNode)node;

                    if (answerNode.answers.Count > maxAmountOfAnswerButtons)
                    {
                        maxAmountOfAnswerButtons = answerNode.answers.Count;
                    }
                }
            }

            OnMaxAmountOfAnswerButtonsCalculated?.Invoke(maxAmountOfAnswerButtons);
        }

        /// Obs�uga mechanizmu przeskakiwania tekstu
        private void HandleSentenceSkipping()
        {
            if (!isDialogStarted || !isCanSkippingText)
            {
                return;
            }

            if (CheckNextSentenceKeyCodes() && !isCurrentSentenceSkipped)
            {
                isCurrentSentenceSkipped = true;
            }
        }

        /// Sprawdzenie, czy przynajmniej jeden klawisz z nextSentenceKeyCodes zosta� naci�ni�ty
        private bool CheckNextSentenceKeyCodes()
        {
            for (int i = 0; i < nextSentenceKeyCodes.Count; i++)
            {
                if (Input.GetKeyDown(nextSentenceKeyCodes[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}