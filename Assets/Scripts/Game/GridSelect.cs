using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Game
{
    public class GridSelect : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private GameObject previewPrefab;
    
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;
        [SerializeField] private Vector3 previewOffset;

        [SerializeField] private bool snapToGrid = true;

        public Vector2Int currentCell { get; private set; }
        public Vector3 cursorWorldPosition { get; private set; }
        public Vector3 cellCenter { get; private set; }
        public bool hasValidPosition { get; private set; }
    
        [SerializeField] private LayerMask groundLayers;

        private GameObject preview;

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            preview = Instantiate(previewPrefab);
            preview.SetActive(false);
        }

        private void Update()
        {
            hasValidPosition = false;

            if (targetCamera == null)
                targetCamera = Camera.main;

            if (preview == null)
                return;

            if (targetCamera == null || Mouse.current == null)
            {
                preview.SetActive(false);
                return;
            }

            Vector2 mousePosition = Mouse.current.position.ReadValue();

            if (!targetCamera.pixelRect.Contains(mousePosition))
            {
                preview.SetActive(false);
                return;
            }
            
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                hasValidPosition = false;

                if (preview != null)
                    preview.SetActive(false);

                return;
            }

            Ray ray = targetCamera.ScreenPointToRay(mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayers) || !hit.collider.TryGetComponent(out Ground ground))
            {
                preview.SetActive(false);
                return;
            }

            float targetY = ground.transform.position.y + ground.transform.localScale.y / 2 + 0.001f ;

            cursorWorldPosition = new Vector3(
                hit.point.x,
                targetY,
                hit.point.z
            );

            Vector3 localPosition = cursorWorldPosition - gridOrigin;

            currentCell = new Vector2Int(Mathf.FloorToInt(localPosition.x), Mathf.FloorToInt(localPosition.z));

            cellCenter = new Vector3(gridOrigin.x + currentCell.x + 0.5f, targetY, gridOrigin.z + currentCell.y + 0.5f);

            Vector3 position = snapToGrid ? cellCenter : cursorWorldPosition;
        
            position.x += previewOffset.x;
            position.z += previewOffset.z;

            preview.transform.position = position;
            preview.SetActive(true);
            hasValidPosition = true;
        }

        private void OnDisable()
        {
            hasValidPosition = false;

            if (preview != null)
                preview.SetActive(false);
        }

        private void OnDestroy()
        {
            if (preview != null)
                Destroy(preview);
        }
    }
}