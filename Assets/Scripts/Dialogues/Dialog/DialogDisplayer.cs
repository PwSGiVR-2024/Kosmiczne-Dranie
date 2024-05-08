using UnityEngine;

namespace dsystem
{
    public class DialogDisplayer : MonoBehaviour
    {
        [Header("MAIN COMPONENT")]
        [SerializeField] private DialogBehaviour dialogBehaviour;

        [Header("NODE PANELS")]
        [SerializeField] private SentencePanel dialogSentensePanel;
        [SerializeField] private AnswerPanel dialogAnswerPanel;

        private void OnEnable()
        {
            dialogBehaviour.AddListenerToDialogFinishedEvent(DisableDialogPanel);

            dialogBehaviour.OnAnswerButtonSetUp += SetUpAnswerButtonsClickEvent;

            dialogBehaviour.OnDialogTextCharWrote += dialogSentensePanel.IncreaseMaxVisibleCharacters;
            dialogBehaviour.OnDialogTextSkipped += dialogSentensePanel.ShowFullDialogText;

            dialogBehaviour.OnSentenceNodeActive += EnableDialogSentencePanel;
            dialogBehaviour.OnSentenceNodeActive += DisableDialogAnswerPanel;
            dialogBehaviour.OnSentenceNodeActive += dialogSentensePanel.ResetDialogText;
            dialogBehaviour.OnSentenceNodeActiveWithParameter += dialogSentensePanel.Setup;

            dialogBehaviour.OnAnswerNodeActive += EnableDialogAnswerPanel;
            dialogBehaviour.OnAnswerNodeActive += DisableDialogSentencePanel;

            dialogBehaviour.OnAnswerNodeActiveWithParameter += dialogAnswerPanel.EnableCertainAmountOfButtons;
            dialogBehaviour.OnMaxAmountOfAnswerButtonsCalculated += dialogAnswerPanel.SetUpButtons;

            dialogBehaviour.OnAnswerNodeSetUp += SetUpAnswerDialogPanel;
        }

        private void OnDisable()
        {
            dialogBehaviour.OnAnswerButtonSetUp -= SetUpAnswerButtonsClickEvent;

            dialogBehaviour.OnDialogTextCharWrote -= dialogSentensePanel.IncreaseMaxVisibleCharacters;
            dialogBehaviour.OnDialogTextSkipped -= dialogSentensePanel.ShowFullDialogText;

            dialogBehaviour.OnSentenceNodeActive -= EnableDialogSentencePanel;
            dialogBehaviour.OnSentenceNodeActive -= DisableDialogAnswerPanel;
            dialogBehaviour.OnSentenceNodeActive += dialogSentensePanel.ResetDialogText;
            dialogBehaviour.OnSentenceNodeActiveWithParameter -= dialogSentensePanel.Setup;

            dialogBehaviour.OnAnswerNodeActive -= EnableDialogAnswerPanel;
            dialogBehaviour.OnAnswerNodeActive -= DisableDialogSentencePanel;

            dialogBehaviour.OnAnswerNodeActiveWithParameter -= dialogAnswerPanel.EnableCertainAmountOfButtons;
            dialogBehaviour.OnMaxAmountOfAnswerButtonsCalculated -= dialogAnswerPanel.SetUpButtons;

            dialogBehaviour.OnAnswerNodeSetUp -= SetUpAnswerDialogPanel;
        }
        /// Wyłączenie panelu odpowiedzi i panelu zdan dialogowych
        public void DisableDialogPanel()
        {
            DisableDialogAnswerPanel();
            DisableDialogSentencePanel();
        }

        /// Włączenie panelu odpowiedzi dialogowych
        public void EnableDialogAnswerPanel()
        {
            AktywujGameObject(dialogAnswerPanel.gameObject, true);
            dialogAnswerPanel.DisalbleAllButtons();
        }

        /// Wyłączenie panelu odpowiedzi dialogowych
        public void DisableDialogAnswerPanel()
        {
            AktywujGameObject(dialogAnswerPanel.gameObject, false);
        }

        /// Włączenie panelu zdan dialogowych
        public void EnableDialogSentencePanel()
        {
            dialogSentensePanel.ResetDialogText();

            AktywujGameObject(dialogSentensePanel.gameObject, true);
        }

        /// Wyłączenie panelu zdan dialogowych
        public void DisableDialogSentencePanel()
        {
            AktywujGameObject(dialogSentensePanel.gameObject, false);
        }

        /// Włączanie lub wyłączanie obiektu w zależności od flagi bool isActive
        public void AktywujGameObject(GameObject gameObject, bool isActive)
        {
            if (gameObject == null)
            {
                Debug.LogWarning("Obiekt gry jest pusty");
                return;
            }

            gameObject.SetActive(isActive);
        }

        /// Konfiguracja zdarzenia onClick przycisku odpowiedzi
        public void SetUpAnswerButtonsClickEvent(int index, AnswerNode answerNode)
        {
            dialogAnswerPanel.GetButtonByIndex(index).onClick.AddListener(() =>
            {
                dialogBehaviour.SetCurrentNodeAndHandleDialogGraph(answerNode.childSentenceNodes[index]);
            });
        }

        /// Konfiguracja panelu dialogowego odpowiedzi
        public void SetUpAnswerDialogPanel(int index, string answerText)
        {
            dialogAnswerPanel.GetButtonTextByIndex(index).text = answerText;
        }
    }
}