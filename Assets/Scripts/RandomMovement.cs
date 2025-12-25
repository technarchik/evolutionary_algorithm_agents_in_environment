using System.Collections;
using UnityEngine;

public class RandomMovement : MonoBehaviour
{
    [Header("Movement area")]
    public Vector3 areaCenter;
    public Vector3 areaSize = new Vector3(20, 0, 20);

    [Header("Movement settings")]
    public float moveSpeed = 2f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 4f;

    private Vector3 currentTarget;
    private bool isWalking = true;

    private Coroutine walkingCoroutine;


    private void Start()
    {
        areaCenter = transform.position;
        walkingCoroutine = StartCoroutine(WalkingRoutine());
    }

    private IEnumerator WalkingRoutine()
    {
        while (true)
        {
            // wait until can move
            yield return new WaitUntil(() => isWalking);

            // choose new target
            currentTarget = GetRandomPoint();

            // move to target
            while (Vector3.Distance(transform.position, currentTarget) > 0.2f)
            {
                if (!isWalking)
                    yield return null;

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    currentTarget,
                    moveSpeed * Time.deltaTime
                );

                yield return null;
            }

            // wait randomly
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);
        }
    }

    // stop walking at all
    public void PauseWalking()
    {
        isWalking = false;
    }

    // resume walking process
    public void ResumeWalking()
    {
        isWalking = true;
    }

    // force targeting to new target
    public void SetNewRandomTarget()
    {
        currentTarget = GetRandomPoint();
    }

    private Vector3 GetRandomPoint()
    {
        return new Vector3(
            areaCenter.x + Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
            transform.position.y,
            areaCenter.z + Random.Range(-areaSize.z / 2f, areaSize.z / 2f)
        );
    }
}
