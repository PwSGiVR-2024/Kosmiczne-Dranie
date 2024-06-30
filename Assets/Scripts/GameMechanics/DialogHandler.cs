using dsystem;
using UnityEngine;

public class DialogHandler : MonoBehaviour
{
    public bool disableDialogs = false;

    [SerializeField] private DialogBehaviour dialogBehaviour;
    [SerializeField] private DialogNodeGraph dialogGraph1;
    [SerializeField] private DialogNodeGraph dialogGraph2;
    [SerializeField] private DialogNodeGraph dialogGraph3;
    [SerializeField] private DialogNodeGraph dialogGraph4;
    [SerializeField] private DialogNodeGraph dialogGraph5;
    [SerializeField] private DialogNodeGraph dialogGraph6;
    [SerializeField] private DialogNodeGraph dialogGraphevil1;
    [SerializeField] private DialogNodeGraph dialogGraphevil2;
    [SerializeField] private DialogNodeGraph dialogGraphevil3;
    [SerializeField] private DialogNodeGraph dialogGraphevil4;
    [SerializeField] private DialogNodeGraph dialogGraphevil5;
    [SerializeField] private DialogNodeGraph dialogGraphevil6;
    [SerializeField] private DialogNodeGraph startDialog;
    [SerializeField] private DialogNodeGraph endDialog;
    [SerializeField] private DialogNodeGraph pizdaDialog;


    SiteController[] siteControllers;
    void Start()
    {
        siteControllers = FindObjectsOfType<SiteController>();
        for (int i = 0; i < siteControllers.Length; i++)
        {
            var controller = siteControllers[i];
            controller.onCaptured.AddListener(() => OnSiteCaptured(controller));
        }
    }

    void Update()
    {

    }
    void OnSiteCaptured(SiteController site)
    {
        if (disableDialogs) return;

        if (site.currentController == Affiliation.Blue)
        {
            switch (site.gameObject.name)
            {
                case "site1":
                    dialogBehaviour.StartDialog(dialogGraph1);
                    break;
                case "site2":
                    dialogBehaviour.StartDialog(dialogGraph2);
                    break;
                case "site3":
                    dialogBehaviour.StartDialog(dialogGraph3);
                    break;
                case "site4":
                    dialogBehaviour.StartDialog(dialogGraph4);
                    break;
                case "site5":
                    dialogBehaviour.StartDialog(dialogGraph5);
                    break;
                case "site6":
                    dialogBehaviour.StartDialog(dialogGraph6);
                    break;
            }
        }
        else if (site.currentController == Affiliation.Red && site.previousControler == Affiliation.Blue)
        {
            switch (site.gameObject.name)
            {
                case "site1":
                    dialogBehaviour.StartDialog(dialogGraphevil1);
                    break;
                case "site2":
                    dialogBehaviour.StartDialog(dialogGraphevil2);
                    break;
                case "site3":
                    dialogBehaviour.StartDialog(dialogGraphevil3);
                    break;
                case "site4":
                    dialogBehaviour.StartDialog(dialogGraphevil4);
                    break;
                case "site5":
                    dialogBehaviour.StartDialog(dialogGraphevil5);
                    break;
                case "site6":
                    dialogBehaviour.StartDialog(dialogGraphevil6);
                    break;
            }
        }
    }
}
