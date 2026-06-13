using UnityEngine;
using UnityEngine.InputSystem;
public class CarController : MonoBehaviour
{
    Rigidbody2D rb;
    Vector2 moveInput;
    float acceleration = 5f;
    float deacceleration = 5f;
    float maxSpeed = 6f;
    float turnSpeed = 200f;
    float currentSpeed = 0f;
    public float oilSlowFactor = 0.8f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    
    void FixedUpdate()
    {
        float moveAmount = moveInput.y;
        if (moveAmount != 0)
        {
            currentSpeed += moveAmount * acceleration * Time.fixedDeltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deacceleration * Time.fixedDeltaTime);
        }

        float turnAmount = -moveInput.x * turnSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation + turnAmount);
        
        rb.linearVelocity = transform.up * currentSpeed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("OilSpill"))
        {
            currentSpeed *= oilSlowFactor;
        }
    }

    void Update()
    {
        
    }
}
