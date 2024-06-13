using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectUnitButton : MonoBehaviour
{

    public GameObject unitPrefab;
    public bool isOutpost = false;


    public Image image;

    public Button mainButton;
    public bool selected = false;
    public int selectedCount = 1;

    public Button selectX1;
    public Button selectX10;
    public Button selectX25;
    public Button selectX100;

    private Color normalMainColor;
    public Color selectedMainColor;

    public UnityEvent onSelect = new();

    private void Start()
    {
        if (unitPrefab.TryGetComponent(out Outpost _))
            isOutpost = true;

        else isOutpost = false;

        normalMainColor = image.color;
        mainButton.onClick.AddListener(ToggleSelect);

        selectX1?.onClick.AddListener(() => SelectCountButton(selectX1, 1));
        selectX10?.onClick.AddListener(() => SelectCountButton(selectX10, 10));
        selectX25?.onClick.AddListener(() => SelectCountButton(selectX25, 25));
        selectX100?.onClick.AddListener(() => SelectCountButton(selectX100, 100));
    }

    private void ToggleSelect()
    {
        if (selected) DeselectMain();
        else SelectMain();
    }

    public void SelectMain()
    {
        selected = true;

        image.color = selectedMainColor;
        onSelect.Invoke();
    }

    public void DeselectMain()
    {
        selected = false;
        image.color = normalMainColor;
    }

    private void SelectCountButton(Button button, int count)
    {
        selectedCount = count;

        DeselectCountButton(selectX1);
        DeselectCountButton(selectX10);
        DeselectCountButton(selectX25);
        DeselectCountButton(selectX100);

        button.image.color = button.colors.selectedColor;
    }

    private void DeselectCountButton(Button button)
    {
        button.image.color = button.colors.normalColor;
    }
}
