using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line
{
    public List<Vector2> points = new List<Vector2>();
    public float offset = 1f;
    public Vector2 direction;
    int xMax = GridManager.Instance.xMax;
    int yMax = GridManager.Instance.yMax;
    

    // ���캯����ʵ����ʱ�Զ����ɵ�
    public Line()
    {
        CreateFirstPoint();
        CreateNextPoint();

    }

    void CreateFirstPoint()
    {
        Vector2 p1 = new Vector2(Random.Range(-10, 11), Random.Range(-10, 11));
        GridPoint point = GridManager.Instance.pointsLocation.Find(p => p.position == p1);

        if (point == null || point.occupied)
        {
            // ����Ҳ���������ռ�ݣ����������� p1
            // ��ѭ���ֱ������δ��ռ�ݵĵ�
            do
            {
                p1 = new Vector2(Random.Range(-xMax, xMax + 1), Random.Range(-yMax, yMax + 1));
                point = GridManager.Instance.pointsLocation.Find(p => p.position == p1);
            } while (point == null || point.occupied);
        }
        GridManager.Instance.pointsLocation.Find(p => p.position == p1).occupied = true;
        points.Add(p1);
    }

    void CreateNextPoint()
    {
        Vector2 p1 = points[0]; // ��һ����
        RandomDirection();
        direction = DirectionCheck(direction, p1);
        Vector2 p2 = p1 - direction;
        points.Add(p2);
    }

    Vector2 DirectionCheck(Vector2 direction, Vector2 currentPoint)
    {
        // �ҳ�����뵱ǰ����ͬһ�л�ͬһ�е��߶�
        List<Line> tempLines = GridManager.Instance.lines.FindAll(
            line => line.points[0].x == currentPoint.x || line.points[0].y == currentPoint.y
        );

        List<Vector2> tempDirections = new List<Vector2>();

        foreach (Line line in tempLines)
        {
            tempDirections.Add(line.direction); // ȷ�� Line.direction �� public
        }

        // ����ͷ��ͷ
        if (tempDirections.Contains(-direction))
        {
            // ��������෴����ѡ���һ�����÷���
            direction = tempDirections[0];
        }

        return direction; // �����޸ĺ�ķ���
    }
    void RandomDirection()
    {
        int random = Random.Range(0, 4);
        switch (random)
        {
            case 0: direction = Vector2.up; break; // ��
            case 1: direction = Vector2.down; break;// ��
            case 2: direction = Vector2.left; break; // ��
            case 3: direction = Vector2.right; break; // ��
        }
    }
}
