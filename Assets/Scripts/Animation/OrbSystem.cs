using System;
using UnityEngine;

public class WhiteOrb : MonoBehaviour
{
    public float speed = 15f;

    private Transform target;
    private Action onArrive;

    public void Init(Transform target, Action onArrive)
    {
        this.target = target;
        this.onArrive = onArrive;
    }

    void Update()
    {
        if (!target)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            onArrive?.Invoke();
            Destroy(gameObject);
        }
    }
}
