using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    public enum ZCubeType
    {
        Basic,
        Player,
        Goal,
        Transmissive1,
        Transmissive2,
        Transmissive3
    }

    public class ZCube : MonoBehaviour
    {
        public const float MaxHeight = 10f;
        public const float InactiveColorPrecision = 0.2f;
        public const float ColorPrecision = 0.025f;
        public static readonly Color DefaultColor = new Color(0.71f, 0.71f, 0.71f);
        public static readonly Color IdleColor = new Color(0.07f, 0.07f, 0.07f);

        public float RadialDistance { get; private set; }
        public Vector3 LocalPos { get; private set; }

        private Renderer _rend;
        private MaterialPropertyBlock _properties;
        private ZCubeType _type;

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
                col = Color.Lerp(IdleColor, GetCubeColor(Type) * DefaultColor, (transform.localScale.y - 1f) / (MaxHeight - 1f));
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

        public static Color GetCubeColor(ZCubeType type)
        {
            if (type == ZCubeType.Player) //bad practice 
            {
                type = GameCore.Instance.Player.CurrentCubeType;
            }
            switch (type)
            {
                case ZCubeType.Goal:
                    return Color.green;
                case ZCubeType.Transmissive1:
                    return new Color(1f, 0.34f, 1f);
                case ZCubeType.Transmissive2:
                    return new Color(1f, 0.71f, 0f);
                case ZCubeType.Transmissive3:
                    return new Color(0f, 1f, 0.82f);
                default:
                    return Color.clear;
            }
        }

        public void LeaveGhost()
        {
            const float duration = 0.5f;
            var ghost = Instantiate(_rend, _rend.transform.position, _rend.transform.rotation);
            ghost.transform.DOScale(transform.localScale * 4f, duration);
            ghost.material.shader = Shader.Find("Unlit/ColorTransparent");
            var col = GetCubeColor(Type);
            ghost.material.color = col;
            ghost.material.DOColor(new Color(col.r, col.g, col.b, 0f), duration / 2f)
                .SetDelay(duration / 2f).OnComplete(() => Destroy(ghost.gameObject));
        }

        public ZCubeType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                _properties.SetColor("_OverrideColor", GetCubeColor(Type));
                _rend.SetPropertyBlock(_properties);
            }
        }
    }
}
