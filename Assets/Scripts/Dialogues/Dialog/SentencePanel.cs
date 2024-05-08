using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace dsystem
{
    public class SentencePanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI dialogNameText;
        [SerializeField] private TextMeshProUGUI dialogText;
        [SerializeField] private Image dialogCharacterImage;
        /// Ustawienie maksymalnej liczby widocznych znaków dialogu na zero
        public void ResetDialogText()
        {
            dialogText.maxVisibleCharacters = 0;
        }

        /// Ustawienie maksymalnej liczby widocznych znaków dialogu na d³ugoœæ tekstu dialogowego
        public void ShowFullDialogText(string text)
        {
            dialogText.maxVisibleCharacters = text.Length;
        }

        /// Przypisanie tekstu dialogowego, nazwy postaci i obrazu postaci do panelu dialogowego
        public void Setup(string name, string text, Sprite sprite)
        {
            dialogNameText.text = name;
            dialogText.text = text;

            if (sprite == null)
            {
                dialogCharacterImage.color = new Color(dialogCharacterImage.color.r,
                    dialogCharacterImage.color.g, dialogCharacterImage.color.b, 0);
                return;
            }

            dialogCharacterImage.color = new Color(dialogCharacterImage.color.r,
                    dialogCharacterImage.color.g, dialogCharacterImage.color.b, 255);
            dialogCharacterImage.sprite = sprite;
        }

        /// Zwiêkszenie maksymalnej liczby widocznych znaków dialogu
        public void IncreaseMaxVisibleCharacters()
        {
            dialogText.maxVisibleCharacters++;
        }
    }
}