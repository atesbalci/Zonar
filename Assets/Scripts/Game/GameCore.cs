using System;
using UniRx;

namespace Game
{
    public class TransmissionCompletedEvent { }

    public enum GameState
    {
        AwaitingTransmission,
        Transmitting
    }

    public class GameCore
    {
        public static GameCore Instance { get { return _instance ?? (_instance = new GameCore()); } }
        public const float TransmissionDuration =2f;

        public GameState State
        {
            get { return _state; }
            set
            {
                if (value == GameState.Transmitting)
                {
                    Observable.Timer(TimeSpan.FromSeconds(TransmissionDuration)).Subscribe(l =>
                        {
                            State = GameState.AwaitingTransmission;
                        });
                }
                else if (value == GameState.AwaitingTransmission && _state != GameState.AwaitingTransmission)
                {
                    MessageBroker.Default.Publish(new TransmissionCompletedEvent());
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
