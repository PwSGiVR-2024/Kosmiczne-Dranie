using UnityEngine;
using dsystem;

public class TestDialogStarter : MonoBehaviour
{
    [SerializeField] private DialogBehaviour dialogBehaviour;
    [SerializeField] private DialogNodeGraph dialogGraph;

    private void Start()
    {
        if (dialogBehaviour && dialogGraph)
            dialogBehaviour.StartDialog(dialogGraph);
    }
}