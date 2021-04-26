using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelUpButtonController : MonoBehaviour
{
    public UnityEvent onClicked;
    private void OnMouseDown()
    {
        onClicked.Invoke();
    }
}
