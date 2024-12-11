using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlinkoSliderControllerScript : MonoBehaviour
{
    [Header("Grid Reference")]
    public PlinkoBallGridGenerator generator;

    [Header("Slider Configuration")]
    public Slider rowCountSlider;
    public TMP_Text rowCountText;

    [Header("Slider Constraints")]
    [Tooltip("Minimum number of rows allowed")]
    public int minRows = 4;

    [Tooltip("Maximum number of rows allowed")]
    public int maxRows = 12;

    private void Start()
    {
        ConfigureRowCountSlider();
        UpdateGridRows();
    }

    void ConfigureRowCountSlider()
    {
        if (rowCountSlider == null)
        {
            Debug.LogError("Row Count Slider is not assigned!");
            return;
        }

        rowCountSlider.minValue = minRows;
        rowCountSlider.maxValue = maxRows;
        rowCountSlider.value = minRows;

        // Add listener for slider value changes
        rowCountSlider.onValueChanged.AddListener(delegate { UpdateGridRows(); });
    }

    public void UpdateGridRows()
    {
        int currentRowCount = Mathf.RoundToInt(rowCountSlider.value);

        if (rowCountText != null)
        {
            rowCountText.text = $"Rows: {currentRowCount}";
        }

        if (generator != null)
        {
            generator.ConfigureGrid(currentRowCount);
        }
        else Debug.LogError("Plinko Grid Generator is not assigned!");
    }
}