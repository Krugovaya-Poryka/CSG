using UnityEngine;

public class GridMover : MonoBehaviour
{
    public GridBehavior grid;
    public float speed = 3f;

    private int currentIndex = 0;
    private bool moving = false;

    void Update()
    {
        if (!moving)
            return;

        if (currentIndex >= grid.path.Count)
        {
            moving = false;
            return;
        }

        Vector3 target = grid.path[currentIndex].transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            currentIndex++;
        }
    }

    public void StartMoving()
    {
        currentIndex = 0;
        moving = true;
    }
}