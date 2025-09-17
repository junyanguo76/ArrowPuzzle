using Kalkatos.DottedArrow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public GameObject arrowPrefab;
public int arrowCount = 4;     // ���ɼ�ͷ����
public float spacing = 2f;     // ���

private List<Vector2> usedPositions = new List<Vector2>();
private List<Vector2> usedDirections = new List<Vector2>();
private List<Cycle> arrows = new List<Cycle>(); // �������м�ͷ�� Cycle

void Start()
{
    Vector2[] spawnPositions = GeneratePositions(arrowCount, spacing,new Vector2(10,10));

    foreach (Vector2 pos in spawnPositions)
    {
        // ���ɼ�ͷ
        GameObject arrowObj = Instantiate(arrowPrefab, pos, Quaternion.identity);

        // ��ȡ��ͷ��β�ͽű�
        Arrow arrowScript = arrowObj.GetComponentInChildren<Arrow>();
        Cycle tailScript = arrowObj.GetComponentInChildren<Cycle>();

        // ���ɺϷ�����
        Vector2 dir = GetValidDirection(pos);
        arrowScript.SetDirection(dir);

        // ��ʼ��β��С��
        tailScript.InitializeTailBalls(dir);

        // ����ӵ��б�֮ǰ�ü�β�ͣ��������
        foreach (Cycle existing in arrows)
        {
            if (tailScript.pathPoints.Count < existing.pathPoints.Count)
                tailScript.TrimPathAtIntersection(existing.pathPoints);
            else
                existing.TrimPathAtIntersection(tailScript.pathPoints);
        }

        // �����ͷ��Ϣ
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

        // ��������������������
        for (int x = 0; x < cols; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Vector2 gridPos = new Vector2(x, y) * spacing - areaSize / 2f;
                availableGrid.Add(gridPos);
            }
        }

        // �����ȡ n ������
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