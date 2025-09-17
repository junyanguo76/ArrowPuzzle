using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject arrowPrefab;
    public GameObject ballPrefab;
    public Vector2 areaSize = new Vector2(10, 10);
    public float cellSize = 2f;
    public int minBalls = 2;
    public int maxBalls = 5;
    public float spacing = 0.3f;

    private bool[,] occupied;

    void Start()
    {
        int cols = Mathf.FloorToInt(areaSize.x / cellSize);
        int rows = Mathf.FloorToInt(areaSize.y / cellSize);

        occupied = new bool[cols, rows];

        Vector2 origin = (Vector2)transform.position - areaSize / 2f + Vector2.one * cellSize / 2f;

        for (int x = 0; x < cols; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (occupied[x, y]) continue;

                Vector2 spawnPos = origin + new Vector2(x * cellSize, y * cellSize);

                // 生成箭头
                GameObject arrowObj = Instantiate(arrowPrefab, spawnPos, Quaternion.identity, transform);

                // 标记格子已占用
                occupied[x, y] = true;

                // 生成尾巴小球
                int ballCount = Random.Range(minBalls, maxBalls + 1);
                Vector2 dir = arrowObj.GetComponent<Arrow>().dir;

                Vector2 lastPos = spawnPos;
                for (int i = 0; i < ballCount; i++)
                {
                    lastPos -= dir * spacing;  // 向箭头相反方向生成小球
                    GameObject ball = Instantiate(ballPrefab, lastPos, Quaternion.identity, transform);
                    // 这里可以不占用格子，或者根据网格规则占用
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, areaSize.y, 0));
    }
}

