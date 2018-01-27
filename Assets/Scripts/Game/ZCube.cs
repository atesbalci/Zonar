﻿using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    public enum ZCubeType
    {
        Basic,
        Transmissive,
        Player,
        Boost,
        Goal,
    }

    public class ZCube : MonoBehaviour
    {
        public const float MaxHeight = 10f;
        public ZCubeType Type;

        private Renderer _rend;
        private MaterialPropertyBlock _properties;

        private void Start()
        {
            _rend = GetComponentInChildren<Renderer>();
            _properties = new MaterialPropertyBlock();
        }

        public void RefreshColor()
        {
            _properties.SetColor("_Color", GetCubeColor() * Color.Lerp(Color.black, new Color(0.71f, 0.71f, 0.71f), (transform.localScale.y - 1f) / (MaxHeight - 1f)));
            _rend.SetPropertyBlock(_properties);
        }

        public Color GetCubeColor()
        {
            switch (Type)
            {
                case ZCubeType.Basic:
                return Color.gray;
                case ZCubeType.Transmissive:
                return Color.blue;
                case ZCubeType.Player:
                return Color.blue;
                case ZCubeType.Boost:
                return Color.magenta;
                case ZCubeType.Goal:
                return Color.red;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetCubeType()
        {
            if (Type != ZCubeType.Player)
            {
                if ((GameCore.Instance.Player.GoalPosition - transform.position).magnitude < 0.6f)
                {
                    Type = ZCubeType.Goal;
                }
                else
                {
                    var randy = Random.Range(0f, 100f);
                    if (randy < 1) // TODO: boost must be far from player 
                    {
                        Type = ZCubeType.Boost;
                    }
                    else if (randy < 8)
                    {
                        Type = ZCubeType.Transmissive;
                    }
                    else
                    {
                        Type = ZCubeType.Basic;
                    }
                }
            }
        }
    }
}
