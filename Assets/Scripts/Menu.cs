using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject GameOverMenu;

    [Header("GameOverMenu Stuff")]
    public Text Header;
    public Text Score;
    public Text ScoreValue;
    public Text HighScore;
    public Text HighScoreValue;
    public Text TapToRestart;

    void Start()
    {
        MessageBroker.Default.Receive<GameStateChangeEvent>().Subscribe(ev =>
        {
            if (ev.State == GameState.GameOver)
            {
                ActivateGameOverMenu();
                //TODO: Score calculation stuff here, blur stuff also
            }
        });
    }

    private void ActivateGameOverMenu()
    {
        Header.transform.DOMoveX(Screen.width/2f, 0.3f);
        Score.transform.DOMoveX(Screen.width/2f, 0.6f);
        ScoreValue.transform.DOMoveX(Screen.width/2f, 0.7f);
        HighScore.transform.DOMoveX(Screen.width/2f, 0.8f);
        HighScoreValue.transform.DOMoveX(Screen.width/2f, 0.9f);
        TapToRestart.transform.DOMoveX(Screen.width/2f, 1f);
    }

    void Update ()
    {
	    if (Input.GetMouseButton(0))
	    {
	        if (GameCore.Instance.State == GameState.Menu)
	        {
	            GameCore.Instance.State = GameState.AwaitingTransmission;
                MainMenu.SetActive(false);
	        }
	    }	
	}
}
