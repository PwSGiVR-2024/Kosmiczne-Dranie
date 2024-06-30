using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace dsystem
{
    public class Node : ScriptableObject
    {
        [HideInInspector] public List<Node> connectedNodesList;
        [HideInInspector] public DialogNodeGraph nodeGraph;
        [HideInInspector] public Rect rect = new Rect();

        [HideInInspector] public bool isDragging;
        [HideInInspector] public bool isSelected;

        protected float standartHeight;

        /// Metoda inicjalizacji bazowej
        public virtual void Initialise(Rect rect, string nodeName, DialogNodeGraph nodeGraph)
        {
            name = nodeName;
            standartHeight = rect.height;
            this.rect = rect;
            this.nodeGraph = nodeGraph;
        }
        /// Metoda rysowania podstawowa
        public virtual void Draw(GUIStyle nodeStyle, GUIStyle lableStyle)
        { }

        public virtual bool AddToParentConnectedNode(Node nodeToAdd)
        { return true; }

        public virtual bool AddToChildConnectedNode(Node nodeToAdd)
        { return true; }
#if UNITY_EDITOR
        /// Przetwarzanie zdarzenia myszy w wêŸle
        public void ProcessNodeEvents(Event currentEvent)
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

                default:
                    break;
            }
        }
        /// Przetwarzanie zdarzenia naciœniêcia myszy na wêŸle
        private void ProcessMouseDownEvent(Event currentEvent)
        {
            if (currentEvent.button == 0)
            {
                ProcessLeftMouseDownEvent(currentEvent);
            }
            else if (currentEvent.button == 1)
            {
                ProcessRightMouseDownEvent(currentEvent);
            }
        }
        /// Przetwarzanie zdarzenia klikniêcia lewym przyciskiem myszy na wêŸle
        private void ProcessLeftMouseDownEvent(Event currentEvent)
        {
            OnNodeLeftClick();
        }
        /// Przetwarzanie zdarzenia naciœniêcia prawego przycisku myszy na wêŸle
        private void ProcessRightMouseDownEvent(Event currentEvent)
        {
            nodeGraph.SetNodeToDrawLineFromAndLinePosition(this, currentEvent.mousePosition);
        }
        /// Przetwarzanie zdarzenia puszczenia przycisku myszy na wêŸle
        private void ProcessMouseUpEvent(Event currentEvent)
        {
            if (currentEvent.button == 0)
            {
                ProcessLeftMouseUpEvent(currentEvent);
            }
        }
        /// Przetwarzanie zdarzenia puszczenia lewego przycisku myszy na wêŸle
        private void ProcessLeftMouseUpEvent(Event currentEvent)
        {
            isDragging = false;
        }
        /// Przetwarzanie zdarzenia przeci¹gniêcia myszy na wêŸle
        private void ProcessMouseDragEvent(Event currentEvent)
        {
            if (currentEvent.button == 0)
            {
                ProcessLeftMouseDragEvent(currentEvent);
            }
        }
        /// Przetwarzanie zdarzenia przeci¹gniêcia myszy lewym przyciskiem na wêŸle
        private void ProcessLeftMouseDragEvent(Event currentEvent)
        {
            isDragging = true;
            DragNode(currentEvent.delta);
            GUI.changed = true;
        }
        /// Zaznaczanie i odznaczanie wêz³a
        public void OnNodeLeftClick()
        {
            Selection.activeObject = this;

            if (isSelected)
            {
                isSelected = false;
            }
            else
            {
                isSelected = true;
            }
        }
        /// Przeci¹ganie wêz³a
        public void DragNode(Vector2 delta)
        {
            rect.position += delta;
            EditorUtility.SetDirty(this);
        }
#endif
    }
}