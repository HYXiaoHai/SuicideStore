using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class CrackAnimationiContoller : MonoBehaviour
{
    public Transform circle;
    public Transform mask;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.anyKey)
        {
            StartCoroutine(PlayAnimation());
        }
    }

    IEnumerator PlayAnimation()
    {
        circle.localScale = Vector3.zero;
        mask.localScale = Vector3.zero;

        circle.DOScale(1f,2.5f).SetEase(Ease.InOutQuart);
        yield return new WaitForSeconds(1.5f);
        mask.DOScale(1f,1f).SetEase(Ease.OutCubic);
    }
}
