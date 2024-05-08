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

        /// Ustawienie opóŸnienia miêdzy znakami w dialogu (parametr float dialogCharDelay)
        public void SetCharDelay(float value)
        {
            dialogCharDelay = value;
        }

        /// Ustawienie klawiszy do przejœcia do nastêpnego zdania
        public void SetNextSentenceKeyCodes(List<KeyCode> keyCodes)
        {
            nextSentenceKeyCodes = keyCodes;
        }

        /// Rozpoczêcie dialogu
        public void StartDialog(DialogNodeGraph dialogNodeGraph)
        {
            isDialogStarted = true;

            if (dialogNodeGraph.nodesList == null)
            {
                Debug.LogWarning("Lista wêz³ów w grafie dialogowym jest pusta");
                return;
            }

            onDialogStarted?.Invoke();

            currentNodeGraph = dialogNodeGraph;

            DefineFirstNode(dialogNodeGraph);
            CalculateMaxAmountOfAnswerButtons();
            HandleDialogGraphCurrentNode(currentNode);
        }

        /// Ta metoda jest przeznaczona dla wygody. Wywo³uje metodê BindExternalFunction z klasy DialogExternalFunctionsHandler
        public void BindExternalFunction(string funcName, Action function)
        {
            ExternalFunctionsHandler.BindExternalFunction(funcName, function);
        }

        /// Dodanie nas³uchiwacza do zdarzenia UnityEvent OnDialogFinished
        public void AddListenerToDialogFinishedEvent(UnityAction action)
        {
            onDialogFinished.AddListener(action);
        }

        /// Ustawienie bie¿¹cego wêz³a na Node i wywo³anie metody HandleDialogGraphCurrentNode
        public void SetCurrentNodeAndHandleDialogGraph(Node node)
        {
            currentNode = node;
            HandleDialogGraphCurrentNode(this.currentNode);
        }

        /// Obs³uga bie¿¹cego wêz³a dialogowego
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

        /// Obs³uga wêz³a zdania
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

        /// Obs³uga wêz³a odpowiedzi
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

        /// Znalezienie pierwszego wêz³a, który nie ma wêz³a nadrzêdnego, ale ma potomny
        private void DefineFirstNode(DialogNodeGraph dialogNodeGraph)
        {
            if (dialogNodeGraph.nodesList.Count == 0)
            {
                Debug.LogWarning("Lista wêz³ów w DialogNodeGraph jest pusta");
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

        /// Sprawdzenie, czy nastêpny wêze³ dialogowy ma wêze³ potomny
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

        /// Obliczenie maksymalnej liczby przycisków odpowiedzi
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

        /// Obs³uga mechanizmu przeskakiwania tekstu
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

        /// Sprawdzenie, czy przynajmniej jeden klawisz z nextSentenceKeyCodes zosta³ naciœniêty
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