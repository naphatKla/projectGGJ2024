using Plugins.Singleton;
using UnityEngine;

namespace Manager
{
    public class ChopsticksController : MonoSingleton<ChopsticksController>
    {
        [SerializeField] private Vector2 offset;
        public Animator Animator => _animator;
        private Animator _animator;
        void Start()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
            _animator = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset;
            if (Input.GetMouseButtonDown(0)) 
                _animator.SetTrigger( "IsPick");
        }
    }
}
