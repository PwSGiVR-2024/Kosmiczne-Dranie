using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace dsystem
{
    public class NodeEditor : EditorWindow
    {
        private static DialogNodeGraph currentNodeGraph;
        private Node currentNode;

        private GUIStyle nodeStyle;
        private GUIStyle selectedNodeStyle;

        private GUIStyle lableStyle;

        private Rect selectionRect;
        private Vector2 mouseScrollClickPosition;

        private Vector2 graphOffset;
        private Vector2 graphDrag;

        private const float nodeWidth = 190f;
        private const float nodeHeight = 135f;

        private const float connectingLineWidth = 2f;
        private const float connectingLineArrowSize = 4f;

        private const int lableFontSize = 12;

        private const int nodePadding = 20;
        private const int nodeBorder = 10;

        private const float gridLargeLineSpacing = 100f;
        private const float gridSmallLineSpacing = 25;

        private bool isScrollWheelDragging = false;

        /// Definiowanie parametr�w stylu w�z��w i etykiet po w��czeniu
        private void OnEnable()
        {
            Selection.selectionChanged += ChangeEditorWindowOnSelection;

            nodeStyle = new GUIStyle();
            nodeStyle.normal.background = EditorGUIUtility.Load(StringConstants.Node) as Texture2D;
            nodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
            nodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

            selectedNodeStyle = new GUIStyle();
            selectedNodeStyle.normal.background = EditorGUIUtility.Load(StringConstants.SelectedNode) as Texture2D;
            selectedNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
            selectedNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

            lableStyle = new GUIStyle();
            lableStyle.alignment = TextAnchor.MiddleLeft;
            lableStyle.fontSize = lableFontSize;
            lableStyle.normal.textColor = Color.white;
        }
        /// Zapisywanie wszystkich zmian i wyrejestrowywanie si� z zdarze�
        private void OnDisable()
        {
            Selection.selectionChanged -= ChangeEditorWindowOnSelection;

            AssetDatabase.SaveAssets();
            SaveChanges();
        }

        /// Otw�rz okno edytora w�z��w, gdy zas�b grafu w�z��w zostanie podw�jnie klikni�ty w inspektorze
        [OnOpenAsset(0)]
        public static bool OnDoubleClickAsset(int instanceID, int line)
        {
            DialogNodeGraph nodeGraph = EditorUtility.InstanceIDToObject(instanceID) as DialogNodeGraph;

            if (currentNodeGraph != null)
            {
                SetUpNodes();
            }

            if (nodeGraph != null)
            {
                OpenWindow();

                currentNodeGraph = nodeGraph;

                SetUpNodes();

                return true;
            }

            return false;
        }

        public static void SetCurrentNodeGraph(DialogNodeGraph nodeGraph)
        {
            currentNodeGraph = nodeGraph;
        }
        /// Otw�rz okno edytora w�z��w
        [MenuItem("Dialog Node Based Editor", menuItem = "Window/Dialog Node Based Editor")]
        public static void OpenWindow()
        {
            NodeEditor window = (NodeEditor)GetWindow(typeof(NodeEditor));
            window.titleContent = new GUIContent("Dialog Graph Editor");
            window.Show();

        }
        /// Renderowanie i obs�uga zdarze� interfejsu graficznego (GUI)
        private void OnGUI()
        {
            if (currentNodeGraph != null)
            {
                DrawDraggedLine();
                DrawNodeConnection();
                DrawGridBackground(gridSmallLineSpacing, 0.2f, Color.gray);
                DrawGridBackground(gridLargeLineSpacing, 0.2f, Color.gray);
                ProcessEvents(Event.current);
                DrawNodes();
            }

            if (GUI.changed)
                Repaint();
        }
        /// Konfigurowanie w�z��w podczas otwierania edytora
        private static void SetUpNodes()
        {
            foreach (Node node in currentNodeGraph.nodesList)
            {
                if (node.GetType() == typeof(AnswerNode))
                {
                    AnswerNode answerNode = (AnswerNode)node;
                    answerNode.CalculateAmountOfAnswers();
                    answerNode.CalculateAnswerNodeHeight();
                }
                if (node.GetType() == typeof(SentenceNode))
                {
                    SentenceNode sentenceNode = (SentenceNode)node;
                    sentenceNode.CheckNodeSize(nodeWidth, nodeHeight);
                }
            }
        }
        /// Rysowanie linii po��czenia podczas przeci�gania jej
        private void DrawDraggedLine()
        {
            if (currentNodeGraph.linePosition != Vector2.zero)
            {
                Handles.DrawBezier(currentNodeGraph.nodeToDrawLineFrom.rect.center, currentNodeGraph.linePosition,
                   currentNodeGraph.nodeToDrawLineFrom.rect.center, currentNodeGraph.linePosition,
                   Color.white, null, connectingLineWidth);
            }
        }
        /// Rysowanie po��cze� mi�dzy w�z�ami w oknie edytora
        private void DrawNodeConnection()
        {
            if (currentNodeGraph.nodesList == null)
            {
                return;
            }

            foreach (Node node in currentNodeGraph.nodesList)
            {
                Node parentNode = null;
                Node childNode = null;

                if (node.GetType() == typeof(AnswerNode))
                {
                    AnswerNode answerNode = (AnswerNode)node;

                    for (int i = 0; i < answerNode.childSentenceNodes.Count; i++)
                    {
                        if (answerNode.childSentenceNodes[i] != null)
                        {
                            parentNode = node;
                            childNode = answerNode.childSentenceNodes[i];

                            DrawConnectionLine(parentNode, childNode);
                        }
                    }
                }
                else if (node.GetType() == typeof(SentenceNode))
                {
                    SentenceNode sentenceNode = (SentenceNode)node;

                    if (sentenceNode.childNode != null)
                    {
                        parentNode = node;
                        childNode = sentenceNode.childNode;

                        DrawConnectionLine(parentNode, childNode);
                    }
                }
            }
        }
        /// Rysowanie linii po��czenia od w�z�a nadrz�dnego do w�z�a podrz�dnego
        private void DrawConnectionLine(Node parentNode, Node childNode)
        {
            Vector2 startPosition = parentNode.rect.center;
            Vector2 endPosition = childNode.rect.center;

            Vector2 midPosition = (startPosition + endPosition) / 2;
            Vector2 direction = endPosition - startPosition;

            Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
            Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;

            Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;

            Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1,
                Color.white, null, connectingLineWidth);
            Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2,
                Color.white, null, connectingLineWidth);

            Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition,
                Color.white, null, connectingLineWidth);

            GUI.changed = true;
        }
        /// Rysowanie linii t�a siatki w oknie edytora w�z��w
        private void DrawGridBackground(float gridSize, float gridOpacity, Color color)
        {
            int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
            int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

            Handles.color = new Color(color.r, color.g, color.b, gridOpacity);

            graphOffset += graphDrag * 0.5f;

            Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0);

            for (int i = 0; i < verticalLineCount; i++)
            {
                Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + gridOffset, new Vector3(gridSize * i, position.height + gridSize, 0f) + gridOffset);
            }

            for (int j = 0; j < horizontalLineCount; j++)
            {
                Handles.DrawLine(new Vector3(-gridSize, gridSize * j, 0) + gridOffset, new Vector3(position.width + gridSize, gridSize * j, 0f) + gridOffset);
            }

            Handles.color = Color.white;
        }
        /// Wywo�anie metody Draw ze wszystkich istniej�cych w�z��w na li�cie w�z��w
        private void DrawNodes()
        {
            if (currentNodeGraph.nodesList == null)
            {
                return;
            }

            foreach (Node node in currentNodeGraph.nodesList)
            {
                if (!node.isSelected)
                {
                    node.Draw(nodeStyle, lableStyle);
                }
                else
                {
                    node.Draw(selectedNodeStyle, lableStyle);
                }
            }

            GUI.changed = true;
        }
        /// Przetwarzanie zdarze� 
        private void ProcessEvents(Event currentEvent)
        {
            graphDrag = Vector2.zero;

            if (currentNode == null || currentNode.isDragging == false)
            {
                currentNode = GetHighlightedNode(currentEvent.mousePosition);
            }

            if (currentNode == null || currentNodeGraph.nodeToDrawLineFrom != null)
            {
                ProcessNodeEditorEvents(currentEvent);
            }
            else
            {
                currentNode.ProcessNodeEvents(currentEvent);
            }
        }
        /// Przetwarzanie wszystkich zdarze�
        private void ProcessNodeEditorEvents(Event currentEvent)
        {
            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    ProcessMouseDownEvent(currentEvent);
                    break;

                case EventType.MouseUp:
                    ProcessMouseUpEvent(currentEvent);
                    break;

                case EventType.MouseDrag:
                    ProcessMouseDragEvent(currentEvent);
                    break;

                case EventType.Repaint:
                    SelectNodesBySelectionRect(currentEvent.mousePosition);
                    break;

                default:
                    break;
            }
        }
        /// Przetwarzanie zdarzenia naci�ni�cia przycisku myszy
        private void ProcessMouseDownEvent(Event currentEvent)
        {
            if (currentEvent.button == 1)
            {
                ProcessRightMouseDownEvent(currentEvent);
            }
            else if (currentEvent.button == 0 && currentNodeGraph.nodesList != null)
            {
                ProcessLeftMouseDownEvent(currentEvent);
            }
            else if (currentEvent.button == 2)
            {
                ProcessScrollWheelDownEvent(currentEvent);
            }
        }
        /// Przetwarzanie zdarzenia naci�ni�cia prawego przycisku myszy
        private void ProcessRightMouseDownEvent(Event currentEvent)
        {
            if (GetHighlightedNode(currentEvent.mousePosition) == null)
            {
                ShowContextMenu(currentEvent.mousePosition);
            }
        }
        /// Przetwarzanie zdarzenia naci�ni�cia lewego przycisku myszy
        private void ProcessLeftMouseDownEvent(Event currentEvent)
        {
            ProcessNodeSelection(currentEvent.mousePosition);
        }
        /// Przetwarzanie zdarzenia naci�ni�cia ko�a myszy
        private void ProcessScrollWheelDownEvent(Event currentEvent)
        {
            mouseScrollClickPosition = currentEvent.mousePosition;
            isScrollWheelDragging = true;
        }

        /// Przetwarzanie zdarzenia zwolnienia przycisku myszy
        private void ProcessMouseUpEvent(Event currentEvent)
        {
            if (currentEvent.button == 1)
            {
                ProcessRightMouseUpEvent(currentEvent);
            }
            else if (currentEvent.button == 2)
            {
                ProcessScrollWheelUpEvent(currentEvent);
            }
        }
        /// Przetwarzanie zdarzenia zwolnienia prawego przycisku myszy
        private void ProcessRightMouseUpEvent(Event currentEvent)
        {
            if (currentNodeGraph.nodeToDrawLineFrom != null)
            {
                CheckLineConnection(currentEvent);
                ClearDraggedLine();
            }
        }
        /// Przetwarzanie zdarzenia przewijania ko�a myszy w g�r�
        private void ProcessScrollWheelUpEvent(Event currentEvent)
        {
            selectionRect = new Rect(0, 0, 0, 0);
            isScrollWheelDragging = false;
        }
        /// Przetwarzanie zdarzenia przeci�gania mysz�
        private void ProcessMouseDragEvent(Event currentEvent)
        {
            if (currentEvent.button == 0)
            {
                ProcessLeftMouseDragEvent(currentEvent);
            }
            else if (currentEvent.button == 1)
            {
                ProcessRightMouseDragEvent(currentEvent);
            }
        }
        /// Przetwarzanie zdarzenia przeci�gania lewym przyciskiem myszy
        private void ProcessLeftMouseDragEvent(Event currentEvent)
        {
            SelectNodesBySelectionRect(currentEvent.mousePosition);

            graphDrag = currentEvent.delta;

            foreach (var node in currentNodeGraph.nodesList)
            {
                node.DragNode(graphDrag);
            }

            GUI.changed = true;
        }
        /// Przetwarzanie zdarzenia przeci�gania prawym przyciskiem myszy
        private void ProcessRightMouseDragEvent(Event currentEvent)
        {
            if (currentNodeGraph.nodeToDrawLineFrom != null)
            {
                DragConnectiongLine(currentEvent.delta);
                GUI.changed = true;
            }
        }
        /// Przeci�gnij lini� ��cz�c� z w�z�a
        private void DragConnectiongLine(Vector2 delta)
        {
            currentNodeGraph.linePosition += delta;
        }

        /// Sprawd� po��czenie linii po zwolnieniu prawego przycisku myszy
        private void CheckLineConnection(Event currentEvent)
        {
            if (currentNodeGraph.nodeToDrawLineFrom != null)
            {
                Node node = GetHighlightedNode(currentEvent.mousePosition);

                if (node != null)
                {
                    currentNodeGraph.nodeToDrawLineFrom.AddToChildConnectedNode(node);
                    node.AddToParentConnectedNode(currentNodeGraph.nodeToDrawLineFrom);
                }
            }
        }
        /// Wyczy�� przeci�gni�t� lini�
        private void ClearDraggedLine()
        {
            currentNodeGraph.nodeToDrawLineFrom = null;
            currentNodeGraph.linePosition = Vector2.zero;
            GUI.changed = true;
        }
        /// Przetwarzaj wyb�r w�z�a, dodaj do listy wybranych w�z��w, je�li w�ze� jest zaznaczony
        private void ProcessNodeSelection(Vector2 mouseClickPosition)
        {
            Node clickedNode = GetHighlightedNode(mouseClickPosition);

            //Anuluj zaznaczenie wszystkich w�z��w po klikni�ciu poza w�z�em
            if (clickedNode == null)
            {
                foreach (Node node in currentNodeGraph.nodesList)
                {
                    if (node.isSelected)
                    {
                        node.isSelected = false;
                    }
                }

                return;
            }
        }
        /// Narysuj prostok�t zaznaczenia i zaznacz wszystkie w�z�y wewn�trz niego

        private void SelectNodesBySelectionRect(Vector2 mousePosition)
        {
            if (!isScrollWheelDragging)
            {
                return;
            }

            selectionRect = new Rect(mouseScrollClickPosition.x, mouseScrollClickPosition.y,
                mousePosition.x - mouseScrollClickPosition.x, mousePosition.y - mouseScrollClickPosition.y);

            EditorGUI.DrawRect(selectionRect, new Color(0, 0, 0, 0.5f));


            foreach (Node node in currentNodeGraph.nodesList)
            {
                if (selectionRect.Contains(node.rect.position))
                {
                    node.isSelected = true;
                }
            }
        }
        /// Zwr�� w�ze�, kt�ry znajduje si� w pozycji myszy
        private Node GetHighlightedNode(Vector2 mousePosition)
        {
            if (currentNodeGraph.nodesList.Count == 0)
            {
                return null;
            }

            foreach (Node node in currentNodeGraph.nodesList)
            {
                if (node.rect.Contains(mousePosition))
                {
                    return node;
                }
            }

            return null;
        }
        /// Poka� menu kontekstowe
        private void ShowContextMenu(Vector2 mousePosition)
        {
            GenericMenu contextMenu = new GenericMenu();

            contextMenu.AddItem(new GUIContent("Create Sentence Node"), false, CreateSentenceNode, mousePosition);
            contextMenu.AddItem(new GUIContent("Create Answer Node"), false, CreateAnswerNode, mousePosition);
            contextMenu.AddSeparator("");
            contextMenu.AddItem(new GUIContent("Select All Nodes"), false, SelectAllNodes, mousePosition);
            contextMenu.AddItem(new GUIContent("Remove Selected Nodes"), false, RemoveSelectedNodes, mousePosition);
            contextMenu.ShowAsContext();
        }
        /// Utw�rz w�ze� zdania w pozycji myszy i dodaj go do zasobu grafu w�z��w
        private void CreateSentenceNode(object mousePositionObject)
        {
            SentenceNode sentenceNode = ScriptableObject.CreateInstance<SentenceNode>();
            InitialiseNode(mousePositionObject, sentenceNode, "Sentence Node");
        }
        /// Utw�rz w�ze� odpowiedzi w pozycji myszy i dodaj go do zasobu grafu w�z��w
        private void CreateAnswerNode(object mousePositionObject)
        {
            AnswerNode answerNode = ScriptableObject.CreateInstance<AnswerNode>();
            InitialiseNode(mousePositionObject, answerNode, "Answer Node");
        }
        /// Zaznacz wszystkie w�z�y w edytorze w�z��w
        private void SelectAllNodes(object userData)
        {
            foreach (Node node in currentNodeGraph.nodesList)
            {
                node.isSelected = true;
            }

            GUI.changed = true;
        }
        /// Usu� wszystkie zaznaczone w�z�y
        private void RemoveSelectedNodes(object userData)
        {
            Queue<Node> nodeDeletionQueue = new Queue<Node>();

            foreach (Node node in currentNodeGraph.nodesList)
            {
                if (node.isSelected)
                {
                    nodeDeletionQueue.Enqueue(node);
                }
            }

            while (nodeDeletionQueue.Count > 0)
            {
                Node nodeTodelete = nodeDeletionQueue.Dequeue();

                currentNodeGraph.nodesList.Remove(nodeTodelete);

                DestroyImmediate(nodeTodelete, true);
                AssetDatabase.SaveAssets();
            }
        }
        /// Utw�rz w�ze� w pozycji myszy i dodaj go do zasobu grafu w�z��w
        private void InitialiseNode(object mousePositionObject, Node node, string nodeName)
        {
            Vector2 mousePosition = (Vector2)mousePositionObject;

            currentNodeGraph.nodesList.Add(node);

            node.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), nodeName, currentNodeGraph);

            AssetDatabase.AddObjectToAsset(node, currentNodeGraph);
            AssetDatabase.SaveAssets();
        }
        /// Zmie� bie��cy graf w�z��w i narysuj nowy.
        private void ChangeEditorWindowOnSelection()
        {
            DialogNodeGraph nodeGraph = Selection.activeObject as DialogNodeGraph;

            if (nodeGraph != null)
            {
                currentNodeGraph = nodeGraph;
                GUI.changed = true;
            }
        }
    }
}