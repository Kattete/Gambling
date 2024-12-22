using System;
using TMPro;
using UnityEngine;

public class SessionTimerScript : MonoBehaviour
{
    public static SessionTimerScript Instance { get; private set; }
    public TMP_Text timerText;
    private float sessionTime = 0f;
    private bool isRunning = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.timerText = this.timerText;
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnLevelWasLoaded(int level)
    {
        if (timerText == null)
        {
            GameObject timerTextObjc = GameObject.Find("TimerText");
            if (timerTextObjc != null)
            {
                timerText = timerTextObjc.GetComponent<TMP_Text>();
            }
        }

    }

    private void Start()
    {
        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (isRunning)
        {
            sessionTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    private void UpdateTimerDisplay()
    {
        if(timerText != null)
        {
            TimeSpan time = TimeSpan.FromSeconds(sessionTime);
            string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", time.Hours, time.Minutes, time.Seconds);

            timerText.text = $"Session Time: {formattedTime}";
        }
    }

    public void PauseTimer()
    {
        isRunning = false;
    }

    public void ResumeTimer()
    {
        isRunning = true;
    }

    public void ResetTimer()
    {
        sessionTime = 0f;
        UpdateTimerDisplay();
    }

    public float GetSessionTime()
    {
        return sessionTime;
    }
}
