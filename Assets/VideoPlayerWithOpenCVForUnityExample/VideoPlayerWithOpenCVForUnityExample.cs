using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ARFoundationWithOpenCVForUnityExample
{

    public class VideoPlayerWithOpenCVForUnityExample : MonoBehaviour
    {

        [Header("UI")]
        public Text exampleTitle;
        public Text versionInfo;
        public ScrollRect scrollRect;
        private static float verticalNormalizedPosition = 1f;

        void Awake()
        {
            //QualitySettings.vSyncCount = 0;
            //Application.targetFrameRate = 60;
        }

        IEnumerator Start()
        {

            exampleTitle.text = "VideoPlayerWithOpenCVForUnity Example " + Application.version;

            versionInfo.text = OpenCVForUnity.CoreModule.Core.NATIVE_LIBRARY_NAME + " " + OpenCVForUnity.UnityUtils.Utils.getVersion() + " (" + OpenCVForUnity.CoreModule.Core.VERSION + ")";
            versionInfo.text += " / UnityEditor " + Application.unityVersion;
            versionInfo.text += " / ";
#if UNITY_EDITOR
            versionInfo.text += "Editor";
#elif UNITY_STANDALONE_WIN
            versionInfo.text += "Windows";
#elif UNITY_STANDALONE_OSX
            versionInfo.text += "Mac OSX";
#elif UNITY_STANDALONE_LINUX
            versionInfo.text += "Linux";
#elif UNITY_ANDROID
            versionInfo.text += "Android";
#elif UNITY_IOS
            versionInfo.text += "iOS";
#elif UNITY_WSA
            versionInfo.text += "WSA";
#elif UNITY_WEBGL
            versionInfo.text += "WebGL";
#endif
            versionInfo.text += " ";
#if ENABLE_MONO
            versionInfo.text += "Mono";
#elif ENABLE_IL2CPP
            versionInfo.text += "IL2CPP";
#elif ENABLE_DOTNET
            versionInfo.text += ".NET";
#endif

            scrollRect.verticalNormalizedPosition = verticalNormalizedPosition;


            yield break;
        }

        public void OnScrollRectValueChanged()
        {
            verticalNormalizedPosition = scrollRect.verticalNormalizedPosition;
        }

        public void OnShowSystemInfoButtonClick()
        {
            SceneManager.LoadScene("ShowSystemInfo");
        }

        public void OnShowLicenseButtonClick()
        {
            SceneManager.LoadScene("ShowLicense");
        }

        public void OnVideoPlayerOnlyExampleButtonClick()
        {
            SceneManager.LoadScene("VideoPlayerOnlyExample");
        }

        public void OnVideoPlayerWithOpenCVForUnitySyncExampleButtonClick()
        {
            SceneManager.LoadScene("VideoPlayerWithOpenCVForUnitySyncExample");
        }
        public void OnVideoPlayerWithOpenCVForUnityAsyncExampleButtonClick()
        {
            SceneManager.LoadScene("VideoPlayerWithOpenCVForUnityAsyncExample");
        }
        public void OnVideoPlayerToMatHelperExampleButtonClick()
        {
            SceneManager.LoadScene("VideoPlayerToMatHelperExample");
        }

        public void OnFaceDetectionYuNetV2ExampleButtonClick()
        {
            SceneManager.LoadScene("FaceDetectionYuNetV2Example");
        }

        public void OnObjectDetectionYOLOXExampleButtonClick()
        {
            SceneManager.LoadScene("ObjectDetectionYOLOXExample");
        }

        public void OnHandPoseEstimationMediaPipeExampleButtonClick()
        {
            SceneManager.LoadScene("HandPoseEstimationMediaPipeExample");
        }

        public void OnPoseEstimationMediaPipeExampleButtonClick()
        {
            SceneManager.LoadScene("PoseEstimationMediaPipeExample");
        }

        public void OnVideoCaptureTrackingExampleButtonClick()
        {
            SceneManager.LoadScene("VideoCaptureTrackingExample");
        }

        public void OnVideoPlayerTrackingExampleButtonClick()
        {
            SceneManager.LoadScene("VideoPlayerTrackingExample");
        }
    }
}