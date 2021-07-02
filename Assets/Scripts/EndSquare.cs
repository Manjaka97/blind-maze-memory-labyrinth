using UnityEngine;

public class EndSquare : MonoBehaviour
{
    public LevelManager level;
 
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        level.ClearLevel();
    }
}
