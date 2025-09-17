using Kalkatos.DottedArrow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public GameObject arrowPrefab;
public int arrowCount = 4;     // 生成箭头数量
public float spacing = 2f;     // 间距

private List<Vector2> usedPositions = new List<Vector2>();
private List<Vector2> usedDirections = new List<Vector2>();
private List<Cycle> arrows = new List<Cycle>(); // 保存所有箭头的 Cycle

void Start()
{
    Vector2[] spawnPositions = GeneratePositions(arrowCount, spacing,new Vector2(10,10));

    foreach (Vector2 pos in spawnPositions)
    {
        // 生成箭头
        GameObject arrowObj = Instantiate(arrowPrefab, pos, Quaternion.identity);

        // 获取箭头和尾巴脚本
        Arrow arrowScript = arrowObj.GetComponentInChildren<Arrow>();
        Cycle tailScript = arrowObj.GetComponentInChildren<Cycle>();

        // 生成合法方向
        Vector2 dir = GetValidDirection(pos);
        arrowScript.SetDirection(dir);

        // 初始化尾巴小球
        tailScript.InitializeTailBalls(dir);

        // 在添加到列表之前裁剪尾巴，避免叠放
        foreach (Cycle existing in arrows)
        {
            if (tailScript.pathPoints.Count < existing.pathPoints.Count)
                tailScript.TrimPathAtIntersection(existing.pathPoints);
            else
                existing.TrimPathAtIntersection(tailScript.pathPoints);
        }

        // 保存箭头信息
        usedPositions.Add(pos);
        usedDirections.Add(dir);
        arrows.Add(tailScript);
    }
}

    Vector2[] GeneratePositions(int n, float spacing, Vector2 areaSize)
    {
        List<Vector2> positions = new List<Vector2>();

        int cols = Mathf.FloorToInt(areaSize.x / spacing);
        int rows = Mathf.FloorToInt(areaSize.y / spacing);

        List<Vector2> availableGrid = new List<Vector2>();

        // 生成所有整数格子坐标
        for (int x = 0; x < cols; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Vector2 gridPos = new Vector2(x, y) * spacing - areaSize / 2f;
                availableGrid.Add(gridPos);
            }
        }

        // 随机抽取 n 个格子
        for (int i = 0; i < n && availableGrid.Count > 0; i++)
        {
            int index = Random.Range(0, availableGrid.Count);
            positions.Add(availableGrid[index]);
            availableGrid.RemoveAt(index);
        }

        return positions.ToArray();
    }

    Vector2 GetValidDirection(Vector2 pos)
{
    List<Vector2> possibleDirs = new List<Vector2> { Vector2.right, Vector2.left, Vector2.up, Vector2.down };

    for (int i = possibleDirs.Count - 1; i >= 0; i--)
    {
        Vector2 candidate = possibleDirs[i];

        for (int j = 0; j < usedPositions.Count; j++)
        {
            Vector2 dirToOther = (usedPositions[j] - pos).normalized;

            if (dirToOther == candidate && usedDirections[j] == -candidate)
            {
                possibleDirs.RemoveAt(i);
                break;
            }
        }
    }

    if (possibleDirs.Count == 0)
        return Vector2.right;

    return possibleDirs[Random.Range(0, possibleDirs.Count)];
}
}