using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    public GameObject headPrefab;


    public float axisLength = 1.0f; // 坐标轴长度
    public LineRenderer lineRenderer;
    public float maxDistance = 1.0f;

    public int xMax = 10; // x轴最大整数坐标
    public int yMax = 10; // y轴最大整数坐标
    public int linesNum = 5;

    public List<GridPoint> pointsLocation = new List<GridPoint>();
    public List<Line> lines = new List<Line>();
    void Start()
    {
        Instance = this;
        GeneratePoints();
        GenerateLines(linesNum);
        // 设置 LineRenderer
        DrawLines();

    }

    void DrawLines()
    {
        foreach (Line line in lines)
        {
            // 每条线新建一个 GameObject
            int lineIndex = GridManager.Instance.lines.IndexOf(line);

            GameObject lineObj = new GameObject("Line");
            lineObj.transform.parent = this.transform;
            GameObject head = Instantiate(headPrefab, (Vector3)line.points[0], Quaternion.identity, lineObj.transform);
            head.GetComponent<Head>().Init(line.direction,lineIndex);


        }
    }

    void GenerateLines(int num)
    {
        for(int i = 0; i < num; i++)
        {
            Line line = new Line();
            lines.Add(line);
        }
    }
    void GeneratePoints()
    {
        pointsLocation.Clear(); // 清空列表，防止重复添加

        for (int x = -xMax; x <= xMax; x++)
        {
            for (int y = -yMax; y <= yMax; y++)
            {
                pointsLocation.Add(new GridPoint(new Vector2(x, y)));
            }
        }
    }
    void OnDrawGizmos()
    {
        // 绘制X轴（红色）
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * axisLength);

        // 绘制Y轴（绿色）
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * axisLength);

        // 绘制Z轴（蓝色）
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.forward * axisLength);
    }
}

[System.Serializable]
public class GridPoint
{
    public Vector2 position; // 坐标
    public bool occupied;    // 是否被占据

    public GridPoint(Vector2 pos)
    {
        position = pos;
        occupied = false; // 默认未占据
    }
}