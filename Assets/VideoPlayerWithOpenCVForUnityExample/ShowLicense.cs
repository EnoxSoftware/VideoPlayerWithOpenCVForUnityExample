using UnityEngine;
using UnityEngine.SceneManagement;

namespace VideoPlayerWithOpenCVForUnityExample
{

    public class ShowLicense : MonoBehaviour
    {

        public void OnBackButtonClick()
        {
            SceneManager.LoadScene("VideoPlayerWithOpenCVForUnityExample");
        }
    }
}
