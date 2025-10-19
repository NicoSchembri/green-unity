using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(LineRenderer))]
public class GrappleTongue : MonoBehaviour
{
    [Header("Tongue Origin")]
    public Transform mouthPoint;          // Works like firePoint — attach under player sprite

    [Header("Grapple Settings")]
    public float grappleForce = 130f;
    public float stopDistance = 0.5f;
    public float extendSpeed = 25f;
    public float retractSpeed = 35f;
    public float maxLength = 10f;

    [Header("Layer Mask")]
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private LineRenderer line;
    private Vector2 grapplePoint;
    private bool isGrappling = false;
    private bool extending = false;
    private bool retracting = false;

    private float currentLength = 0f;
    private Coroutine tongueRoutine;

    private Vector3 originalMouthLocalPos;
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        line = GetComponent<LineRenderer>();
        line.positionCount = 0;

        if (mouthPoint != null)
            originalMouthLocalPos = mouthPoint.localPosition;
    }

    void Update()
    {
        // Flip mouthPoint to match facing direction
        if (transform.localScale.x > 0 && !facingRight)
            FlipMouthPoint(true);
        else if (transform.localScale.x < 0 && facingRight)
            FlipMouthPoint(false);

        // Fire tongue
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouthPos = mouthPoint != null ? (Vector2)mouthPoint.position : (Vector2)transform.position;
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = mouseWorldPos - mouthPos;

            RaycastHit2D hit = Physics2D.Raycast(mouthPos, direction.normalized, maxLength, groundLayer);
            Vector2 target = (hit.collider != null) ? hit.point : mouthPos + direction.normalized * maxLength;

            if (tongueRoutine != null)
                StopCoroutine(tongueRoutine);

            tongueRoutine = StartCoroutine(ExtendTongue(target, hit.collider != null));
        }

        // Release tongue
        if (Input.GetMouseButtonUp(0))
            StopGrapple();
    }

    private IEnumerator ExtendTongue(Vector2 target, bool hitSomething)
    {
        isGrappling = false;
        extending = true;
        retracting = false;

        line.positionCount = 2;
        currentLength = 0f;

        Vector2 start = mouthPoint != null ? (Vector2)mouthPoint.position : (Vector2)transform.position;
        float totalDist = Vector2.Distance(start, target);

        while (currentLength < totalDist)
        {
            currentLength += extendSpeed * Time.deltaTime;
            Vector2 tip = Vector2.Lerp(start, target, currentLength / totalDist);
            DrawTongue(start, tip);
            yield return null;
        }

        extending = false;

        if (hitSomething)
        {
            isGrappling = true;
            grapplePoint = target;
        }
        else
        {
            tongueRoutine = StartCoroutine(RetractTongue());
        }
    }

    private IEnumerator RetractTongue()
    {
        retracting = true;

        Vector2 start = mouthPoint != null ? (Vector2)mouthPoint.position : (Vector2)transform.position;
        Vector2 end = line.GetPosition(1);
        float totalDist = Vector2.Distance(start, end);
        float retractProgress = 0f;

        while (retractProgress < 1f)
        {
            retractProgress += (retractSpeed / totalDist) * Time.deltaTime;
            Vector2 tip = Vector2.Lerp(end, start, retractProgress);
            DrawTongue(start, tip);
            yield return null;
        }

        retracting = false;
        StopGrapple();
    }

    void FixedUpdate()
    {
        if (isGrappling)
        {
            Vector2 toTarget = grapplePoint - rb.position;
            float distance = toTarget.magnitude;

            if (distance > stopDistance)
            {
                rb.AddForce(toTarget.normalized * grappleForce);
            }
            else
            {
                tongueRoutine = StartCoroutine(RetractTongue());
            }
        }
    }

    void LateUpdate()
    {
        if (line.positionCount > 0 && !extending && !retracting)
        {
            Vector2 start = mouthPoint != null ? (Vector2)mouthPoint.position : (Vector2)transform.position;
            DrawTongue(start, isGrappling ? grapplePoint : line.GetPosition(1));
        }
    }

    private void DrawTongue(Vector2 start, Vector2 end)
    {
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    void StopGrapple()
    {
        isGrappling = false;
        extending = false;
        retracting = false;
        line.positionCount = 0;
    }

    private void FlipMouthPoint(bool nowFacingRight)
    {
        facingRight = nowFacingRight;
        if (mouthPoint == null) return;

        Vector3 pos = originalMouthLocalPos;
        pos.x *= nowFacingRight ? 1 : -1;
        mouthPoint.localPosition = pos;
    }
}
