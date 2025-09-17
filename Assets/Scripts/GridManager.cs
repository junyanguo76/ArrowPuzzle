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

                // ���ɼ�ͷ
                GameObject arrowObj = Instantiate(arrowPrefab, spawnPos, Quaternion.identity, transform);

                // ��Ǹ�����ռ��
                occupied[x, y] = true;

                // ����β��С��
                int ballCount = Random.Range(minBalls, maxBalls + 1);
                Vector2 dir = arrowObj.GetComponent<Arrow>().dir;

                Vector2 lastPos = spawnPos;
                for (int i = 0; i < ballCount; i++)
                {
                    lastPos -= dir * spacing;  // ���ͷ�෴��������С��
                    GameObject ball = Instantiate(ballPrefab, lastPos, Quaternion.identity, transform);
                    // ������Բ�ռ�ø��ӣ����߸����������ռ��
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

