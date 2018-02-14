using System;
using System.Linq;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Game
{
    public class PlayerTrail : MonoBehaviour
    {
        private Tweener _levelCompletedTeweener;
        private IDisposable _levelDisposable = null;
        private TrailRenderer _trail;

        private void Start()
        {
            _trail = GetComponentInChildren<TrailRenderer>();
            MessageBroker.Default.Receive<GameStateChangeEvent>().Subscribe(ev =>
            {
                if (_levelCompletedTeweener != null)
                {
                    _levelCompletedTeweener.Kill(true);
                    _levelCompletedTeweener = null;
                }
                if (_levelDisposable != null)
                {
                    _levelDisposable.Dispose();
                }

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
                        var delta = diff * Mathf.Sign(move.x);
                        move.x -= delta;
                        seq.Append(transform.DOMoveX(delta, unitDur * diff).SetRelative());
                        dur -= unitDur * diff;
                    }
                    else if (diff < -0.001f)
                    {
                        var delta = -diff * Mathf.Sign(move.z);
                        move.z -= delta;
                        seq.Append(transform.DOMoveZ(delta, unitDur * -diff).SetRelative());
                        dur -= unitDur * -diff;
                    }
                    if (dur > 0.001f)
                    {
                        ZigZag(seq, move, dur);
                    }

                    var lastBoost = GameCore.Instance.Player.Boosts.LastOrDefault();
                    SetColor(lastBoost > 0 ?
                        ZCube.GetCubeColor((ZCubeType)lastBoost) : Color.white);
                }
                else if (ev.State == GameState.LevelCompleted)
                {
                    _levelDisposable = Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(l =>
                    {
                        _levelCompletedTeweener = transform.DOMoveY(150, 3f);
                    });
                }
            });
        }

        private void SetColor(Color col)
        {
            var gradient = new Gradient
            {
                alphaKeys = _trail.colorGradient.alphaKeys,
                colorKeys = new[]
                    {new GradientColorKey(col, 0f)}
            };
            _trail.colorGradient = gradient;
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
