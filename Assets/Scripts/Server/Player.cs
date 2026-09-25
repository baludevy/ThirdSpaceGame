using System;
using UnityEngine;

namespace Server
{
    public class Player : Entity
    {
        public int id;
        public string username;
        
        [NonSerialized]
        public InputManager inputManager;
        
        public Game.AnimationState animState;

        public void Initialize(int id, string username)
        {
            this.id = id;
            this.username = username;
            
            inputManager = new InputManager(this);
        }
    }
}
