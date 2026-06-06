using UnityEngine;

/// <summary>
/// Bu scripti iklim trigger objesine ekleyin.
/// Clan objesi bu trigger alanına girdiğinde konsola "triggered" yazar.
///
/// Kurulum:
/// 1. İklim trigger objesine bir Collider ekleyin ve "Is Trigger" kutusunu işaretleyin.
/// 2. Clan objesine bir Collider ve Rigidbody ekleyin.
/// 3. Bu scripti iklim trigger objesine ekleyin.
/// </summary>
public class ClimateTrigger : MonoBehaviour
{
    // Clan objesi bu trigger alanına girdiğinde çalışır
    private void OnTriggerEnter(Collider other)
    {
        // Sadece "Clan" isimli objeyi kontrol et
        if (other.gameObject.name == "Clan")
        {
            Debug.Log("triggered");
        }
    }
}
