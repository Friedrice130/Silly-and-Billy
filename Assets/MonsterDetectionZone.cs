using UnityEngine;

public class MonsterDetectionZone : MonoBehaviour
{
    public ChasingMonster monster;
    public LayerMask playerLayer; // 

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            monster.player = other.transform;
            Debug.Log(" Player entered monster detection zone!");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            monster.player = null;
            Debug.Log(" Player left monster detection zone!");
        }
    }

    private void OnDrawGizmos()
    {
        
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            if (col is BoxCollider2D box)
            {
                Gizmos.DrawCube(box.offset, box.size);
            }
            else if (col is CircleCollider2D circle)
            {
                Gizmos.DrawSphere(circle.offset, circle.radius);
            }
        }
    }
}
