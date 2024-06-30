using UnityEngine;

namespace dsystem
{
    public class DialogDisplayer : MonoBehaviour
    {
        [Header("MAIN COMPONENT")]
        [SerializeField] private DialogBehaviour dialogBehaviour;

        [Header("NODE PANELS")]
        [SerializeField] private SentencePanel dialogSentensePanel;

        private void OnEnable()
        {
            dialogBehaviour.AddListenerToDialogFinishedEvent(DisableDialogPanel);

            dialogBehaviour.OnDialogTextCharWrote += dialogSentensePanel.IncreaseMaxVisibleCharacters;
            dialogBehaviour.OnDialogTextSkipped += dialogSentensePanel.ShowFullDialogText;

            dialogBehaviour.OnSentenceNodeActive += EnableDialogSentencePanel;
            dialogBehaviour.OnSentenceNodeActive += dialogSentensePanel.ResetDialogText;
            dialogBehaviour.OnSentenceNodeActiveWithParameter += dialogSentensePanel.Setup;

            dialogBehaviour.OnAnswerNodeActive += DisableDialogSentencePanel;

        }

        private void OnDisable()
        {

            dialogBehaviour.OnDialogTextCharWrote -= dialogSentensePanel.IncreaseMaxVisibleCharacters;
            dialogBehaviour.OnDialogTextSkipped -= dialogSentensePanel.ShowFullDialogText;

            dialogBehaviour.OnSentenceNodeActive -= EnableDialogSentencePanel;
            dialogBehaviour.OnSentenceNodeActive += dialogSentensePanel.ResetDialogText;
            dialogBehaviour.OnSentenceNodeActiveWithParameter -= dialogSentensePanel.Setup;

            dialogBehaviour.OnAnswerNodeActive -= DisableDialogSentencePanel;

        }
        /// Wy³¹czenie panelu odpowiedzi i panelu zdan dialogowych
        public void DisableDialogPanel()
        {
            DisableDialogSentencePanel();
        }



        /// W³¹czenie panelu zdan dialogowych
        public void EnableDialogSentencePanel()
        {
            dialogSentensePanel.ResetDialogText();

            AktywujGameObject(dialogSentensePanel.gameObject, true);
        }

        /// Wy³¹czenie panelu zdan dialogowych
        public void DisableDialogSentencePanel()
        {
            AktywujGameObject(dialogSentensePanel.gameObject, false);
        }

        /// W³¹czanie lub wy³¹czanie obiektu w zale¿noœci od flagi bool isActive
        public void AktywujGameObject(GameObject gameObject, bool isActive)
        {
            if (gameObject == null)
            {
                Debug.LogWarning("Obiekt gry jest pusty");
                return;
            }

            gameObject.SetActive(isActive);
        }

    }
}