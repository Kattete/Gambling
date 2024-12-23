using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlinkoStatsManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject statsPanel;
    public Button toggleStatsButton;
    public RectTransform graphContainer;
    public GameObject graphPointPrefab;
    public GameObject graphLinePrefab;

    [Header("Stats Text Fields")]
    public TMP_Text totalProfitText;
    public TMP_Text winRateText;
    public TMP_Text avgMultiplierText;
    public TMP_Text gamesPlayedText;
    public TMP_Text bestWinText;
    public TMP_Text worstLossText;

    [Header("Graph Settings")]
    public int maxDataPoints = 50;
    public float graphWidth = 800f;
    public float graphHeight = 400f;
    public Color profitColor = Color.green;
    public Color lossColor = Color.red;
    public Color lineColor = Color.white;

    private List<float> profitLossHistory = new List<float>();
    private List<GameObject> graphPoints = new List<GameObject>();
    private List<GameObject> graphLines = new List<GameObject>();
    private float totalProfit = 0f;
    private int totalBets = 0;
    private int wins = 0;
    private float totalMultipliers = 0f;
    private float bestWin = 0f;
    private float worstWin = 0f;
    private int totalGamesPlayed = 0;

    private void Start()
    {
        // Initialize the panel as hidden
        statsPanel.SetActive(false);

        // Set up button listener
        toggleStatsButton.onClick.AddListener(ToggleStatsPanel);

        // subscribe to betting events
        PlinkoBettingManager.Instance.onBetResolved += OnBetResolved;
    }

    private void OnBetResolved(float betAmount, float multiplier, float winAmount)
    {
        float profitLoss = winAmount - betAmount;
        UpdateStats(betAmount, multiplier, winAmount, profitLoss);
        UpdateGraph();
    }

    private void UpdateStats(float betAmount, float multiplier, float winAmount, float profitLoss)
    {
        totalBets++;
        totalGamesPlayed++;
        totalProfit += profitLoss;
        totalMultipliers += multiplier;

        if(winAmount > betAmount)
        {
            wins++;
            bestWin = Mathf.Max(bestWin, profitLoss);
        }
        else
        { // worstWin = worstLoss
            worstWin = Mathf.Min(worstWin, profitLoss);
        }

        // Add to history
        profitLossHistory.Add(totalProfit);
        if(profitLossHistory.Count > maxDataPoints)
        {
            profitLossHistory.RemoveAt(0);
        }

        // Updaate UI Texts
        UpdateStatsText();
    }

    private void UpdateStatsText()
    {
        float winRate = totalBets > 0 ? (float)wins / totalBets * 100 : 0;
        float avgMultiplier = totalBets > 0 ? totalMultipliers / totalBets : 0;

        totalProfitText.text = $"Total Profit: {totalProfit}$";
        winRateText.text = $"Win Rate: {winRate}%";
        avgMultiplierText.text = $"Avg Multiplier: {avgMultiplier}x";
        bestWinText.text = $"Best Win: {bestWin}$";
        worstLossText.text = $"Worst Loss: {worstWin}$";
        gamesPlayedText.text = $"Games Played: {totalGamesPlayed}";
    }

    private void UpdateGraph()
    {
        // Clear existing Graph
        ClearGraph();

        if (profitLossHistory.Count < 2) return;

        float minValue = profitLossHistory.Min();
        float maxValue = profitLossHistory.Max();
        float valueRange = Mathf.Max(Mathf.Abs(maxValue - minValue), 1f);

        // Create points and lines
        for (int i = 0; i < profitLossHistory.Count; i++)
        {
            // Calculate position
            float xPosition = (i / (float)(profitLossHistory.Count - 1)) * graphWidth;
            float yPosition = ((profitLossHistory[i] - minValue) / valueRange) * graphHeight;

            // Create point
            GameObject point = Instantiate(graphPointPrefab, graphContainer);
            RectTransform pointRect = point.GetComponent<RectTransform>();
            pointRect.anchoredPosition = new Vector2(xPosition, yPosition);

            // Set point color based on profit/loss
            Image pointImage = point.GetComponent<Image>();
            pointImage.color = profitLossHistory[i] >= 0 ? profitColor : lossColor;

            graphPoints.Add(point);
            if (i < profitLossHistory.Count - 1)
            {
                float nextX = ((i + 1) / (float)(profitLossHistory.Count - 1)) * graphWidth;
                float nextY = ((profitLossHistory[i + 1] - minValue) / valueRange) * graphHeight;

                GameObject line = Instantiate(graphLinePrefab, graphContainer);
                RectTransform lineRect = line.GetComponent<RectTransform>();

                // Position and rotate line to connect points
                float distance = Vector2.Distance(
                    new Vector2(xPosition, yPosition),
                    new Vector2(nextX, nextY)
                );
                float angle = Mathf.Atan2(nextY - yPosition, nextX - xPosition) * Mathf.Rad2Deg;

                lineRect.anchoredPosition = new Vector2(xPosition, yPosition);
                lineRect.sizeDelta = new Vector2(distance, 2f);
                lineRect.rotation = Quaternion.Euler(0, 0, angle);

                line.GetComponent<Image>().color = lineColor;
                graphLines.Add(line);
            }
        }
    }

    private void ClearGraph()
    {
        foreach (GameObject point in graphPoints)
        {
            Destroy(point);
        }
        foreach (GameObject line in graphLines)
        {
            Destroy(line);
        }
        graphPoints.Clear();
        graphLines.Clear();
    }

    private void ToggleStatsPanel()
    {
        statsPanel.SetActive(!statsPanel.activeSelf);
        if (statsPanel.activeSelf)
        {
            UpdateGraph();
        }
    }

    private void OnDestroy()
    {
        if (PlinkoBettingManager.Instance != null)
        {
            PlinkoBettingManager.Instance.onBetResolved -= OnBetResolved;
        }
    }
}
