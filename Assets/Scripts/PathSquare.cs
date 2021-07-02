using System.Collections;
using UnityEngine;

public class PathSquare : MonoBehaviour
{
    SpriteRenderer sr;
    public Color a1;
    private Color a0;
    public float duration;
    private float time;
    private Coroutine lerpUp;
    private Coroutine lerpDown;

    // Start is called before the first frame update
    void Start()
    {
        a1 = new Color(0, 0, 0, 1);
        a0 = new Color(0, 0, 0, 0);
        sr = GetComponent<SpriteRenderer>();
        duration = .35f;
        time = 0f;
        lerpUp = null;
        lerpDown = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (lerpUp != null)
        {
            StopCoroutine(lerpUp);
        }
        lerpDown = StartCoroutine(LerpAlphaDown());
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (lerpDown != null)
        {
            StopCoroutine(lerpDown);
        }
        lerpUp = StartCoroutine(LerpAlphaUp());
    }

    IEnumerator LerpAlphaUp()
    {
        time = 0f;
        while(time < duration)
        {
            time += Time.deltaTime;
            sr.color = Color.Lerp(a0, a1, time / duration);
            yield return null;
        }
    }

    IEnumerator LerpAlphaDown()
    {
        time = 0f;
        while (time < .5f)
        {
            time += Time.deltaTime;
            sr.color = Color.Lerp(a1, a0, time / .5f);
            yield return null;
        }
    }
}
