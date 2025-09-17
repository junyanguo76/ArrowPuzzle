using Kalkatos.DottedArrow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cycle : MonoBehaviour
{
    public Transform arrow;           // ��ͷ����
    public GameObject ballPrefab;     // С�� prefab
    public int minBalls = 2;
    public int maxBalls = 8;
    public float spacing = 0.3f;      // С����
    [Range(0f, 1f)] public float bendProbability = 0.2f; // ���߸���
    public float followSpeed = 20f;   // С������ٶ�

    private List<GameObject> balls = new List<GameObject>();
    public List<Vector2> pathPoints = new List<Vector2>(); // β�͸��ӵ�
    private Material sharedMat;
    private bool hasInitialized = false;

    /// <summary>
    /// ��ʼ��β��С��ֻ�ܵ���һ��
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

            // ������ͷ�Ƿ��غ�
            if (Vector2.Distance(ball.transform.position, arrow.position) < 0.8f)
            {
                // ɾ����ǰ���ɵ�С��
                DestroyImmediate(ball);

                // ɾ��֮ǰ�Ѿ����ɵ�С��
                for (int j = balls.Count - 1; j >= 0; j--)
                {
                    if (balls[j] != null)
                    {
                        DestroyImmediate(balls[j]);
                        balls.RemoveAt(j);
                    }
                }

                // ɾ����ͷ
                if (arrow != null)
                    DestroyImmediate(arrow.gameObject);

                // ���β��·����
                pathPoints.Clear();

                return;
            }

            balls.Add(ball);

            // Ӧ�ò���
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
        // ��� pathPoints Ϊ�գ���Ӽ�ͷ��ʼλ����Ϊ��һ����
        if (pathPoints.Count == 0)
            pathPoints.Add(arrow.position);
    }

    void Update()
    {
        if (!hasInitialized || balls.Count == 0) return;

        Vector2 lastPoint = pathPoints[pathPoints.Count - 1];

        // ÿ�μ�ͷ�ƶ����¸��Ӳ�����µ�·����
        if (Vector2.Distance((Vector2)arrow.position, lastPoint) >= spacing)
        {
            Vector2 moveDir = (Vector2)arrow.position - lastPoint;

            // ˮƽ��ֱ����
            if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y))
                moveDir = new Vector2(Mathf.Sign(moveDir.x), 0);
            else
                moveDir = new Vector2(0, Mathf.Sign(moveDir.y));

            Vector2 newPoint = lastPoint + moveDir * spacing;
            pathPoints.Add(newPoint);

            // ����·������
            while (pathPoints.Count > balls.Count * 5)
                pathPoints.RemoveAt(0);
        }

        // С����·�������
        float distanceBehind = spacing;
        foreach (var ball in balls)
        {
            Vector2 targetPos = GetPointAlongPath(distanceBehind);
            ball.transform.position = Vector2.MoveTowards(ball.transform.position, targetPos, followSpeed * Time.deltaTime);
            distanceBehind += spacing;
        }
    }

    // ��ȡ·����ĳ�������
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

    // ��ȡ����һ����ֱ���������
    Vector2 GetPerpendicularDirection(Vector2 prevDir)
    {
        if (prevDir == Vector2.up || prevDir == Vector2.down)
            return Random.value < 0.5f ? Vector2.left : Vector2.right;
        else
            return Random.value < 0.5f ? Vector2.up : Vector2.down;
    }


    /// <summary>
    /// �޼���ǰβ��·�����������һ��β��·���ཻ��ɾ�����ཻ�㿪ʼ��β�������е�
    /// </summary>
    public void TrimPathAtIntersection(List<Vector2> otherPath)
    {
        for (int i = 0; i < pathPoints.Count; i++)
        {
            foreach (Vector2 otherPoint in otherPath)
            {
                if (Vector2.Distance(pathPoints[i], otherPoint) < 0.5f) // �ཻ��ֵ����΢��
                {
                    // ɾ�� i ֮���·����
                    int removeCount = pathPoints.Count - i;
                    pathPoints.RemoveRange(i, removeCount);

                    // ɾ����Ӧ��С��
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

                    return; // �ҵ���һ���ཻ���ͽ���
                }
            }
        }
    }

    public IEnumerator CleanupInvalidTails(float delay = 1f)
    {
        yield return new WaitForSeconds(delay);

        // ����β��С��ɾ�����ͷ�غϻ򲻺Ϲ��
        for (int i = balls.Count - 1; i >= 0; i--)
        {
            if (balls[i] == null) continue;

            // ���ͷ�غϻ��������Զ��岻�Ϲ�����
            if (Vector2.Distance(balls[i].transform.position, arrow.position) < 1f)
            {
                Destroy(balls[i]);
                balls.RemoveAt(i);
            }
        }

        // ���β��ȫ����ɾ�����׸��򲻺Ϲ棬ɾ����ͷ
        if (balls.Count == 0)
        {
            Destroy(arrow.gameObject);
            pathPoints.Clear();
        }
    }


}