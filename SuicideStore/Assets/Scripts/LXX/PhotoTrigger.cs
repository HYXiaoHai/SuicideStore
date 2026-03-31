using UnityEngine;

public class PhotoTrigger : MonoBehaviour
{
    public int photoIndex;
    public PhotoSystem photoSystem;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && photoSystem != null)
        {
            photoSystem.OnPhotoTrigger(photoIndex);
        }
    }
}
