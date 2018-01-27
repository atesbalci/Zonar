﻿using System;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Game
{
    public class CubesController : MonoBehaviour
    {
        private const int WaveRadius = 41;
        private const float RingWidth = 1f;
        private const float Speed = 3f;

        public GameObject CubePrefab;
        private GameObject _cubeParent;
        private List<ZCube> _cubes;
        private float _timer;
        private Tweener[] _tweeners;

        private void Start()
        {
            Tile();
            Vector3 pos = Vector3.zero;
            MessageBroker.Default.Receive<GameStateChangeEvent>().Subscribe(ev =>
            {
                if (ev.State == GameState.AwaitingTransmission)
                {
                    SetUserCube(GameCore.Instance.Player.transform.position);
                    foreach (var tweener in _tweeners)
                    {
                        tweener.Kill(true);
                    }
                    Update();
                }
                else if (ev.State == GameState.Transmitting)
                {
                    pos = GameCore.Instance.Player.transform.position;
                    var playerDistance = _cubeParent.transform.InverseTransformPoint(pos).magnitude * 2f;
                    _tweeners = new Tweener[_cubes.Count];
                    for (var i = 0; i < _cubes.Count; i++)
                    {
                        var cube = _cubes[i];
                        var y = cube.transform.localScale.y;
                        var dist = Vector3.Distance(cube.transform.position, pos);
                        _tweeners[i] = cube.transform.DOScaleY(dist < 1.5f ? ZCube.MaxHeight : 1f,
                            Mathf.Min(GameCore.TransmissionDuration - 0.1f,
                                GameCore.TransmissionDuration * (1f - dist / playerDistance))).SetEase(Ease.InOutSine);
                    }
                }
            });
        }

        public void Tile()
        {
            _cubes = new List<ZCube>();
            _cubeParent = new GameObject("Cubes");
            for (int x = 0; x < WaveRadius; x++)
            {
                for (int y = 0; y < WaveRadius; y++)
                {
                    //It's working don't fix it
                    var cube = Instantiate(CubePrefab, new Vector3(x - WaveRadius / 2 - 1, 0f, y - WaveRadius / 2 - 1) * 1.1f, Quaternion.identity).GetComponent<ZCube>();
                    cube.transform.SetParent(_cubeParent.transform);
                    _cubes.Add(cube);
                    cube.SetCubeType();
                }
            }
            _cubes[(WaveRadius/2+1) * WaveRadius + WaveRadius/2+1].Type = ZCubeType.Player;
        }

        private void Update()
        {
            var state = GameCore.Instance.State;
            if (state == GameState.AwaitingTransmission)
            {
                _timer += Time.deltaTime;
                if (_timer > 7f)
                {
                    _timer -= 7f;
                }
            }

            foreach (var cube in _cubes)
            {
                if (state == GameState.AwaitingTransmission)
                {
                    var scale = cube.transform.localScale;
                    if (cube.Type == ZCubeType.Player)
                    {
                        scale = new Vector3(1f, ZCube.MaxHeight, 1f);
                    }
                    else
                    {
                        scale.y = CalculateHeight(cube.transform.localPosition.magnitude, _timer, Speed,
                            RingWidth, 1f, ZCube.MaxHeight);
                    }

                    cube.transform.localScale = scale;
                }
                cube.RefreshColor();
            }
        }

        private static float CalculateHeight(float distance, float time, float speed, float width, float min, float max)
        {
            return Mathf.Clamp(max - Mathf.Abs(Mathf.Max(0f, Mathf.Abs(distance - time * speed) - width) * (max - min)), min, max);
        }

        public void SetUserCube(Vector3 pos)
        {
            _cubeParent.transform.position = pos;
            _timer = 0f;
            foreach (var zCube in _cubes) //set other cubes
            {
                zCube.SetCubeType();
            }
        }
    }
}
