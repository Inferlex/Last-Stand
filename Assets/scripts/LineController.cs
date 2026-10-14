using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : MonoBehaviour
{

    private LineRenderer lr;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
    }

    public void SetPoints(Transform point1, Transform point2)
    {
        GetComponent<LineRenderer>().SetPosition(0, point1.position);
        GetComponent<LineRenderer>().SetPosition(1, point2.position);
        StartCoroutine(Vanish(3));
    }

    private IEnumerator Vanish(int i)
    {
        yield return new WaitForSeconds(0.1f);
        lr.startWidth -= 0.03f;
        lr.endWidth -= 0.03f;
        i--;
        if (i == 0)
            Destroy(gameObject);
        StartCoroutine(Vanish(i));
    }

}
