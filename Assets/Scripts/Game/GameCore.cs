using System;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game
{
    public class GameStateChangeEvent
    {
        public GameState State { get; set; }
    }

    public enum GameState
    {
        AwaitingTransmission,
        Transmitting,
        GameOver,
        LevelCompleted,
        Menu,
    }

    public class GameCore
    {
        public static GameCore Instance { get { return _instance ?? (_instance = new GameCore()); } }
        public static float TransmissionDuration = 0.75f;
        public static float MaxBoostLeapDistance = 20f;

        public Player Player { get; set; }

        public GameState State
        {
            get { return _state; }
            set
            {
                if (value == State)
                {
                    return;
                }
                if (value == GameState.Transmitting)
                {
                    Observable.Timer(TimeSpan.FromSeconds(TransmissionDuration)).Subscribe(l =>
                    {
                        State = GameState.AwaitingTransmission;
                    });
                }
                _state = value;
                MessageBroker.Default.Publish(new GameStateChangeEvent { State = value });
            }
        }

        private static GameCore _instance;
        private GameState _state;

        public GameCore()
        {
            State = GameState.Menu;
        }

        public void Restart(int level = 1)
        {
            //cubes,player,ui,endgame,timer,state,score,
            var cubeController = Object.FindObjectOfType<CubesController>();
            if (cubeController != null)
            {
                Player.transform.position = Vector3.zero;
                Player.Level = level == 1 ? 1 : Player.Level++;
                Player.Score = level == 1 ? 0 :Player.Score;
                Player.CurrentCubeType = ZCubeType.Basic;
                Player.NextCubeType = ZCubeType.Basic;
                Player.IsBoostActive = false;
                Camera.main.transform.position = Player.CamOffset;
                var trail = Object.FindObjectOfType<PlayerTrail>();
                if (trail != null)
                {
                    trail.transform.position = new Vector3(0f,10f,0f);
                }
                cubeController.Timer = 0;
                State = GameState.AwaitingTransmission;
                Player.ConsecutiveBoostCount = 0;
                Player.CalculateGoalPosition();
            }
        }

        public static float GetBoostSpeed()
        {
            return GetTransmissionDuration(0.05f, 0.005f, 0.01f);
        }

        public static float GetNormalSpeed()
        {
            return GetTransmissionDuration(0.1f, 0.01f, 0.02f);
        }

        private static float GetTransmissionDuration(float start, float levelDecrease, float minDuration)
        {
            return Mathf.Clamp(start - Instance.Player.Level * levelDecrease, minDuration, 1f);
        }
    }
}
