using System.Collections;
using UnityEngine;
using TMPro;  // Import TextMeshPro namespace

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;  // Reference to the TextMeshProUGUI component
    private float score = 0f;  // Track how long the player has been alive
    private bool isAlive = true;  // Track whether the player is alive

    void Start()
    {
        score = 0f;  // Initialize the score
        UpdateScoreText();  // Update the UI at the start
    }

    void Update()
    {
        if (isAlive)
        {
            score += Time.deltaTime;  // Increase the score over time (1 per second)
            UpdateScoreText();  // Update the score text UI
        }
    }

    // Method to update the score text
    void UpdateScoreText()
    {
        scoreText.text = "Score: " + Mathf.FloorToInt(score).ToString();
    }

    // Call this method when the player dies
    public void PlayerDied()
    {
        isAlive = false;
    }
}
