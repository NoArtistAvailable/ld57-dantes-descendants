using System.Collections;
using elZach.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LD57
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager instance =>
            _instance.OrSetToActive(ref _instance, FindAnyObjectByType<LevelManager>);
        private static LevelManager _instance;

        public SceneReference nextScene;
        
        public Image transitionImage;
        public float duration = 0.6f;
        public AudioSource inAudio, outAudio;

        void Start()
        {
            this.StartCoroutine(TransitionIn());
        }

        public void LoadNext() => LoadScene(nextScene);
        
        private Coroutine alreadyStarted;
        public void LoadScene(SceneReference scene)
        {
            if (alreadyStarted != null) return;
            alreadyStarted = this.StartCoroutine(LoadRoutine(scene.value));
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            yield return this.StartCoroutine(TransitionOut());
            SceneManager.LoadScene(sceneName);
        }

        public IEnumerator TransitionIn()
        {
            inAudio.Play();
            transitionImage.gameObject.SetActive(true);
            float progress = 0f;
            while (progress <= 1f)
            {
                transitionImage.material.SetFloat("_Progress", progress);
                progress += Time.deltaTime / duration;
                yield return null;
            }
            transitionImage.gameObject.SetActive(false);
        }

        public IEnumerator TransitionOut()
        {
            outAudio.Play();
            transitionImage.gameObject.SetActive(true);
            float progress = 0f;
            while (progress <= 1f)
            {
                transitionImage.material.SetFloat("_Progress", 1 - progress);
                progress += Time.deltaTime / duration;
                yield return null;
            }
        }

    }
}