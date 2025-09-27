using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(LineRenderer))]
public class GrappleTongue : MonoBehaviour
{
    [Header("Grapple Settings")]
    public float grappleForce = 130f;   
    public float stopDistance = 0.5f;

    [Header("Layer Mask")]
    public LayerMask groundLayer;      

    private Rigidbody2D rb;
    private LineRenderer line;

    private Vector2 grapplePoint;
    private bool isGrappling = false;
    private bool straightLine = false;

    private float waveProgress = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        line = GetComponent<LineRenderer>();
        line.positionCount = 0;
    }

    void Update()
    {
        // Start grapple
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = mouseWorldPos - (Vector2)transform.position;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, Mathf.Infinity, groundLayer);

            if (hit.collider != null)
            {
                StartGrapple(hit.point);
            }
        }

        // Release grapple
        if (Input.GetMouseButtonUp(0))
        {
            StopGrapple();
        }
    }

    void FixedUpdate()
    {
        if (isGrappling)
        {
            Vector2 toTarget = grapplePoint - rb.position;
            float distance = toTarget.magnitude;

            if (distance > stopDistance)
            {
                // Apply pulling force toward grapple point
                rb.AddForce(toTarget.normalized * grappleForce);
            }
            else
            {
                StopGrapple();
            }
        }
    }

    void LateUpdate()
    {
        if (isGrappling)
        {
            if (straightLine)
            {
                line.positionCount = 2;
                line.SetPosition(0, transform.position);
                line.SetPosition(1, grapplePoint);
            }
            else
            {
                waveProgress += Time.deltaTime * 5f;
                DrawWavyRope();
            }
        }
    }

    void StartGrapple(Vector2 target)
    {
        grapplePoint = target;
        isGrappling = true;
        straightLine = true;
        waveProgress = 0f;
        line.positionCount = 2;
    }

    void StopGrapple()
    {
        isGrappling = false;
        straightLine = false;
        waveProgress = 0f;
        line.positionCount = 0;
    }

    void DrawWavyRope()
    {
        int segments = 20;
        line.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            Vector2 point = Vector2.Lerp(transform.position, grapplePoint, t);
            float wave = Mathf.Sin(t * Mathf.PI * 2f + waveProgress) * 0.1f;

            Vector2 perpendicular = Vector2.Perpendicular((grapplePoint - (Vector2)transform.position).normalized);
            Vector2 offset = perpendicular * wave;

            line.SetPosition(i, point + offset);
        }
    }
}
