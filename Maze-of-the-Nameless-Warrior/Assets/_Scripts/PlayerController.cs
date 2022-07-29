using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stepSize = 2f;
    [SerializeField] private Transform movePoint;
    [SerializeField] private LayerMask whatStopsMovement;
    [SerializeField] public bool isInputEnabled = true;
    // Start is called before the first frame update
    void Start()
    {
        movePoint.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = isInputEnabled ? Input.GetAxisRaw("Horizontal") : 0f;
        float vertical = isInputEnabled ? Input.GetAxisRaw("Vertical") : 0f;
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, movePoint.position) <= 0f) {
            if (Mathf.Abs(horizontal) == 1) {
                if (!Physics2D.OverlapCircle(movePoint.position +
                    new Vector3(horizontal * stepSize, 0f, 0f), .2f, whatStopsMovement)) {
                    movePoint.position += new Vector3(horizontal * stepSize, 0f, 0f);
                }
            } else if (Mathf.Abs(vertical) == 1) {
                if (!Physics2D.OverlapCircle(movePoint.position +
                    new Vector3(0f, vertical * stepSize, 0f), .2f, whatStopsMovement)) {
                    movePoint.position += new Vector3(0f, vertical * stepSize, 0f);
                }
            }
        }
    }
}
