using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UtilsModule;
using System;
using System.Collections;
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
    public class VideoPlayerOnlyExample : MonoBehaviour
    {

        /// <summary>
        /// VIDEO_FILENAME
        /// </summary>
        public string VIDEO_FILENAME;

        /// <summary>
        /// The video player.
        /// </summary>
        VideoPlayer videoPlayer;

        /// <summary>
        /// The video path input field.
        /// </summary>
        public InputField videoPathInputField;

        /// <summary>
        /// The play button.
        /// </summary>
        public Button PlayButton;

        /// <summary>
        /// The stop button.
        /// </summary>
        public Button StopButton;

        // Use this for initialization
        void Start ()
        {

            videoPlayer = GetComponent<VideoPlayer>();

            videoPlayer.prepareCompleted += PrepareCompleted;
            videoPlayer.errorReceived += ErrorReceived;

            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = Application.streamingAssetsPath + "/" + VIDEO_FILENAME;
            videoPathInputField.text = videoPlayer.url;

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

            if (!videoPlayer.isPrepared)
            {

                videoPlayer.url = videoPathInputField.text;

                videoPlayer.Prepare();

                PlayButton.interactable = false;
                StopButton.interactable = true;
            }

        }

        /// <summary>
        /// Raises the stop button click event.
        /// </summary>
        public void OnStopButtonClick()
        {

            videoPlayer.Stop();

            PlayButton.interactable = true;
            StopButton.interactable = false;
        }

        void PrepareCompleted(VideoPlayer vp){

            Debug.Log("Video Url: " + vp.url);
            Debug.Log("width: " + vp.width + " height: " + vp.height);

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



            gameObject.GetComponent<Renderer>().material.mainTexture = vp.texture;

            // 読込が完了したら再生.
            videoPlayer.Play();
        }

        void ErrorReceived(VideoPlayer source, string message)
        {
            Debug.Log("ErrorReceived: " + message);

            PlayButton.interactable = true;
            StopButton.interactable = false;
        }

    }
}