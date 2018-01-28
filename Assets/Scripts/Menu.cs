using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game;
using UniRx;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

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

    public BlurOptimized BlurOptimized;
    public List<Tweener> Tweeners = new List<Tweener>(); 

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
        BlurOptimized = FindObjectOfType<BlurOptimized>();
    }

    private void ActivateGameOverMenu()
    {

        foreach (var tweener in Tweeners)
        {
            tweener.Kill(true);
        }
        Tweeners.Clear();
        Tweeners.Add(Header.transform.DOMoveX(Screen.width/2f, 0.3f));
        Tweeners.Add(Score.transform.DOMoveX(Screen.width/2f, 0.6f));
        Tweeners.Add(ScoreValue.transform.DOMoveX(Screen.width/2f, 0.7f));
        Tweeners.Add(HighScore.transform.DOMoveX(Screen.width/2f, 0.8f));
        Tweeners.Add(HighScoreValue.transform.DOMoveX(Screen.width/2f, 0.9f));
        Tweeners.Add(TapToRestart.transform.DOMoveX(Screen.width/2f, 1f));

        if (BlurOptimized != null)
        {
            BlurOptimized.enabled = true;
        }
    }

    private void DeactivateGameOverMenu()
    {
        foreach (var tweener in Tweeners)
        {
            tweener.Kill(true);
        }
        Tweeners.Clear();
        Tweeners.Add(Header.transform.DOMoveX(-1200f, 0.2f));
        Tweeners.Add(Score.transform.DOMoveX(-1200f, 0.2f));
        Tweeners.Add(ScoreValue.transform.DOMoveX(-1200f, 0.2f));
        Tweeners.Add(HighScore.transform.DOMoveX(-1200f, 0.2f));
        Tweeners.Add(HighScoreValue.transform.DOMoveX(-1200f, 0.2f));
        Tweeners.Add(TapToRestart.transform.DOMoveX(-1200f, 0.2f));

        if (BlurOptimized != null)
        {
            BlurOptimized.enabled = false;
        }
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
	        else if (GameCore.Instance.State == GameState.GameOver)
	        {
	            DeactivateGameOverMenu();
	            GameCore.Instance.Restart();
            }
	    }	
	}
}
