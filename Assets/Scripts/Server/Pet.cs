using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Server
{
    public class Pet : Entity
    {
        private Entity target;
        private float stateTimeRemaining;

        public bool facingRight;
        
        public PetAnimationState animationState = PetAnimationState.Idle;

        public override void Tick(float deltaTime)
        {
            stateTimeRemaining -= deltaTime;

            if (stateTimeRemaining <= 0f || (animationState == PetAnimationState.Walk && target == null))
            {
                ChooseBehavior();
            }

            if (animationState != PetAnimationState.Walk || target == null)
            {
                return;
            }

            Vector2 offset = target.position - position;
            
            if (offset.x > 0f)
                facingRight = true;
            else if (offset.x < 0f)
                facingRight = false;
            
            const float followDistance = 2f;
            const float speed = 0.04f;

            if (offset.sqrMagnitude <= followDistance * followDistance)
            {
                animationState = PetAnimationState.Idle;
                return;
            }

            if (Mathf.Abs(offset.x) > followDistance)
            {
                position = new Vector2(
                    Mathf.MoveTowards(position.x, target.position.x, speed),
                    position.y
                );
            }
            else
            {
                position = new Vector2(
                    position.x,
                    Mathf.MoveTowards(position.y, target.position.y, speed)
                );
            }
        }

        private void ChooseBehavior()
        {
            List<Player> players = WorldManager.Instance.playerManager.players;
            target = null;

            float choice = Random.value;

            if (choice < 0.4f)
            {
                animationState = PetAnimationState.Idle;
                stateTimeRemaining = Random.Range(2f, 4f);
            }
            else if (choice < 0.1f || players == null || players.Count == 0)
            {
                animationState = PetAnimationState.Sleep;
                stateTimeRemaining = Random.Range(5f, 10f);
            }
            else
            {
                target = players[Random.Range(0, players.Count)];
                animationState = PetAnimationState.Walk;
                stateTimeRemaining = Random.Range(2f, 8f);
            }
        }
    }
}
