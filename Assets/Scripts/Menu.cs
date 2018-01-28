using System;
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
    public Text InGameScore;

    [Header("GameOverMenu Stuff")]
    public Text Header;
    public Text Score;
    public Text ScoreValue;
    public Text HighScore;
    public Text HighScoreValue;
    public Text TapToRestart;

    [Header("Level Complete Stuff")]
    public Text LevelText;
    public Text LevelScore;
    public Text LevelScoreValue;
    public Text TapToNextLevel;

    public BlurOptimized BlurOptimized;
    public List<Tweener> Tweeners = new List<Tweener>();
    private bool _blockInput;

    void Start()
    {
        Header.transform.position = new Vector3(-Screen.width, Header.transform.position.y, Header.transform.position.z);
        Score.transform.position = new Vector3(-Screen.width, Score.transform.position.y, Score.transform.position.z);
        ScoreValue.transform.position = new Vector3(-Screen.width, ScoreValue.transform.position.y, ScoreValue.transform.position.z);
        HighScore.transform.position = new Vector3(-Screen.width, HighScore.transform.position.y, HighScore.transform.position.z);
        HighScoreValue.transform.position = new Vector3(-Screen.width, HighScoreValue.transform.position.y, HighScoreValue.transform.position.z);
        TapToRestart.transform.position = new Vector3(-Screen.width, TapToRestart.transform.position.y, TapToRestart.transform.position.z);
        LevelText.transform.position = new Vector3(-Screen.width, LevelText.transform.position.y, LevelText.transform.position.z);
        LevelScore.transform.position = new Vector3(-Screen.width, LevelScore.transform.position.y, LevelScore.transform.position.z);
        LevelScoreValue.transform.position = new Vector3(-Screen.width, LevelScoreValue.transform.position.y, LevelScoreValue.transform.position.z);
        TapToNextLevel.transform.position = new Vector3(-Screen.width, TapToNextLevel.transform.position.y, TapToNextLevel.transform.position.z);

        MessageBroker.Default.Receive<GameStateChangeEvent>().Subscribe(ev =>
        {
            if (ev.State == GameState.GameOver)
            {
                ActivateGameOverMenu();
                _blockInput = true;
                Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(l =>
                {
                    _blockInput = false;
                });
            }
            else if (ev.State == GameState.LevelCompleted)
            {
                ActivateLevelCompleteMenu();
                _blockInput = true;
                Observable.Timer(TimeSpan.FromSeconds(1.5f)).Subscribe(l =>
                {
                    _blockInput = false;
                });
            }
        });
        BlurOptimized = FindObjectOfType<BlurOptimized>();
    }

    void Update ()
    {
	    if (Input.GetMouseButton(0) && !_blockInput)
	    {
	        if (GameCore.Instance.State == GameState.Menu)
	        {
	            GameCore.Instance.State = GameState.AwaitingTransmission;
                MainMenu.SetActive(false);
	        }
	        else if (GameCore.Instance.State == GameState.GameOver)
	        {
	            GameCore.Instance.Restart();
	            DeactivateGameOverMenu();
            }
            else if (GameCore.Instance.State == GameState.LevelCompleted)
            {
	            GameCore.Instance.Restart(++GameCore.Instance.Player.Level);
                DeactivateLevelCompleteMenu();
            }
        }
        if (GameCore.Instance.State == GameState.LevelCompleted || GameCore.Instance.State == GameState.GameOver ||
            GameCore.Instance.State == GameState.Menu)
        {
            InGameScore.text = "";
        }
        else
        {
            InGameScore.text = GameCore.Instance.Player.Score.ToString("D");
        }
    }

    private void ActivateLevelCompleteMenu()
    {
        KillTweeners();
        LevelText.text = "Level " + (GameCore.Instance.Player.Level) + " Completed";
        LevelScoreValue.text = GameCore.Instance.Player.Score.ToString("D");
        Tweeners.Add(LevelText.transform.DOMoveX(Screen.width / 2f, 0.3f));
        Tweeners.Add(LevelScore.transform.DOMoveX(Screen.width / 2f, 0.6f));
        Tweeners.Add(LevelScoreValue.transform.DOMoveX(Screen.width / 2f, 0.7f));
        Tweeners.Add(TapToNextLevel.transform.DOMoveX(Screen.width / 2f, 0.8f));
        if (BlurOptimized != null)
        {
            BlurOptimized.enabled = true;
        }
    }

    private void DeactivateLevelCompleteMenu()
    {
        KillTweeners();
        Tweeners.Add(LevelText.transform.DOMoveX(-Screen.width, 0.2f));
        Tweeners.Add(LevelScore.transform.DOMoveX(-Screen.width, 0.2f));
        Tweeners.Add(LevelScoreValue.transform.DOMoveX(-Screen.width, 0.2f));
        Tweeners.Add(TapToNextLevel.transform.DOMoveX(-Screen.width, 0.2f));
        if (BlurOptimized != null)
        {
            BlurOptimized.enabled = false;
        }
    }

    private void ActivateGameOverMenu()
    {
        if (PlayerPrefs.HasKey("highscore")) //TODO: move highscore logic to somewhere else
        {
            var score = PlayerPrefs.GetInt("highscore");
            if (GameCore.Instance.Player.Score > score)
            {
                HighScoreValue.text = GameCore.Instance.Player.Score.ToString("D");
                PlayerPrefs.SetInt("highscore", GameCore.Instance.Player.Score);
                PlayerPrefs.Save();
            }
            else
            {
                HighScoreValue.text = PlayerPrefs.GetInt("highscore").ToString("D");
            }
        }
        else
        {
            HighScoreValue.text = GameCore.Instance.Player.Score.ToString("D");
            PlayerPrefs.SetInt("highscore", GameCore.Instance.Player.Score);
            PlayerPrefs.Save();
        }

        KillTweeners();
        ScoreValue.text = GameCore.Instance.Player.Score.ToString("D");
        Tweeners.Add(Header.transform.DOMoveX(Screen.width / 2f, 0.3f));
        Tweeners.Add(Score.transform.DOMoveX(Screen.width / 2f, 0.6f));
        Tweeners.Add(ScoreValue.transform.DOMoveX(Screen.width / 2f, 0.7f));
        Tweeners.Add(HighScore.transform.DOMoveX(Screen.width / 2f, 0.8f));
        Tweeners.Add(HighScoreValue.transform.DOMoveX(Screen.width / 2f, 0.9f));
        Tweeners.Add(TapToRestart.transform.DOMoveX(Screen.width / 2f, 1f));

        if (BlurOptimized != null)
        {
            BlurOptimized.enabled = true;
        }
    }

    private void DeactivateGameOverMenu()
    {
        KillTweeners();
        Tweeners.Add(Header.transform.DOMoveX(-Screen.width, 0.2f));
        Tweeners.Add(Score.transform.DOMoveX(-Screen.width, 0.2f));
        Tweeners.Add(ScoreValue.transform.DOMoveX(-Screen.width, 0.2f));
        Tweeners.Add(HighScore.transform.DOMoveX(-Screen.width, 0.2f));
        Tweeners.Add(HighScoreValue.transform.DOMoveX(-Screen.width, 0.2f));
        Tweeners.Add(TapToRestart.transform.DOMoveX(-Screen.width, 0.2f));

        if (BlurOptimized != null)
        {
            BlurOptimized.enabled = false;
        }
    }

    private void KillTweeners()
    {
        foreach (var tweener in Tweeners)
        {
            tweener.Kill(true);
        }
        Tweeners.Clear();
    }
}
