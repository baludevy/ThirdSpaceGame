using System;
using UnityEngine;

namespace Game
{
    public class LocalPlayer : MonoBehaviour
    {
        public static LocalPlayer Instance;

        public PlayerMovement movement;
        public Camera cam;
        
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }
    }
}
