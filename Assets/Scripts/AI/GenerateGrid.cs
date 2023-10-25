using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateGrid : MonoBehaviour
{
    [SerializeField] Vector2Int gridSize;
    [SerializeField] Vector2 spacing;

    Vector2Int[] cells;

    public Vector2Int[] Cells => cells;

    public void Init() {
        cells = new Vector2Int[gridSize.x * gridSize.y];
        for (int y = 0; y < gridSize.y; y++) {
            for (int x = 0; x < gridSize.x; x++) {
                cells[y * gridSize.x + x] = new Vector2Int((int)transform.position.x + x + (int)(x * spacing.x), (int)transform.position.z + y + (int)(y * spacing.y));
            }
        }
    }


    private void OnDrawGizmosSelected() {
        for (int x = 0; x < gridSize.x; x++) {
            for (int y = 0; y < gridSize.y; y++) {
                Gizmos.DrawSphere(
                    new Vector3Int((int)transform.position.x + x + (int)(x * spacing.x), (int)transform.position.y, (int)transform.position.z + y + (int)(y * spacing.y)),
                    0.5f
                    );
            }
        }
    }
}
