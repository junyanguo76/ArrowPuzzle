using Kalkatos.DottedArrow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cycle : MonoBehaviour
{
    public Transform arrow;           // 箭头对象
    public GameObject ballPrefab;     // 小球 prefab
    public int minBalls = 2;
    public int maxBalls = 8;
    public float spacing = 0.3f;      // 小球间距
    [Range(0f, 1f)] public float bendProbability = 0.2f; // 折线概率
    public float followSpeed = 20f;   // 小球跟随速度

    private List<GameObject> balls = new List<GameObject>();
    public List<Vector2> pathPoints = new List<Vector2>(); // 尾巴格子点
    private Material sharedMat;
    private bool hasInitialized = false;

    /// <summary>
    /// 初始化尾巴小球，只能调用一次
    /// </summary>
    public void InitializeTailBalls(Vector2 arrowDir)
    {
        if (hasInitialized) return;
        hasInitialized = true;

        Color lineColor = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.6f, 1f);
        Renderer arrowRenderer = arrow.GetComponent<Renderer>();
        if (arrowRenderer != null)
        {
            sharedMat = new Material(arrowRenderer.material);
            sharedMat.color = lineColor;
            arrowRenderer.material = sharedMat;
        }

        int ballCount = Random.Range(minBalls, maxBalls + 1);

        float initialOffset = spacing;
        Vector2 startPos = (Vector2)arrow.position - arrowDir * initialOffset;
        pathPoints.Clear();
        pathPoints.Add(startPos);

        Vector2 lastPos = startPos;
        Vector2 lastDir = -arrowDir;

        for (int i = 0; i < ballCount; i++)
        {
            if (i > 0 && Random.value < bendProbability)
                lastDir = GetPerpendicularDirection(lastDir);

            lastPos += lastDir * spacing;
            pathPoints.Add(lastPos);

            GameObject ball = Instantiate(ballPrefab, lastPos, Quaternion.identity, transform.parent);

            // 检查与箭头是否重合
            if (Vector2.Distance(ball.transform.position, arrow.position) < 0.8f)
            {
                // 删除当前生成的小球
                DestroyImmediate(ball);

                // 删除之前已经生成的小球
                for (int j = balls.Count - 1; j >= 0; j--)
                {
                    if (balls[j] != null)
                    {
                        DestroyImmediate(balls[j]);
                        balls.RemoveAt(j);
                    }
                }

                // 删除箭头
                if (arrow != null)
                    DestroyImmediate(arrow.gameObject);

                // 清空尾巴路径点
                pathPoints.Clear();

                return;
            }

            balls.Add(ball);

            // 应用材质
            if (sharedMat != null)
            {
                Renderer r = ball.GetComponent<Renderer>();
                if (r != null) r.material = sharedMat;
            }
        }
        StartCoroutine(CleanupInvalidTails());
    }

    private void Start()
    {
        // 如果 pathPoints 为空，添加箭头初始位置作为第一个点
        if (pathPoints.Count == 0)
            pathPoints.Add(arrow.position);
    }

    void Update()
    {
        if (!hasInitialized || balls.Count == 0) return;

        Vector2 lastPoint = pathPoints[pathPoints.Count - 1];

        // 每次箭头移动到新格子才添加新的路径点
        if (Vector2.Distance((Vector2)arrow.position, lastPoint) >= spacing)
        {
            Vector2 moveDir = (Vector2)arrow.position - lastPoint;

            // 水平或垂直方向
            if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y))
                moveDir = new Vector2(Mathf.Sign(moveDir.x), 0);
            else
                moveDir = new Vector2(0, Mathf.Sign(moveDir.y));

            Vector2 newPoint = lastPoint + moveDir * spacing;
            pathPoints.Add(newPoint);

            // 限制路径长度
            while (pathPoints.Count > balls.Count * 5)
                pathPoints.RemoveAt(0);
        }

        // 小球沿路径点跟随
        float distanceBehind = spacing;
        foreach (var ball in balls)
        {
            Vector2 targetPos = GetPointAlongPath(distanceBehind);
            ball.transform.position = Vector2.MoveTowards(ball.transform.position, targetPos, followSpeed * Time.deltaTime);
            distanceBehind += spacing;
        }
    }

    // 获取路径上某个距离点
    Vector2 GetPointAlongPath(float distance)
    {
        float accumulated = 0f;

        for (int i = pathPoints.Count - 1; i > 0; i--)
        {
            float segment = Vector2.Distance(pathPoints[i], pathPoints[i - 1]);
            if (accumulated + segment >= distance)
            {
                float t = (distance - accumulated) / segment;
                return Vector2.Lerp(pathPoints[i], pathPoints[i - 1], t);
            }
            accumulated += segment;
        }

        return pathPoints[0];
    }

    // 获取与上一方向垂直的随机方向
    Vector2 GetPerpendicularDirection(Vector2 prevDir)
    {
        if (prevDir == Vector2.up || prevDir == Vector2.down)
            return Random.value < 0.5f ? Vector2.left : Vector2.right;
        else
            return Random.value < 0.5f ? Vector2.up : Vector2.down;
    }


    /// <summary>
    /// 修剪当前尾巴路径：如果和另一条尾巴路径相交，删除从相交点开始到尾部的所有点
    /// </summary>
    public void TrimPathAtIntersection(List<Vector2> otherPath)
    {
        for (int i = 0; i < pathPoints.Count; i++)
        {
            foreach (Vector2 otherPoint in otherPath)
            {
                if (Vector2.Distance(pathPoints[i], otherPoint) < 0.5f) // 相交阈值，可微调
                {
                    // 删除 i 之后的路径点
                    int removeCount = pathPoints.Count - i;
                    pathPoints.RemoveRange(i, removeCount);

                    // 删除对应的小球
                    if (balls.Count >= removeCount)
                    {
                        int startIndex = Mathf.Max(0, balls.Count - removeCount);
                        for (int j = balls.Count - 1; j >= startIndex; j--)
                        {
                            if (balls[j] != null)
                                Destroy(balls[j]);
                            balls.RemoveAt(j);
                        }
                    }

                    return; // 找到第一个相交点后就结束
                }
            }
        }
    }

    public IEnumerator CleanupInvalidTails(float delay = 1f)
    {
        yield return new WaitForSeconds(delay);

        // 遍历尾巴小球，删除与箭头重合或不合规的
        for (int i = balls.Count - 1; i >= 0; i--)
        {
            if (balls[i] == null) continue;

            // 与箭头重合或者其他自定义不合规条件
            if (Vector2.Distance(balls[i].transform.position, arrow.position) < 1f)
            {
                Destroy(balls[i]);
                balls.RemoveAt(i);
            }
        }

        // 如果尾巴全部被删或者首个球不合规，删除箭头
        if (balls.Count == 0)
        {
            Destroy(arrow.gameObject);
            pathPoints.Clear();
        }
    }


}