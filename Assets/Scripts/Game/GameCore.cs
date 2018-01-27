using System;
using UniRx;

namespace Game
{
    public enum GameState
    {
        AwaitingTransmission,
        Transmitting
    }

    public class GameCore
    {
        public static GameCore Instance { get { return _instance ?? (_instance = new GameCore()); } }
        public const float TransmissionDuration = 2f;

        public GameState State
        {
            get { return _state; }
            set
            {
                if (State == GameState.Transmitting)
                {
                    Observable.Timer(TimeSpan.FromSeconds(TransmissionDuration)).Subscribe(l =>
                        {
                            State = GameState.AwaitingTransmission;
                        });
                }
                _state = value;
            }
        }

        private static GameCore _instance;
        private GameState _state;

        public GameCore()
        {
            State = GameState.AwaitingTransmission;
        }
    }
}
