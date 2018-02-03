using System;
using UniRx;
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
        public const float InactiveColorPrecision = 0.2f;
        public const float ColorPrecision = 0.025f;
        public static readonly Color DefaultIdleColor = new Color(0.07f, 0.07f, 0.07f);
        public static readonly Color DefaultColor = new Color(0.71f, 0.71f, 0.71f);
        public static Color IdleColor = DefaultIdleColor;

        public ZCubeType Type { get; set; }
        public float RadialDistance { get; private set; }
        public Vector3 LocalPos { get; private set; }

        private Renderer _rend;
        private MaterialPropertyBlock _properties;

        public void Init()
        {
            _rend = GetComponentInChildren<Renderer>();
            _properties = new MaterialPropertyBlock();
            RadialDistance = transform.localPosition.magnitude;
            LocalPos = transform.localPosition;
        }

        public void RefreshColor(float noiseSeed)
        {
            Vector4 col;
            if (!Mathf.Approximately(transform.localScale.y, 1f))
            {
                col = Color.Lerp(IdleColor, GetCubeColor() * DefaultColor, (transform.localScale.y - 1f) / (MaxHeight - 1f));
                col /= ColorPrecision;
                col = new Vector4(Mathf.Round(col.x), Mathf.Round(col.y), Mathf.Round(col.z), Mathf.Round(col.w));
                col *= ColorPrecision;
            }
            else
            {
                noiseSeed = Mathf.Round(noiseSeed / InactiveColorPrecision) * InactiveColorPrecision - 0.5f;
                col = IdleColor + noiseSeed * new Color(0.03f, 0.03f, 0.03f);
            }
            _properties.SetColor("_Color", col);
            _rend.SetPropertyBlock(_properties);
        }

        public Color GetCubeColor()
        {
            var t = Type;
            if (Type == ZCubeType.Player) //bad practice 
            {
                t = GameCore.Instance.Player.CurrentCubeType;
            }
            switch (t)
            {
                case ZCubeType.Basic:
                return Color.clear;
                case ZCubeType.Transmissive:
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
                if ((GameCore.Instance.Player.GoalPosition - transform.position).magnitude < 0.5f)
                {
                    Type = ZCubeType.Goal;
                    Debug.Log(transform.TransformPoint(Vector3.zero));
                }
                else
                {
                    var randy = Random.Range(0f, 100f);
                    if (randy < Mathf.Clamp(2 - (GameCore.Instance.Player.Level - 1) * 0.5f, 0.5f, 100f)) // TODO: boost must be far from player 
                    {
                        Type = ZCubeType.Boost;
                    }
                    else if (randy < Mathf.Clamp(5 - (GameCore.Instance.Player.Level - 1) * 0.5f, 2f, 100f))
                    {
                        Type = ZCubeType.Transmissive;
                    }
                    else
                    {
                        Type = ZCubeType.Basic;
                    }
                }
            }
            _properties.SetColor("_OverrideColor", GetCubeColor());
            _rend.SetPropertyBlock(_properties);
        }
    }
}
