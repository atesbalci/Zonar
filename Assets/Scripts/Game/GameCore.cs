using System;
using UniRx;

namespace Game
{
    public class GameStateChangeEvent
    {
        public GameState State { get; set; }
    }

    public enum GameState
    {
        AwaitingTransmission,
        Transmitting
    }

    public class GameCore
    {
        public static GameCore Instance { get { return _instance ?? (_instance = new GameCore()); } }
        public static float TransmissionDuration = 2f;

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
            State = GameState.AwaitingTransmission;
        }
    }
}
