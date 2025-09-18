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
    

    // 构造函数，实例化时自动生成点
    public Line()
    {
        CreateFirstPoint();
        CreateSecondPoint();
        CreateNextPoint();

    }

    void CreateFirstPoint()
    {
        Vector2 p1 = new Vector2(Random.Range(-10, 11), Random.Range(-10, 11));
        GridPoint point = GridManager.Instance.pointsLocation.Find(p => p.position == p1);

        if (point == null || point.occupied)
        {
            // 如果找不到或者已占据，就重新生成 p1
            // 在循环里，直到生成未被占据的点
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
        Vector2 p1 = points[^1]; // C# 8.0 语法，最后一个元素
        Vector2 p2 = points[^2];
        Vector2 p3;

        if (Random.value <= 0.9f) // 9/10 概率
        {
            p3 = p1 + p1 - p2;
        }
        else
        {
            if (p1.x - p2.x == 0) // 垂直方向
            {
                p3 = p1 + (Random.value < 0.5f ? Vector2.up : Vector2.down);
            }
            else // 水平方向
            {
                p3 = p1 + (Random.value < 0.5f ? Vector2.right : Vector2.left);
            }
        }

        CheckPointPosition(p3);
    }

    void CheckPointPosition(Vector2 p3)
    {
        var gridPoint = GridManager.Instance.pointsLocation.Find(p => p.position == p3);
        if (gridPoint != null && !gridPoint.occupied)
        {
            points.Add(p3);
            gridPoint.occupied = true;
        }
    }

    void CreateSecondPoint()
    {
        Vector2 p1 = points[0]; // 上一条点
        RandomDirection();
        direction = DirectionCheck(direction, p1);
        Vector2 p2 = p1 - direction;
        points.Add(p2);
        CheckPointPosition(p2);
    }

    Vector2 DirectionCheck(Vector2 direction, Vector2 currentPoint)
    {
        // 找出起点与当前点在同一行或同一列的线段
        List<Line> tempLines = GridManager.Instance.lines.FindAll(
            line => line.points[0].x == currentPoint.x || line.points[0].y == currentPoint.y
        );

        List<Vector2> tempDirections = new List<Vector2>();

        foreach (Line line in tempLines)
        {
            tempDirections.Add(line.direction); // 确保 Line.direction 是 public
        }

        // 避免头对头
        if (tempDirections.Contains(-direction))
        {
            // 如果遇到相反方向，选择第一个可用方向
            direction = tempDirections[0];
        }

        return direction; // 返回修改后的方向
    }
    void RandomDirection()
    {
        int random = Random.Range(0, 4);
        switch (random)
        {
            case 0: direction = Vector2.up; break; // 上
            case 1: direction = Vector2.down; break;// 下
            case 2: direction = Vector2.left; break; // 左
            case 3: direction = Vector2.right; break; // 右
        }
    }
}
