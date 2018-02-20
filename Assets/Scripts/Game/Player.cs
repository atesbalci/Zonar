﻿using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Game
{
    public class Player : MonoBehaviour
    {
        public Vector3 CamOffset;
        public List<int> Boosts { get; private set; }
        private int _boostSteps;
        public const int BoostLimit = 3;
        public bool IsBoostActive;
        public Vector3 GoalPosition;
        public ZCubeType CurrentCubeType;
        public ZCubeType NextCubeType;
        private bool _debugboost;

        public int Level = 1;
        public int Score;

        private void Start()
        {
            DOTween.Init();
            CamOffset = Camera.main.transform.position;
            GameCore.Instance.Player = this;
            CalculateGoalPosition();
            Boosts = new List<int>();

            MessageBroker.Default.Receive<GameStateChangeEvent>().Subscribe(ev =>
            {
                if (ev.State == GameState.AwaitingTransmission)
                {
                    CurrentCubeType = NextCubeType;
                    if (CurrentCubeType == ZCubeType.Goal)
                    {
                        GameCore.Instance.State = GameState.LevelCompleted;
                    }
                }
            });
        }

        public void CalculateGoalPosition()
        {
            GoalPosition = new Vector3(Random.Range(100*Level, 150 * Level) * CubesController.Gap, 0f, Random.Range(-150 * Level, 150 * Level) * CubesController.Gap);
        }

        void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyUp(KeyCode.Space))
            {
                IsBoostActive = !IsBoostActive;
                _debugboost = !_debugboost;
            }
#endif
            if (IsBoostActive && GameCore.Instance.State == GameState.AwaitingTransmission)
            {
                BoostMove();
            }
            else if (Input.GetMouseButtonDown(0) && GameCore.Instance.State != GameState.GameOver && GameCore.Instance.State == GameState.AwaitingTransmission)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                var hits = Physics.SphereCastAll(ray, 2f, 1000);
                var selectedHits =
                    hits.Where(y => y.transform.localScale.y > 8f)
                        .OrderBy(x => (ray.origin - x.transform.position).magnitude)
                        .ToList();
                var boost = selectedHits.Where(x =>
                {
                    var c = x.transform.GetComponent<ZCube>();
                    if (c != null && c.Type >= ZCubeType.Transmissive1
                                  && c.Type <= ZCubeType.Transmissive3)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }).ToList();

                if (boost.Any())
                {
                    selectedHits = boost.ToList();
                }

                foreach (var raycastHit in selectedHits)
                {
                    var selectedCube = raycastHit.transform.GetComponent<ZCube>();
                    if (selectedCube != null)
                    {
                        if (selectedCube.Type == ZCubeType.Basic)
                        {
                            continue;
                        }
                        selectedCube.LeaveGhost();
                        if (selectedCube.Type >= ZCubeType.Transmissive1
                            && selectedCube.Type <= ZCubeType.Transmissive3) //Set boost count
                        {
                            if (Boosts.Count > 0 && Boosts.First() != (int) selectedCube.Type)
                                Boosts.Clear();
                            Boosts.Add((int)selectedCube.Type);
                        }
                        if (Boosts.Count == BoostLimit)
                        {
                            IsBoostActive = true;
                            _boostSteps = 3 + Level; //TODO: change later
                        }
                        NormalMove(selectedCube);
                        break;
                    }
                }
            }
        }

        private void NormalMove(ZCube cube)
        {
            if (IsBoostActive || (((cube.Type >= ZCubeType.Transmissive1 && cube.Type <= ZCubeType.Transmissive3) || cube.Type == ZCubeType.Goal) && (cube.transform.localScale.y > 8f || IsBoostActive)))//NodeSelect Logic here
            {
                if (IsBoostActive)
                {
                    GameCore.TransmissionDuration = GameCore.GetBoostSpeed()*(transform.position - cube.transform.position).magnitude;
                }
                else
                {
                    GameCore.TransmissionDuration = GameCore.GetNormalSpeed()*(transform.position - cube.transform.position).magnitude;
                }

                Score += Mathf.CeilToInt((transform.position - cube.transform.position).magnitude * Level);
                transform.position = cube.transform.position;//Move player
                Camera.main.transform.DOMove(transform.position + CamOffset, GameCore.TransmissionDuration);
                GameCore.Instance.State = GameState.Transmitting;
                NextCubeType = cube.Type;
            }
        }

        private void BoostMove()
        {
            var controller = FindObjectOfType<CubesController>();
            if (controller != null)
            {
                var selectedCubeindex = 0;
                var distance = float.MaxValue;
                for (int i = 0; i < 50; i++)
                {
                    var no = Random.Range(0, controller.Cubes.Count);
                    var zCube = controller.Cubes[no];
                    var distanceToGoal = (zCube.transform.position - GoalPosition).magnitude;
                    var distanceFromPlayer = (zCube.transform.position - transform.position).magnitude;
                    if (distanceToGoal < distance && distanceFromPlayer < GameCore.MaxBoostLeapDistance)
                    {
                        selectedCubeindex = no;
                        distance = distanceToGoal;
                    }
                }
                NormalMove(controller.Cubes.FirstOrDefault(x => x.Type == ZCubeType.Goal) ?? controller.Cubes[selectedCubeindex]);

                if (--_boostSteps == 0 && !_debugboost) 
                {
                    IsBoostActive = false;
                    Boosts.Clear();
                }
            }
        }
    }
}
