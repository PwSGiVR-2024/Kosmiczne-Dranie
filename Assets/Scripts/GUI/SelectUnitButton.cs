using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectUnitButton : MonoBehaviour
{
    // assign in inspector
    public ShipClass shipClass;

    public GameObject unitPrefab;
    public AiController prefabController;
    public bool isOutpost = false;


    public Image image;

    public Button mainButton;
    public bool selected = false;
    public int selectedCount = 0;

    public Button selectX1;
    public Button selectX10;
    public Button selectX25;
    public Button selectX100;

    private Color normalMainColor;
    public Color selectedMainColor;

    public UnityEvent onSelect = new();
    public UnityEvent onDeselect = new();
    public UnityEvent onCountChange = new();

    public GameObject hoverInfo;
    public TMP_Text metalPrice;
    public TMP_Text crystalPrice;
    public TMP_Text maintenance;
    public TMP_Text size;
    public TMP_Text power;
    public TMP_Text speed;
    public TMP_Text range;
    public TMP_Text health;

    private void Start()
    {
        if (unitPrefab.TryGetComponent(out Outpost outpost))
        {
            isOutpost = true;

            metalPrice.text = "Metals: " + outpost.values.metalPrice;
            crystalPrice.text = "Crystals: " + outpost.values.crystalPrice;
            maintenance.text = "Maintenance: " + outpost.values.maintenancePrice;
            size.text = "Size: " + 1000;
            power.text = "Power: " + 1200;
            speed.text = "Speed: " + 0;
            range.text = "Spot: " + outpost.values.range;
            health.text = "Health: " + outpost.values.health;
        }

        else if (unitPrefab.TryGetComponent(out AiController unit))
        {
            prefabController = unit;
            isOutpost = false;

            metalPrice.text = "Metals: " + unit.Values.metalPrice;
            crystalPrice.text = "Crystals: " + unit.Values.crystalPrice;
            maintenance.text = "Maintenance: " + unit.Values.maintenancePrice;
            size.text = "Size: " + unit.Values.size;
            power.text = "Power: " + unit.Values.power;
            speed.text = "Speed: " + unit.Values.unitSpeed;
            range.text = "Spot: " + unit.Values.spotDistance;
            health.text = "Health: " + unit.Values.health;
        }

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
        onDeselect.Invoke();
    }

    private void SelectCountButton(Button button, int count)
    {
        selectedCount = count;

        DeselectCountButton(selectX1);
        DeselectCountButton(selectX10);
        DeselectCountButton(selectX25);
        DeselectCountButton(selectX100);

        button.image.color = button.colors.selectedColor;

        onCountChange.Invoke();
    }

    private void DeselectCountButton(Button button)
    {
        button.image.color = button.colors.normalColor;
    }

    public void OnPointerEnter(BaseEventData data)
    {
        hoverInfo.SetActive(true);
    }

    public void OnPointerExit(BaseEventData data)
    {
        hoverInfo.SetActive(false);
    }
}
