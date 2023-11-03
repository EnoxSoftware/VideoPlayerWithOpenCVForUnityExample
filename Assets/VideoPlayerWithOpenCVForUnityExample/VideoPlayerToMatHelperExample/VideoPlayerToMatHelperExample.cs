using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using VideoPlayerWithOpenCVForUnity.UnityUtils.Helper;

namespace VideoPlayerWithOpenCVForUnityExample
{
    /// <summary>
    /// VideoPlayer To Mat Helper Example
    /// </summary>
    [RequireComponent(typeof(VideoPlayerToMatHelper))]
    public class VideoPlayerToMatHelperExample : MonoBehaviour
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
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The videoplayer to mat helper.
        /// </summary>
        VideoPlayerToMatHelper videoPlayerToMatHelper;

        /// <summary>
        /// comicFilter
        /// </summary>
        ComicFilter comicFilter;

        /// <summary>
        /// The FPS monitor.
        /// </summary>
        FpsMonitor fpsMonitor;

        // Use this for initialization
        void Start()
        {
            fpsMonitor = GetComponent<FpsMonitor>();

            // Get the WebCamTextureToMatHelper component attached to the current game object
            videoPlayerToMatHelper = gameObject.GetComponent<VideoPlayerToMatHelper>();
            videoPlayerToMatHelper.outputColorFormat = VideoPlayerToMatHelper.ColorFormat.RGBA;

            VideoPlayer videoPlayer = GetComponent<VideoPlayer>();
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = Application.streamingAssetsPath + "/" + VIDEO_FILENAME;
            videoPathInputField.text = videoPlayer.url;

            comicFilter = new ComicFilter();
            applyComicFilterToggle.isOn = applyComicFilter;

            OnPlayButtonClick();

        }

        /// <summary>
        /// Raises the webcam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized()
        {
            Debug.Log("OnWebCamTextureToMatHelperInitialized");

            // Retrieve the current frame from the WebCamTextureToMatHelper as a Mat object
            Mat webCamTextureMat = videoPlayerToMatHelper.GetMat();

            // Create a new Texture2D with the same dimensions as the Mat and RGBA32 color format
            texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);

            // Convert the Mat to a Texture2D, effectively transferring the image data
            Utils.matToTexture2D(webCamTextureMat, texture);

            // Set the Texture2D as the main texture of the Renderer component attached to the game object
            gameObject.GetComponent<Renderer>().material.mainTexture = texture;

            // Adjust the scale of the game object to match the dimensions of the texture
            gameObject.transform.localScale = new Vector3(webCamTextureMat.cols(), webCamTextureMat.rows(), 1);
            Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            if (fpsMonitor != null)
            {
                fpsMonitor.Add("width", videoPlayerToMatHelper.GetWidth().ToString());
                fpsMonitor.Add("height", videoPlayerToMatHelper.GetHeight().ToString());
                fpsMonitor.Add("fps", videoPlayerToMatHelper.GetFPS().ToString());
                fpsMonitor.consoleText = null;
            }

            // Get the width and height of the webCamTextureMat
            float width = webCamTextureMat.width();
            float height = webCamTextureMat.height();

            // Calculate the scale factors for width and height based on the screen dimensions
            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;

            // Adjust the orthographic size of the main Camera to fit the aspect ratio of the image
            if (widthScale < heightScale)
            {
                // If the width scale is smaller, adjust the orthographic size based on width and screen height
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            }
            else
            {
                // If the height scale is smaller or equal, adjust the orthographic size based on height
                Camera.main.orthographicSize = height / 2;
            }
        }

        /// <summary>
        /// Raises the webcam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed()
        {
            Debug.Log("OnWebCamTextureToMatHelperDisposed");

            // Destroy the texture and set it to null
            if (texture != null)
            {
                Texture2D.Destroy(texture);
                texture = null;
            }
        }

        /// <summary>
        /// Raises the webcam texture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred(string errorCode)
        {
            Debug.Log("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);

            PlayButton.interactable = true;
            PauseButton.interactable = false;
            StopButton.interactable = false;
            applyComicFilterToggle.interactable = false;
            if (fpsMonitor != null)
            {
                fpsMonitor.consoleText = "ErrorCode: " + errorCode;
            }
        }

        // Update is called once per frame
        void Update()
        {
            // Check if the web camera is playing and if a new frame was updated
            if (videoPlayerToMatHelper.IsPlaying() && videoPlayerToMatHelper.DidUpdateThisFrame())
            {
                //Debug.Log("Update "+webCamTextureToMatHelper.GetFrameIndex());

                // Retrieve the current frame as a Mat object
                Mat rgbaMat = videoPlayerToMatHelper.GetMat();

                if (applyComicFilter)
                {
                    comicFilter.Process(rgbaMat, rgbaMat);

                    Imgproc.putText(rgbaMat, "VideoPlayer To Mat Helper Example", new Point(5, rgbaMat.rows() - 65), Imgproc.FONT_HERSHEY_SIMPLEX, 1.5, new Scalar(255, 0, 0, 255), 5, Imgproc.LINE_AA, false);

                    Imgproc.putText(rgbaMat, "width:" + rgbaMat.width() + " height:" + rgbaMat.height() + " frame:" + videoPlayerToMatHelper.GetFrameIndex(), new Point(5, rgbaMat.rows() - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 1.5, new Scalar(255, 0, 0, 255), 5, Imgproc.LINE_AA, false);
                }

                Utils.fastMatToTexture2D(rgbaMat, texture);
            }
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy()
        {
            // Dispose of the webCamTextureToMatHelper object and release any resources held by it.
            videoPlayerToMatHelper.Dispose();

            if (comicFilter != null)
                comicFilter.Dispose();
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick()
        {
            SceneManager.LoadScene("VideoPlayerWithOpenCVForUnityExample");
        }

        /// <summary>
        /// Raises the play button click event.
        /// </summary>
        public void OnPlayButtonClick()
        {
            if (videoPlayerToMatHelper.IsPaused())
            {
                videoPlayerToMatHelper.Play();
            }
            else
            {
                VideoPlayer videoPlayer = GetComponent<VideoPlayer>();
                videoPlayer.url = videoPathInputField.text;

                videoPlayerToMatHelper.Initialize();
            }

            PlayButton.interactable = false;
            PauseButton.interactable = true;
            StopButton.interactable = true;
        }

        /// <summary>
        /// Raises the pause button click event.
        /// </summary>
        public void OnPauseButtonClick()
        {
            videoPlayerToMatHelper.Pause();

            PlayButton.interactable = true;
            PauseButton.interactable = false;
            StopButton.interactable = true;
        }

        /// <summary>
        /// Raises the stop button click event.
        /// </summary>
        public void OnStopButtonClick()
        {
            videoPlayerToMatHelper.Stop();

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
    }
}