using UnityEditor;
using UnityEngine;

namespace dsystem
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Nodes/Sentence Node", fileName = "New Sentence Node")]
    public class SentenceNode : Node
    {
        [SerializeField] private Sentence sentence;

        [Space(10)]
        public Node parentNode;
        public Node childNode;

        [Space(7)]
        [SerializeField] private bool isExternalFunc;
        [SerializeField] private string externalFunctionName;

        private string externalButtonLable;

        private const float lableFieldSpace = 30f;
        private const float textFieldWidth = 60f;

        private const float externalNodeHeight = 60f;

        /// Zwracanie nazwy funkcji zewnêtrznej
        public string GetExternalFunctionName()
        {
            return externalFunctionName;
        }
        /// Zwracanie nazwy postaci zdania
        public string GetSentenceCharacterName()
        {
            return sentence.characterName;
        }
        /// Ustawianie tekstu zdania
        public void SetSentenceText(string text)
        {
            sentence.text = text;
        }
        /// Zwracanie tekstu zdania
        public string GetSentenceText()
        {
            return sentence.text;
        }
        /// Zwracanie sprite'a postaci zdania
        public Sprite GetCharacterSprite()
        {
            return sentence.characterSprite;
        }
        /// Zwraca wartoœæ pola logicznego isExternalFunc
        public bool IsExternalFunc()
        {
            return isExternalFunc;
        }
        /// Metoda rysowania wêz³a zdania
        public override void Draw(GUIStyle nodeStyle, GUIStyle lableStyle)
        {
            base.Draw(nodeStyle, lableStyle);

            GUILayout.BeginArea(rect, nodeStyle);

            EditorGUILayout.LabelField("Sentence Node", lableStyle);

            DrawCharacterNameFieldHorizontal();
            DrawSentenceTextFieldHorizontal();
            DrawCharacterSpriteHorizontal();
            DrawExternalFunctionTextField();

            if (GUILayout.Button(externalButtonLable))
            {
                isExternalFunc = !isExternalFunc;

            }

            GUILayout.EndArea();
        }
        /// Narysuj etykietê i pola tekstowe dla nazwy postaci
        private void DrawCharacterNameFieldHorizontal()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Name ", GUILayout.Width(lableFieldSpace));
            sentence.characterName = EditorGUILayout.TextField(sentence.characterName, GUILayout.Width(textFieldWidth));
            EditorGUILayout.EndHorizontal();
        }
        /// Narysuj etykietê i pola tekstowe dla tekstu zdania
        private void DrawSentenceTextFieldHorizontal()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Text ", GUILayout.Width(lableFieldSpace));
            sentence.text = EditorGUILayout.TextField(sentence.text, GUILayout.Width(textFieldWidth));
            EditorGUILayout.EndHorizontal();
        }
        /// Narysuj etykietê i pola tekstowe dla sprite'a postaci
        private void DrawCharacterSpriteHorizontal()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Sprite ", GUILayout.Width(lableFieldSpace));
            sentence.characterSprite = (Sprite)EditorGUILayout.ObjectField(sentence.characterSprite,
                typeof(Sprite), false, GUILayout.Width(textFieldWidth));
            EditorGUILayout.EndHorizontal();
        }
        /// Narysuj etykietê i pola tekstowe dla funkcji zewnêtrznej
        private void DrawExternalFunctionTextField()
        {
            if (isExternalFunc)
            {
                externalButtonLable = "Remove external func";

                EditorGUILayout.BeginHorizontal();
                rect.height = externalNodeHeight;
                EditorGUILayout.LabelField($"Func Name ", GUILayout.Width(lableFieldSpace));
                externalFunctionName = EditorGUILayout.TextField(externalFunctionName,
                    GUILayout.Width(textFieldWidth));
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                externalButtonLable = "Add external func";
                rect.height = standartHeight;
            }
        }
        /// Sprawdzanie rozmiaru wêz³a
        public void CheckNodeSize(float width, float height)
        {
            rect.width = width;
            
            if (standartHeight == 0)
            {
                standartHeight = height;
            }

            if (isExternalFunc)
            {
                rect.height = externalNodeHeight;
            }
            else
            {
                rect.height = standartHeight;
            }
        }
        ///Dodawanie wêz³a "nodeToAdd" do pola "childNode"
        public override bool AddToChildConnectedNode(Node nodeToAdd)
        {
            SentenceNode sentenceNodeToAdd;

            if (nodeToAdd.GetType() == typeof(SentenceNode))
            {
                nodeToAdd = (SentenceNode)nodeToAdd;

                if (nodeToAdd == this)
                {
                    return false;
                }
            }

            if (nodeToAdd.GetType() == typeof(SentenceNode))
            {
                sentenceNodeToAdd = (SentenceNode)nodeToAdd;

                if (sentenceNodeToAdd != null && sentenceNodeToAdd.childNode == this)
                {
                    return false;
                }
            }

            childNode = nodeToAdd;
            return true;
        }
        /// Dodawanie wêz³a nodeToAdd do pola parentNode
        public override bool AddToParentConnectedNode(Node nodeToAdd)
        {
            SentenceNode sentenceNodeToAdd;

            if (nodeToAdd.GetType() == typeof(AnswerNode))
            {
                return false;
            }

            if (nodeToAdd.GetType() == typeof(SentenceNode))
            {
                nodeToAdd = (SentenceNode)nodeToAdd;

                if (nodeToAdd == this)
                {
                    return false;
                }
            }

            parentNode = nodeToAdd;

            if (nodeToAdd.GetType() == typeof(SentenceNode))
            {
                sentenceNodeToAdd = (SentenceNode)nodeToAdd;

                if (sentenceNodeToAdd.childNode == this)
                {
                    return true;
                }
                else
                {
                    parentNode = null;
                }
            }

            return true;
        }
    }
}