using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    public GameObject headPrefab;


    public float axisLength = 1.0f; // �����᳤��
    public LineRenderer lineRenderer;
    public float maxDistance = 1.0f;

    public int xMax = 10; // x�������������
    public int yMax = 10; // y�������������
    public int linesNum = 5;

    public List<GridPoint> pointsLocation = new List<GridPoint>();
    public List<Line> lines = new List<Line>();
    void Start()
    {
        Instance = this;
        GeneratePoints();
        GenerateLines(linesNum);
        // ���� LineRenderer
        DrawLines();

    }

    void DrawLines()
    {
        foreach (Line line in lines)
        {
            // ÿ�����½�һ�� GameObject
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
        pointsLocation.Clear(); // ����б���ֹ�ظ����

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
        // ����X�ᣨ��ɫ��
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * axisLength);

        // ����Y�ᣨ��ɫ��
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * axisLength);

        // ����Z�ᣨ��ɫ��
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.forward * axisLength);
    }
}

[System.Serializable]
public class GridPoint
{
    public Vector2 position; // ����
    public bool occupied;    // �Ƿ�ռ��

    public GridPoint(Vector2 pos)
    {
        position = pos;
        occupied = false; // Ĭ��δռ��
    }
}