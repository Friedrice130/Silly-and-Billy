using UnityEngine;

public class BossDoorController : MonoBehaviour
{
    public void OpenDoor()
    {
        Debug.Log("Door: Opening path to the final portal after Cryptid defeat.");
        gameObject.SetActive(false);
    }
}
