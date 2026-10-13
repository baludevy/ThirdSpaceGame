using System;
using UnityEngine;

namespace Game
{
    public class ReflectionCamera : MonoBehaviour
    {
        public static ReflectionCamera Instance;

        public Camera cam;

        void Awake()
        {
            cam = GetComponent<Camera>();
            Instance = this;
        }
    }
}
