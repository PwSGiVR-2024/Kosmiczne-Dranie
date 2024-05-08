using System.Collections.Generic;
using UnityEngine;

namespace dsystem
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Nodes/Node Graph", fileName = "New Node Graph")]
    public class DialogNodeGraph : ScriptableObject
    {
        public List<Node> nodesList = new List<Node>();

        [HideInInspector] public Node nodeToDrawLineFrom = null;
        [HideInInspector] public Vector2 linePosition = Vector2.zero;
        /// Przypisanie wartoœci do pól nodeToDrawLineFrom i linePosition
        public void SetNodeToDrawLineFromAndLinePosition(Node nodeToDrawLineFrom, Vector2 linePosition)
        {
            this.nodeToDrawLineFrom = nodeToDrawLineFrom;
            this.linePosition = linePosition;
        }
        /// Przeci¹ganie wszystkich zaznaczonych wêz³ów
        public void DragAllSelectedNodes(Vector2 delta)
        {
            foreach (var node in nodesList)
            {
                if (node.isSelected)
                {
                    node.DragNode(delta);
                }
            }
        }
        /// Zwracanie liczby zaznaczonych wêz³ów
        public int GetAmountOfSelectedNodes()
        {
            int amount = 0;

            foreach (Node node in nodesList)
            {
                if (node.isSelected)
                {
                    amount++;
                }
            }

            return amount;
        }
    }
}