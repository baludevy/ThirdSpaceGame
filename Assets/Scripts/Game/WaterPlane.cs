using System;
using UnityEngine;

namespace Game
{
    public class WaterPlane : MonoBehaviour
    {
        public static WaterPlane Instance;

        public Transform plane;

        void Awake()
        {
            plane = transform;
            Instance = this;
        }
    }
}
