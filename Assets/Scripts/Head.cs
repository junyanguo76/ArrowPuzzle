using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Head : MonoBehaviour
{
    public float moveSpeed = 5f;
    private bool isMoving = false;
    public bool isBlocked;
    public float lifetime = 4f; // 移动时间（秒）
    public Vector2 dir = new Vector2();
    int index;
    private EdgeCollider2D edgeCollider;
    private void Awake()
    {

        Physics2D.queriesStartInColliders = false;
    }
    public void Init(Vector2 tempdir,int tempIndex)
    {
        float angle = Mathf.Atan2(tempdir.y, tempdir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        dir = tempdir;
        index = tempIndex;

        SetupEdgeCollider();

        // 添加 LineRenderer
        LineRenderer lr = gameObject.AddComponent<LineRenderer>();
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.material = new Material(Shader.Find("Sprites/Default"));

        // 获取 Line 对应的点列表
        List<Vector2> points = GridManager.Instance.lines[index].points;

        // 设置 LineRenderer 点数
        lr.positionCount = points.Count;

        // 遍历赋值
        for (int i = 0; i < points.Count; i++)
        {
            lr.SetPosition(i, (Vector3)points[i]);
        }

    }
    private void OnMouseDown()
    {
        BlockJudge();
        if (!isMoving && !isBlocked)
        {
            isMoving = true;
            StartCoroutine(MoveInDirection());
        }
    }

    // Start is called before the first frame update
    void BlockJudge()
    {
        float checkDistance = 1000f;

        // 检测前方整条直线
        Debug.DrawRay(transform.position, dir.normalized * checkDistance, Color.red, 12f);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, checkDistance);

        // 忽略自身
        if (hit.collider != null && hit.collider.gameObject != this.gameObject)
            isBlocked = true;  // 前方有障碍，不动
        else
            isBlocked = false; // 前方无障碍，可以动
    }

    IEnumerator MoveInDirection()
    {
        float timer = 0f;

        // 获取对应 Line
        Line line = GridManager.Instance.lines[index];

        // 获取 LineRenderer
        LineRenderer lr = GetComponent<LineRenderer>();
        lr.positionCount = line.points.Count;
        lr.useWorldSpace = true;

        // 复制 points 到运行时列表
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < line.points.Count; i++)
            points.Add((Vector3)line.points[i]);

        while (timer < lifetime)
        {
            // 第一个点跟随 Head 自身位置
            points[0] = transform.position;

            // 后续点按前一个点轨迹尾随
            for (int i = 1; i < points.Count; i++)
            {
                points[i] = Vector3.Lerp(points[i], points[i - 1], moveSpeed * Time.deltaTime);
            }

            // 更新 LineRenderer
            for (int i = 0; i < points.Count; i++)
            {
                lr.SetPosition(i, points[i]);
            }

            // 移动 Head 自身
            transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null;
        }

        // 移动结束，销毁父物体（Line + Head）
        Destroy(transform.parent.gameObject);
    }
    public void SetupEdgeCollider()
    {
        // 如果没有 EdgeCollider2D，则添加
        edgeCollider = GetComponent<EdgeCollider2D>();
        if (edgeCollider == null)
            edgeCollider = gameObject.AddComponent<EdgeCollider2D>();

        // 获取对应 Line
        Line line = GridManager.Instance.lines[index];

        // 将 Line.points 转换成 **局部坐标**
        Vector2[] localPoints = line.points
            .Select(p => (Vector2)transform.InverseTransformPoint(new Vector3(p.x, p.y, 0f)))
            .ToArray();

        edgeCollider.points = localPoints;

        // 调整碰撞体厚度和触发模式
        edgeCollider.edgeRadius = 0.1f;
        edgeCollider.isTrigger = true;
    }
}
