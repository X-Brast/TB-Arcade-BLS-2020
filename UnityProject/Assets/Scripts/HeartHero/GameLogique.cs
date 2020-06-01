using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLogique : MonoBehaviour
{
    private int multiplier = 1;
    private int streak = 0;

    void Start()
    {
        UpdateGUI();
        PlayerPrefs.SetInt("Score", 0);
        PlayerPrefs.SetInt("Highstreak", 0);
        PlayerPrefs.SetInt("NotesHit", 0);
    }

    public void AddStreak(){
        streak++;

        if(streak > 50)
            multiplier = 10;
        else if(streak > 40)
            multiplier = 5;
        else if(streak > 30)
            multiplier = 4;
        else if(streak > 20)
            multiplier = 3;
        else if(streak > 10)
            multiplier = 2;
        else
            multiplier = 1;

        if(streak > PlayerPrefs.GetInt("HighStreak"))
            PlayerPrefs.SetInt("Highstreak", streak);

        PlayerPrefs.SetInt("NotesHit", PlayerPrefs.GetInt("NotesHit")+1);

        UpdateGUI();
    }

    public void ResetStreak(){
        streak = 0;
        multiplier = 1;
        UpdateGUI();
    }

    private void UpdateGUI(){
        PlayerPrefs.SetInt("Streak", streak);
        PlayerPrefs.SetInt("Mult", multiplier);
    }

    public void EndGame(){
        if(PlayerPrefs.GetInt("Score") > PlayerPrefs.GetInt("Highscore"));
            PlayerPrefs.SetInt("Highscore", PlayerPrefs.GetInt("Score"));
        SceneManager.LoadScene("MusicHeroClassement");
    }

    public int GetScore(){
        return 100 * multiplier;
    }
}
