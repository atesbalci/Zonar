using UnityEngine;

namespace Game
{
    public enum ZCubeType
    {
        Basic,
        Node,
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

        private void Update()
        {
            _properties.SetColor("_Color", GetCubeColor() * Color.Lerp(Color.black, new Color(0.71f, 0.71f, 0.71f), (transform.localScale.y - 1f) / (MaxHeight - 1f)));
            _rend.SetPropertyBlock(_properties);
        }

        public Color GetCubeColor()
        {
            return Type == ZCubeType.Node ? Color.blue : Color.gray;
        }

        public void SetCubeType()
        {
            Type = Random.Range(0,100) < 3 ? ZCubeType.Node : ZCubeType.Basic;
        }
    }
}
