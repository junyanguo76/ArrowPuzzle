using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Head : MonoBehaviour
{
    public float moveSpeed = 5f;
    private bool isMoving = false;
    public bool isBlocked;
    public float lifetime = 4f; // �ƶ�ʱ�䣨�룩
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

        // ��� LineRenderer
        LineRenderer lr = gameObject.AddComponent<LineRenderer>();
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.material = new Material(Shader.Find("Sprites/Default"));

        // ��ȡ Line ��Ӧ�ĵ��б�
        List<Vector2> points = GridManager.Instance.lines[index].points;

        // ���� LineRenderer ����
        lr.positionCount = points.Count;

        // ������ֵ
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

        // ���ǰ������ֱ��
        Debug.DrawRay(transform.position, dir.normalized * checkDistance, Color.red, 12f);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, checkDistance);

        // ��������
        if (hit.collider != null && hit.collider.gameObject != this.gameObject)
            isBlocked = true;  // ǰ�����ϰ�������
        else
            isBlocked = false; // ǰ�����ϰ������Զ�
    }

    IEnumerator MoveInDirection()
    {
        float timer = 0f;

        // ��ȡ��Ӧ Line
        Line line = GridManager.Instance.lines[index];

        // ��ȡ LineRenderer
        LineRenderer lr = GetComponent<LineRenderer>();
        lr.positionCount = line.points.Count;
        lr.useWorldSpace = true;

        // ���� points ������ʱ�б�
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < line.points.Count; i++)
            points.Add((Vector3)line.points[i]);

        while (timer < lifetime)
        {
            // ��һ������� Head ����λ��
            points[0] = transform.position;

            // �����㰴ǰһ����켣β��
            for (int i = 1; i < points.Count; i++)
            {
                points[i] = Vector3.Lerp(points[i], points[i - 1], moveSpeed * Time.deltaTime);
            }

            // ���� LineRenderer
            for (int i = 0; i < points.Count; i++)
            {
                lr.SetPosition(i, points[i]);
            }

            // �ƶ� Head ����
            transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null;
        }

        // �ƶ����������ٸ����壨Line + Head��
        Destroy(transform.parent.gameObject);
    }
    public void SetupEdgeCollider()
    {
        // ���û�� EdgeCollider2D�������
        edgeCollider = GetComponent<EdgeCollider2D>();
        if (edgeCollider == null)
            edgeCollider = gameObject.AddComponent<EdgeCollider2D>();

        // ��ȡ��Ӧ Line
        Line line = GridManager.Instance.lines[index];

        // �� Line.points ת���� **�ֲ�����**
        Vector2[] localPoints = line.points
            .Select(p => (Vector2)transform.InverseTransformPoint(new Vector3(p.x, p.y, 0f)))
            .ToArray();

        edgeCollider.points = localPoints;

        // ������ײ���Ⱥʹ���ģʽ
        edgeCollider.edgeRadius = 0.1f;
        edgeCollider.isTrigger = true;
    }
}
