using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMover : MonoBehaviour
{
    public float moveSpeed = 5f;

    // ѕлавно рухаЇмо юн≥та по списку гекс≥в (корутина Unity)
    public IEnumerator MoveAlongPath(List<GameObject> path, System.Action onComplete)
    {
        foreach (GameObject hex in path)
        {
            Vector3 targetPos = hex.transform.position;
            // «бер≥гаЇмо Z-координату юн≥та дл€ правильност≥ шар≥в
            targetPos.z = transform.position.z;

            while (Vector3.Distance(transform.position, targetPos) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPos,
                    moveSpeed * Time.deltaTime
                );
                yield return null; // „екаЇмо наступного кадру
            }

            transform.position = targetPos;
        }

        onComplete?.Invoke();
    }
}