using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject Player;
    public float moveSpeed = 5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void OnMove()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Player.transform.Translate(new Vector3(h,v,0)* moveSpeed * Time.deltaTime);
    }
    // Update is called once per frame
    void Update()
    {
        OnMove();
    }
}
