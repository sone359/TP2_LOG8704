using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject scorePanel;
    public TMPro.TMP_Text scoreText;
    public int score = 0;
    
    private void Awake()
    {
        Instance = this;
        scorePanel.SetActive(false);
    }

    public void OnKidnappedCow()
    {
        ++score;
        scorePanel.SetActive(true);
        scoreText.text = score.ToString();
    }
}
