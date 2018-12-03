using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Completed
{
    public class PauseController : MonoBehaviour
    {
        public Transform canvas;
        public bool isPaused;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Pause();
            }
        }
        public void Pause()
        {
            if (canvas.gameObject.activeInHierarchy == false)
            {
                Time.timeScale = 0;
                canvas.gameObject.SetActive(true);
            }
            else if (canvas.gameObject.activeInHierarchy == true)
            {
                canvas.gameObject.SetActive(false);
                Time.timeScale = 1;
            }

        }
        public void Quit()
        {
            Application.Quit();
        }
        public void MMSelect()
        {
            canvas.gameObject.SetActive(false);
            Time.timeScale = 1;
            GameManager.instance.gameObject.SetActive(false);
            GameManager.instance = null;
            SoundManager.instance.gameObject.SetActive(false);
            SoundManager.instance = null;
            Player.instance = null;
            SceneManager.LoadScene(0);
        }
    }
}
