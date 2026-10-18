using System;
using System.Collections.Generic;
using Client;
using UnityEngine;

namespace Game
{
    public class TileManager : MonoBehaviour
    {
        private List<Ground> groundObjects = new List<Ground>();

        [NonSerialized]
        public List<Tile> tiles = new List<Tile>();

        public static TileManager Instance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }
        
        private void Start()
        {
            uint id = 0;

            foreach (Ground ground in FindObjectsByType<Ground>())
            {
                groundObjects.Add(ground);

                Collider collider = ground.GetComponent<Collider>();
                if (collider == null) continue;

                Bounds bounds = collider.bounds;

                for (int x = Mathf.CeilToInt(bounds.min.x); x < bounds.max.x; x++)
                {
                    for (int z = Mathf.CeilToInt(bounds.min.z); z < bounds.max.z; z++)
                    {
                        tiles.Add(new Tile
                        {
                            id = id++,
                            position = new Vector3(x, bounds.max.y, z)
                        });

                        Debug.Log(new Vector3(x, bounds.max.y, z));
                    }
                }
                
            }
        }
        
        public Tile GetTileAtPosition(Vector3 position) => tiles.Find(x => (x.position - position).sqrMagnitude <= 0.01f * 0.01f);
    }
}
