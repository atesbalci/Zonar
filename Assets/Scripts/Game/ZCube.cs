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
        public ZCubeType Type;

        public void SetCubeType()
        {
            Type = Random.Range(0,100) < 3 ? ZCubeType.Node : ZCubeType.Basic;
            GetComponent<Renderer>().material.color = Type == ZCubeType.Node ? Color.blue : Color.gray;
        }
    }
}
