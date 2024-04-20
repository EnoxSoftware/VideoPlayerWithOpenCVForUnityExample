using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UtilsModule;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace VideoPlayerWithOpenCVForUnityExample
{
    /// <summary>
    /// VideoPlayerWithOpenCVForUnityAsyncExample
    /// </summary>
    public class VideoPlayerWithOpenCVForUnityAsyncExample : MonoBehaviour
    {

        /// <summary>
        /// VIDEO_FILENAME
        /// </summary>
        public string VIDEO_FILENAME;

        /// <summary>
        /// The video path input field.
        /// </summary>
        public InputField videoPathInputField;

        /// <summary>
        /// The play button.
        /// </summary>
        public Button PlayButton;

        /// <summary>
        /// The pause button.
        /// </summary>
        public Button PauseButton;

        /// <summary>
        /// The stop button.
        /// </summary>
        public Button StopButton;

        /// <summary>
        /// Determines if applies the comic filter.
        /// </summary>
        public bool applyComicFilter;

        /// <summary>
        /// The apply comic filter toggle.
        /// </summary>
        public Toggle applyComicFilterToggle;

        /// <summary>
        /// The video player.
        /// </summary>
        VideoPlayer videoPlayer;

        /// <summary>
        /// The rgba mat.
        /// </summary>
        Mat rgbaMat;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// comicFilter
        /// </summary>
        ComicFilter comicFilter;

        /// <summary>
        /// The FPS monitor.
        /// </summary>
        FpsMonitor fpsMonitor;


        // Use this for initialization
        void Start ()
        {

            fpsMonitor = GetComponent<FpsMonitor>();

#if !OPENCV_DONT_USE_UNSAFE_CODE
            if (!SystemInfo.supportsAsyncGPUReadback)
            {
                Debug.Log("Error : SystemInfo.supportsAsyncGPUReadback is false.");
                fpsMonitor.consoleText = "Error : SystemInfo.supportsAsyncGPUReadback is false.";
                PlayButton.interactable = false;
                StopButton.interactable = false;
                videoPathInputField.interactable = false;
                applyComicFilterToggle.interactable = false;
                return;
            }

            videoPlayer = GetComponent<VideoPlayer>();

            videoPlayer.sendFrameReadyEvents = true;
            videoPlayer.frameReady += FrameReady;
            videoPlayer.prepareCompleted += PrepareCompleted;
            videoPlayer.errorReceived += ErrorReceived;

            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = Application.streamingAssetsPath + "/" + VIDEO_FILENAME;
            videoPathInputField.text = videoPlayer.url;

            comicFilter = new ComicFilter();
            applyComicFilterToggle.isOn = applyComicFilter;

            OnPlayButtonClick();
#else
            Debug.Log("Error : Allow 'unsafe' Code is disable. Please enable Allow 'unsafe' Code. [MenuItem]->[Tools]->[OpenCV for Unity]->[Open Setup Tools]->[Enable Use Unsafe Code]");
            fpsMonitor.consoleText = "Error : Allow 'unsafe' Code is disable. Please enable Allow 'unsafe' Code. [MenuItem]->[Tools]->[OpenCV for Unity]->[Open Setup Tools]->[Enable Use Unsafe Code]";
            PlayButton.interactable = false;
            StopButton.interactable = false;
            videoPathInputField.interactable = false;
            applyComicFilterToggle.interactable = false;
            return;
#endif
        }
            
        // Update is called once per frame
        void Update ()
        {

            if (videoPlayer == null) return;

            if (videoPlayer.isPlaying && videoPlayer.texture != null) {

                //Debug.Log("Update Frame " + videoPlayer.frame);
            }
        }

        void OnDestroy ()
        {

            AsyncGPUReadback.WaitAllRequests();

            videoPlayer.sendFrameReadyEvents = false;
            videoPlayer.frameReady -= FrameReady;
            videoPlayer.prepareCompleted -= PrepareCompleted;
            videoPlayer.errorReceived -= ErrorReceived;

            if (rgbaMat != null)
                rgbaMat.Dispose ();

            if (comicFilter != null)
                comicFilter.Dispose();
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick ()
        {

            SceneManager.LoadScene ("VideoPlayerWithOpenCVForUnityExample");

        }

        /// <summary>
        /// Raises the play button click event.
        /// </summary>
        public void OnPlayButtonClick()
        {
            if (videoPlayer.isPaused)
            {
                videoPlayer.Play();
            }
            else
            {
                AsyncGPUReadback.WaitAllRequests();

                if (rgbaMat != null)
                    rgbaMat.Dispose();

                videoPlayer.url = videoPathInputField.text;

                videoPlayer.Prepare();

                PlayButton.interactable = false;
                PauseButton.interactable = true;
                StopButton.interactable = true;
            }
        }

        /// <summary>
        /// Raises the pause button click event.
        /// </summary>
        public void OnPauseButtonClick()
        {
            videoPlayer.Pause();

            PlayButton.interactable = true;
            PauseButton.interactable = false;
            StopButton.interactable = true;
        }

        /// <summary>
        /// Raises the stop button click event.
        /// </summary>
        public void OnStopButtonClick()
        {
            videoPlayer.Stop();

            PlayButton.interactable = true;
            PauseButton.interactable = false;
            StopButton.interactable = false;
        }

        /// <summary>
        /// Raises the apply comic filter toggle value changed event.
        /// </summary>
        public void OnApplyComicFilterToggleValueChanged()
        {
            if (applyComicFilter != applyComicFilterToggle.isOn)
            {
                applyComicFilter = applyComicFilterToggle.isOn;
            }
        }

        void PrepareCompleted(VideoPlayer vp){

            Debug.Log("Video Url: " + vp.url);
            Debug.Log("width: " + vp.width + " height: " + vp.height);

            if (fpsMonitor != null)
            {
                fpsMonitor.Add("width", vp.width.ToString());
                fpsMonitor.Add("height", vp.height.ToString());
                fpsMonitor.Add("fps", vp.frameRate.ToString());
                fpsMonitor.consoleText = null;
            }

            int frameWidth = (int)vp.width;
            int frameHeight = (int)vp.height;       

            gameObject.transform.localScale = new Vector3((float)frameWidth, (float)frameHeight, 1);
            float widthScale = (float)Screen.width / (float)frameWidth;
            float heightScale = (float)Screen.height / (float)frameHeight;
            if (widthScale < heightScale)
            {
                Camera.main.orthographicSize = ((float)frameWidth * (float)Screen.height / (float)Screen.width) / 2;
            }
            else
            {
                Camera.main.orthographicSize = (float)frameHeight / 2;
            }

            texture = new Texture2D(frameWidth, frameHeight, TextureFormat.RGBA32, false);
            rgbaMat = new Mat(frameHeight, frameWidth, CvType.CV_8UC4);

            gameObject.GetComponent<Renderer>().material.mainTexture = texture;

            videoPlayer.Play();
        }

        void ErrorReceived(VideoPlayer source, string message)
        {
            Debug.Log("ErrorReceived: " + message);

            PlayButton.interactable = true;
            PauseButton.interactable = false;
            StopButton.interactable = false;
            if (fpsMonitor != null)
            {
                fpsMonitor.consoleText = "ErrorCode: " + message;
            }
        }

        void FrameReady(VideoPlayer vp, long frameIndex)
        {
            //Debug.Log("FrameReady " + frameIndex);

            AsyncGPUReadback.Request(vp.texture, 0, TextureFormat.RGBA32, (request) => { OnCompleteReadback(request, frameIndex); });
        }

        void OnCompleteReadback(AsyncGPUReadbackRequest request, long frameIndex)
        {

            if (request.hasError)
            {
                Debug.Log("GPU readback error detected. "+frameIndex);

            }
            else if (request.done)
            {
                //Debug.Log("Start GPU readback done. "+frameIndex);

                //Debug.Log("Thread.CurrentThread.ManagedThreadId " + Thread.CurrentThread.ManagedThreadId);

#if !OPENCV_DONT_USE_UNSAFE_CODE
                MatUtils.copyToMat(request.GetData<byte>(), rgbaMat);
#endif

                Core.flip(rgbaMat, rgbaMat, 0);

                if (applyComicFilter)
                {
                    comicFilter.Process(rgbaMat, rgbaMat);

                    Imgproc.putText(rgbaMat, "VideoPlayer With OpenCVForUnity Async Example", new Point(5, rgbaMat.rows() - 65), Imgproc.FONT_HERSHEY_SIMPLEX, 1.5, new Scalar(255, 0, 0, 255), 5, Imgproc.LINE_AA, false);

                    Imgproc.putText(rgbaMat, "width:" + rgbaMat.width() + " height:" + rgbaMat.height() + " frame:" + frameIndex, new Point(5, rgbaMat.rows() - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 1.5, new Scalar(255, 0, 0, 255), 5, Imgproc.LINE_AA, false);
                }

                Utils.fastMatToTexture2D(rgbaMat, texture);

                //Debug.Log("End GPU readback done. " + frameIndex);

            }          
        }
    }
}