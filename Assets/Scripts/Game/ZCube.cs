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
            _properties.SetColor("_Color", Color.Lerp(Color.black, new Color(0.71f, 0.71f, 0.71f), transform.localScale.y / MaxHeight - 1f / MaxHeight));
            _rend.SetPropertyBlock(_properties);
        }
        public void SetCubeType()
        {
            Type = Random.Range(0,100) < 3 ? ZCubeType.Node : ZCubeType.Basic;
            GetComponent<Renderer>().material.color = Type == ZCubeType.Node ? Color.blue : Color.gray;
        }
    }
}
