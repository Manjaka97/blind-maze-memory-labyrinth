using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public Transform movePoint;
    public LayerMask whatStopsMovement;
    private float dirX;
    private float dirY;
    private Color white;
    private Color black;
    private SpriteRenderer sr;
    private bool blinking;
    public Vector3 initialPosition;

    // Start is called before the first frame update
    void Start()
    {
        movePoint.parent = null;
        speed = 4f;
        white = Color.white;
        black = Color.black;
        sr = GetComponent<SpriteRenderer>();
        blinking = false;
        initialPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Calling blinking coroutine after each animation completed
        if (!blinking)
        {
            blinking = true;
            StartCoroutine(Blink());
        }

        // Player movement
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, speed * Time.deltaTime);

        dirX = SimpleInput.GetAxisRaw("Horizontal");
        dirY = SimpleInput.GetAxisRaw("Vertical");

        if (Vector3.Distance(transform.position, movePoint.position) == 0f)
        {
            if (Mathf.Abs(dirX) >= .75f)
            {
                if (!Physics2D.OverlapCircle(movePoint.position + new Vector3(Mathf.Round(dirX), 0f, 0f), .2f, whatStopsMovement))
                {
                    movePoint.position += new Vector3(Mathf.Round(dirX), 0f, 0f);
                    AudioManager.Instance.Play("move1");
                    AudioManager.Instance.Play("move2");
                }
                
            }

            else if (Mathf.Abs(dirY) >= .75f)
            {
                if (!Physics2D.OverlapCircle(movePoint.position + new Vector3(0f, Mathf.Round(dirY), 0f), .2f, whatStopsMovement))
                {
                    movePoint.position += new Vector3(0f, Mathf.Round(dirY), 0f);
                    AudioManager.Instance.Play("move1");
                    AudioManager.Instance.Play("move2");
                }
                
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Intersection")
        {
            AudioManager.Instance.Play("intersection");
        }
            
    }

    // Simple blinking animation by lerping from white to black and vice-versa
    IEnumerator Blink()
    {
        yield return StartCoroutine(BlinkBlack());
        yield return StartCoroutine(BlinkWhite());
        blinking = false;
    }

    IEnumerator BlinkBlack()
    {
        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime;
            sr.color = Color.Lerp(white, black, time);
            yield return null;
        }
    }

    IEnumerator BlinkWhite()
    {
        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime;
            sr.color = Color.Lerp(black, white, time);
            yield return null;
        }
    }
}
