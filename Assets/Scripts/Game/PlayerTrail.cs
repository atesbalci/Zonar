using System;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Game
{
    public class PlayerTrail : MonoBehaviour
    {
        private void Start()
        {
            MessageBroker.Default.Receive<GameStateChangeEvent>().Subscribe(ev =>
            {
                if (ev.State == GameState.Transmitting)
                {
                    var move = GameCore.Instance.Player.transform.position - transform.position;
                    var diff = Mathf.Abs(move.x) - Mathf.Abs(move.z);
                    var totalDist = Mathf.Abs(move.x) + Mathf.Abs(move.z);
                    var moveAmt = totalDist / CubesController.Gap;
                    var dur = GameCore.TransmissionDuration;
                    var seq = DOTween.Sequence();
                    var unitDur = dur / moveAmt;
                    if (diff > 0.001f)
                    {
                        move.x = Mathf.Sign(move.x) * Mathf.Abs(move.z);
                        seq.Append(transform.DOMoveX(diff * Mathf.Sign(move.x), unitDur * diff).SetRelative());
                        dur -= unitDur * diff;
                    }
                    else if (diff < -0.001f)
                    {
                        move.z = Mathf.Sign(move.z) * Mathf.Abs(move.x);
                        seq.Append(transform.DOMoveZ(-diff * Mathf.Sign(move.z), unitDur * -diff).SetRelative());
                        dur -= unitDur * -diff;
                    }
                    if (dur > 0.001f)
                    {
                        ZigZag(seq, move, dur);
                    }
                }
                else if (ev.State == GameState.LevelCompleted)
                {
                    Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(l =>
                    {
                        transform.DOMoveY(150, 3f);
                    });
                }
                
            });
        }

        private void ZigZag(Sequence seq, Vector3 move, float duration)
        {
            var amt = Mathf.RoundToInt(Mathf.Abs(move.x) / CubesController.Gap);
            var unitMove = Mathf.Abs(move.x) / amt;
            for (var i = 0; i < amt; i++)
            {
                seq.Append(transform.DOMoveX(Mathf.Sign(move.x) * unitMove, duration / (amt * 2))).SetRelative();
                seq.Append(transform.DOMoveZ(Mathf.Sign(move.z) * unitMove, duration / (amt * 2))).SetRelative();
            }
        }
    }
}
