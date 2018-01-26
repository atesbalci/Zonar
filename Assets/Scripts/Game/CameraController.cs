using UnityEngine;

namespace Game
{
    class CameraController : MonoBehaviour
    {
        private Player _player;
        private Vector3 _offset;
        public float Speed = 5;

        void Awake()
        {
            _player = FindObjectOfType<Player>();
            _offset = transform.position;
        }

        void Update()
        {
            transform.position = Vector3.Lerp(transform.position, _player.transform.position + _offset, Time.deltaTime * Speed);
        }
    }
}
