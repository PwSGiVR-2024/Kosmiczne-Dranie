using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public EnemyHeadquarters enemy;
    public PlayerHeadquarters player;

    public SoundManager soundManager;

    public Canvas worldSpaceCanvas;
    public Canvas screenSpaceCanvas;

    public bool logExterminationChamber = false;

    private Stack<GameObject> exterminationCamp = new();
    private Stack<GameObject> temporaryCamp = new();

    private bool gameOver = false;
    public UnityEvent onGameWin = new();
    public UnityEvent onGameLose = new();

    private IEnumerator ExterminationChamber()
    {
        if (logExterminationChamber)
            Debug.Log("chamber activated");

        GameObject obj;
        while (true)
        {
            if (exterminationCamp.TryPeek(out obj))
            {
                if (obj == null)
                {
                    if (logExterminationChamber)
                        Debug.Log("object is null");

                    exterminationCamp.Pop();
                }
                else
                {
                    if (logExterminationChamber)
                        Debug.Log("destroying: " + obj.name);

                    Destroy(obj);
                    exterminationCamp.Pop();
                    yield return null;
                }
            }

            if (temporaryCamp.TryPeek(out obj))
            {
                if (obj == null)
                {
                    if (logExterminationChamber)
                        Debug.Log("object is null");

                    temporaryCamp.Pop();
                }
                else if (obj.activeSelf)
                {
                    if (logExterminationChamber)
                        Debug.Log("object is active in temporary camp. Skipping iteration");
                }
                else if (obj)
                {
                    if (logExterminationChamber)
                        Debug.Log("destroying: " + obj.name);

                    Destroy(obj);
                    temporaryCamp.Pop();
                    yield return null;
                }
            }

            yield return null;
        }
    }

    private IEnumerator LoadSceneDelayed(string name)
    {
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene(name);
    }

    // Start is called before the first frame update
    void Start()
    {
        enemy.onDestroy.AddListener(() => {
            if (!gameOver){
                onGameWin.Invoke();
                StartCoroutine(LoadSceneDelayed("WinningGoodByeScene"));
                //SceneManager.LoadScene("WinningGoodByeScene");
        }
            gameOver = true;
        });

        player.onDestroy.AddListener(() => {
            if (!gameOver){
                onGameLose.Invoke();
                StartCoroutine(LoadSceneDelayed("LoosingGoodByeScene"));
                //SceneManager.LoadScene("LoosingGoodByeScene");
            }
            gameOver = true;
        });

        StartCoroutine(ExterminationChamber());
    }

    public void AddToExterminationCamp(GameObject obj)
    {
        exterminationCamp.Push(obj);
    }

    public void AddToTemporaryCamp(GameObject obj)
    {
        temporaryCamp.Push(obj);
    }
}
