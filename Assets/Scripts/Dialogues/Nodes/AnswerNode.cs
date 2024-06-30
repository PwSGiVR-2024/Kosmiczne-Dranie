using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace dsystem
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Nodes/Answer Node", fileName = "New Answer Node")]
    public class AnswerNode : Node
    {
        private int amountOfAnswers = 1;

        public List<string> answers = new List<string>();

        public SentenceNode parentSentenceNode;
        public List<SentenceNode> childSentenceNodes = new List<SentenceNode>();

        private const float lableFieldSpace = 18f;
        private const float textFieldWidth = 120f;

        private const float answerNodeWidth = 190f;
        private const float answerNodeHeight = 115f;

        private float currentAnswerNodeHeight = 115f;
        private float additionalAnswerNodeHeight = 20f;
        /// Metoda inicjalizacji wêz³a odpowiedzi
        public override void Initialise(Rect rect, string nodeName, DialogNodeGraph nodeGraph)
        {
            base.Initialise(rect, nodeName, nodeGraph);

            CalculateAmountOfAnswers();

            childSentenceNodes = new List<SentenceNode>(amountOfAnswers);
        }
#if UNITY_EDITOR
        /// Metoda rysowania wêz³a odpowiedzi
        public override void Draw(GUIStyle nodeStyle, GUIStyle lableStyle)
        {
            base.Draw(nodeStyle, lableStyle);

            childSentenceNodes.RemoveAll(item => item == null);

            rect.size = new Vector2(answerNodeWidth, currentAnswerNodeHeight);

            GUILayout.BeginArea(rect, nodeStyle);
            EditorGUILayout.LabelField("Answer Node", lableStyle);

            for (int i = 0; i < amountOfAnswers; i++)
            {
                DrawAnswerLine(i + 1, StringConstants.GreenDot);
            }

            DrawAnswerNodeButtons();

            GUILayout.EndArea();
        }
#endif
        /// Okreœla liczbê odpowiedzi w zale¿noœci od liczby elementów na liœcie odpowiedzi
        public void CalculateAmountOfAnswers()
        {
            if (answers.Count == 0)
            {
                amountOfAnswers = 1;

                answers = new List<string>() { string.Empty };
            }
            else
            {
                amountOfAnswers = answers.Count;
            }
        }
#if UNITY_EDITOR
        /// Narysuj liniê odpowiedzi
        private void DrawAnswerLine(int answerNumber, string iconPathOrName)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"{answerNumber}. ", 
                GUILayout.Width(lableFieldSpace));

            answers[answerNumber - 1] = EditorGUILayout.TextField(answers[answerNumber - 1], 
                GUILayout.Width(textFieldWidth));

            EditorGUILayout.LabelField(EditorGUIUtility.IconContent(iconPathOrName), 
                GUILayout.Width(lableFieldSpace));

            EditorGUILayout.EndHorizontal();
        }
#endif

        private void DrawAnswerNodeButtons()
        {
            if (GUILayout.Button("Add answer"))
            {
                IncreaseAmountOfAnswers();
            }

            if (GUILayout.Button("Remove answer"))
            {
                DecreaseAmountOfAnswers();
            }
        }
        /// Zwiêksz liczbê odpowiedzi i wysokoœæ wêz³a
        private void IncreaseAmountOfAnswers()
        {
            amountOfAnswers++;

            answers.Add(string.Empty);

            currentAnswerNodeHeight += additionalAnswerNodeHeight;
        }
        /// Zmniejsz liczbê odpowiedzi i wysokoœæ wêz³a
        private void DecreaseAmountOfAnswers()
        {
            if (answers.Count == 1)
            {
                return;
            }

            answers.RemoveAt(amountOfAnswers - 1);

            if (childSentenceNodes.Count == amountOfAnswers)
            {
                childSentenceNodes[amountOfAnswers - 1].parentNode = null;
                childSentenceNodes.RemoveAt(amountOfAnswers - 1);
            }

            amountOfAnswers--;

            currentAnswerNodeHeight -= additionalAnswerNodeHeight;
        }
        /// Dodanie wêz³a nodeToAdd do pola parentSentenceNode
        public override bool AddToParentConnectedNode(Node nodeToAdd)
        {
            if (nodeToAdd.GetType() == typeof(SentenceNode))
            {
                parentSentenceNode = (SentenceNode)nodeToAdd;

                return true;
            }

            return false;
        }
        /// Dodawanie wêz³a nodeToAdd do tablicy childSentenceNodes
        public override bool AddToChildConnectedNode(Node nodeToAdd)
        {
            SentenceNode sentenceNodeToAdd;

            if (nodeToAdd.GetType() != typeof(AnswerNode))
            {
                sentenceNodeToAdd = (SentenceNode)nodeToAdd;
            }
            else
            {
                return false;
            }

            if (IsCanAddToChildConnectedNode(sentenceNodeToAdd))
            {
                childSentenceNodes.Add(sentenceNodeToAdd);

                sentenceNodeToAdd.parentNode = this;

                return true;
            }

            return false;
        }
        /// Oblicz wysokoœæ wêz³a odpowiedzi na podstawie liczby odpowiedzi
        public void CalculateAnswerNodeHeight()
        {
            currentAnswerNodeHeight = answerNodeHeight;

            for (int i = 0; i < amountOfAnswers - 1; i++)
            {
                currentAnswerNodeHeight += additionalAnswerNodeHeight;
            }
        }

        private bool IsCanAddToChildConnectedNode(SentenceNode sentenceNodeToAdd)
        {
            return sentenceNodeToAdd.parentNode == null 
                && childSentenceNodes.Count < amountOfAnswers 
                && sentenceNodeToAdd.childNode != this;
        }
    }
}