using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class Arrow : MonoBehaviour
{
    public Vector2 dir;
    public float moveSpeed = 5f;
    private bool isMoving = false;
    public bool isBlocked;
    public float lifetime = 4f; // �ƶ�ʱ�䣨�룩

    private void Awake()
    {

        Physics2D.queriesStartInColliders = false;

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

    
    public void SetDirection(Vector2 newDir)
    {
        dir = newDir;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

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

        while (timer < lifetime)
        {
            transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null; // �ȴ���һ֡
        }

        // �ƶ�ʱ�䵽�����ټ�ͷ
        Destroy(transform.parent.gameObject);
    }
}