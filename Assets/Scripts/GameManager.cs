using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void SceneReloadController()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Scene scene = SceneManager. GetActiveScene();
            SceneManager. LoadScene(scene.name);
        }
    }
        
    // Update is called once per frame
    void Update()
    {
        SceneReloadController();
    }
}
