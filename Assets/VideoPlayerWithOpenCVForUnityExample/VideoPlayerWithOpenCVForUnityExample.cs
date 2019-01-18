using UnityEngine;
using System.Collections;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;


#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using OpenCVForUnity;

using UnityEngine.Video;

namespace OpenCVForUnityExample
{
    /// <summary>
    /// VideoCapture example.
    /// </summary>
    public class VideoPlayerWithOpenCVForUnityExample : MonoBehaviour
    {
        /// <summary>
        /// The video player.
        /// </summary>
        VideoPlayer videoPlayer;

        /// <summary>
        /// The rgba mat.
        /// </summary>
        Mat rgbaMat;

        /// <summary>
        /// The colors.
        /// </summary>
        Color32[] colors;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The video texture.
        /// </summary>
        Texture2D videoTexture;
        
        // Use this for initialization
        void Start ()
        {
            videoPlayer = GetComponent<VideoPlayer> ();

            int width = (int)videoPlayer.clip.width;
            int height = (int)videoPlayer.clip.height;

            colors = new Color32[width * height];
            texture = new Texture2D (width, height, TextureFormat.RGBA32, false);
            rgbaMat = new Mat (height, width, CvType.CV_8UC4);

            videoTexture = new Texture2D (width, height, TextureFormat.RGBA32, false);

            videoPlayer.Play ();

            gameObject.GetComponent<Renderer> ().material.mainTexture = texture;
        }
            
        // Update is called once per frame
        void Update ()
        {
            if (videoPlayer.isPlaying && videoPlayer.texture != null) {
               
                Utils.textureToTexture2D (videoPlayer.texture, videoTexture);

//                Utils.texture2DToMat(videoTexture, rgbaMat);
                Utils.fastTexture2DToMat (videoTexture, rgbaMat);

                Imgproc.putText (rgbaMat, "VideoPlayer With OpenCV for Unity Example", new Point (100, rgbaMat.rows () / 2), Imgproc.FONT_HERSHEY_SIMPLEX, 1.5, new Scalar (255, 0, 0, 255), 5, Imgproc.LINE_AA, false);

                Imgproc.putText (rgbaMat, "width:" + rgbaMat.width () + " height:" + rgbaMat.height () + " frame:" + videoPlayer.frame, new Point (5, rgbaMat.rows () - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 1.5, new Scalar (255, 255, 255, 255), 5, Imgproc.LINE_AA, false);

//                Utils.matToTexture2D (rgbaMat, texture, colors);
                Utils.fastMatToTexture2D (rgbaMat, texture);
            }
        }

        void OnDestroy ()
        {

            if (rgbaMat != null)
                rgbaMat.Dispose ();
        }

        public void OnBackButton ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("OpenCVForUnityExample");
            #else
            Application.LoadLevel ("OpenCVForUnityExample");
            #endif
        }
    }
}