using UnityEngine;

namespace Game
{
    class Player : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100))
                {
                    var cube = hit.transform.GetComponent<ZCube>();
                    if (cube != null && cube.Type == ZCubeType.Node && cube.transform.localScale.y>8f) //NodeSelect Logic here
                    {
                        transform.position = cube.transform.position;
                    }
                }
            }
        }


    }
}
