using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoaderScene : MonoBehaviour
{
    public GameObject loadGameCanvas;
    public GameObject loadScoreCanvas;

    public void LoadLevelGame(int idScene){
        StartCoroutine(LoadSceneGame(idScene));
    }

    public void LoadLevelScore(int idScene){
        StartCoroutine(LoadSceneScore(idScene));
    }

    public void LoadLevelSelection(int idScene){
        StartCoroutine(LoadSceneSelection(idScene));
    }

    IEnumerator LoadSceneGame(int idScene){
        AsyncOperation op = SceneManager.LoadSceneAsync(idScene);

        loadGameCanvas.SetActive(true);

        while(!op.isDone){
            Debug.Log(op.progress);
            yield return null;
        }
    }

    IEnumerator LoadSceneScore(int idScene){
        AsyncOperation op = SceneManager.LoadSceneAsync(idScene);

        loadScoreCanvas.SetActive(true);

        while(!op.isDone){
            Debug.Log(op.progress);
            yield return null;
        }
    }

    IEnumerator LoadSceneSelection(int idScene){
        AsyncOperation op = SceneManager.LoadSceneAsync(idScene);

        while(!op.isDone){
            Debug.Log(op.progress);
            yield return null;
        }
    }
}
