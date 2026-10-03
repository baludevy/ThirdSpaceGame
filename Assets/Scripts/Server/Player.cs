using System;
using UnityEngine;

namespace Server
{
    public class Player : Entity
    {
        public int id;
        public string username;

        public InputManager inputManager;
        public AnimationState animState;
        public Inventory inventory;

        public override void Tick(float deltaTime)
        {
            inputManager.ProcessInputs();
        }

        public void Initialize(int id, string username)
        {
            this.id = id;
            this.username = username;

            inputManager = new InputManager(this);
            inventory = new Inventory(27);
        }

        public override EntityUpdate GetUpdate()
        {
            return new PlayerUpdate
            {
                entityId = entityId,
                entityType = entityType,
                position = position,
                animationState = animState
            };
        }
    }   
}