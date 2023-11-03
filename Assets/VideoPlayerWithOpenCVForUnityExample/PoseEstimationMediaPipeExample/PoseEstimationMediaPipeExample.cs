#if !UNITY_WSA_10_0

using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnityExample.DnnModel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using VideoPlayerWithOpenCVForUnity.UnityUtils.Helper;

namespace VideoPlayerWithOpenCVForUnityExample
{
    /// <summary>
    /// Pose Estimation MediaPipe Example
    /// An example of using OpenCV dnn module with Human Pose Estimation.
    /// Referring to https://github.com/opencv/opencv_zoo/tree/main/models/pose_estimation_mediapipe
    /// </summary>
    [RequireComponent(typeof(VideoPlayerToMatHelper))]
    public class PoseEstimationMediaPipeExample : MonoBehaviour
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
        /// The mask toggle.
        /// </summary>
        public Toggle maskToggle;

        /// <summary>
        /// The mask.
        /// </summary>
        public bool mask;

        /// <summary>
        /// The frameSkip toggle.
        /// </summary>
        public Toggle frameSkipToggle;

        /// <summary>
        /// The frameSkip.
        /// </summary>
        public bool frameSkip;

        [Header("TEST")]

        [TooltipAttribute("Path to test input image.")]
        public string testInputImage;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The webcam texture to mat helper.
        /// </summary>
        VideoPlayerToMatHelper videoPlayerToMatHelper;

        /// <summary>
        /// The image optimization helper.
        /// </summary>
        ImageOptimizationHelper imageOptimizationHelper;

        /// <summary>
        /// The bgr mat.
        /// </summary>
        Mat bgrMat;

        /// <summary>
        /// The person detector.
        /// </summary>
        MediaPipePersonDetector personDetector;

        /// <summary>
        /// The pose estimator.
        /// </summary>
        MediaPipePoseEstimator poseEstimator;

        /// <summary>
        /// The FPS monitor.
        /// </summary>
        FpsMonitor fpsMonitor;

        /// <summary>
        /// PERSON_DETECTION_MODEL_FILENAME
        /// </summary>
        protected static readonly string PERSON_DETECTION_MODEL_FILENAME = "OpenCVForUnity/dnn/person_detection_mediapipe_2023mar.onnx";

        /// <summary>
        /// The person detection model filepath.
        /// </summary>
        string person_detection_model_filepath;

        /// <summary>
        /// POSE_ESTIMATION_MODEL_FILENAME
        /// </summary>
        protected static readonly string POSE_ESTIMATION_MODEL_FILENAME = "OpenCVForUnity/dnn/pose_estimation_mediapipe_2023mar.onnx";

        /// <summary>
        /// The pose estimation model filepath.
        /// </summary>
        string pose_estimation_model_filepath;

        /// <summary>
        /// The persons.
        /// </summary>
        Mat persons;

        /// <summary>
        /// The results.
        /// </summary>
        List<Mat> results;

#if UNITY_WEBGL
        IEnumerator getFilePath_Coroutine;
#endif

        // Use this for initialization
        void Start()
        {
            fpsMonitor = GetComponent<FpsMonitor>();

            imageOptimizationHelper = gameObject.GetComponent<ImageOptimizationHelper>();
            videoPlayerToMatHelper = gameObject.GetComponent<VideoPlayerToMatHelper>();

            VideoPlayer videoPlayer = GetComponent<VideoPlayer>();
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = Application.streamingAssetsPath + "/" + VIDEO_FILENAME;
            videoPathInputField.text = videoPlayer.url;

            // Update GUI state
            maskToggle.isOn = mask;
            frameSkipToggle.isOn = frameSkip;

#if UNITY_WEBGL
            getFilePath_Coroutine = GetFilePath();
            StartCoroutine(getFilePath_Coroutine);
#else
            person_detection_model_filepath = Utils.getFilePath(PERSON_DETECTION_MODEL_FILENAME);
            pose_estimation_model_filepath = Utils.getFilePath(POSE_ESTIMATION_MODEL_FILENAME);
            Run();
#endif
        }

#if UNITY_WEBGL
        private IEnumerator GetFilePath()
        {
            var getFilePathAsync_0_Coroutine = Utils.getFilePathAsync(PERSON_DETECTION_MODEL_FILENAME, (result) =>
            {
                person_detection_model_filepath = result;
            });
            yield return getFilePathAsync_0_Coroutine;

            var getFilePathAsync_1_Coroutine = Utils.getFilePathAsync(POSE_ESTIMATION_MODEL_FILENAME, (result) =>
            {
                pose_estimation_model_filepath = result;
            });
            yield return getFilePathAsync_1_Coroutine;

            getFilePath_Coroutine = null;

            Run();
        }
#endif

        // Use this for initialization
        void Run()
        {
            //if true, The error log of the Native side OpenCV will be displayed on the Unity Editor Console.
            Utils.setDebugMode(true);


            if (string.IsNullOrEmpty(person_detection_model_filepath))
            {
                Debug.LogError(PERSON_DETECTION_MODEL_FILENAME + " is not loaded. Please read “StreamingAssets/OpenCVForUnity/dnn/setup_dnn_module.pdf” to make the necessary setup.");
            }
            else
            {
                personDetector = new MediaPipePersonDetector(person_detection_model_filepath, 0.3f, 0.6f, 5000);// # usually only one person has good performance
            }

            if (string.IsNullOrEmpty(pose_estimation_model_filepath))
            {
                Debug.LogError(POSE_ESTIMATION_MODEL_FILENAME + " is not loaded. Please read “StreamingAssets/OpenCVForUnity/dnn/setup_dnn_module.pdf” to make the necessary setup.");
            }
            else
            {
                poseEstimator = new MediaPipePoseEstimator(pose_estimation_model_filepath, 0.9f);
            }


            if (string.IsNullOrEmpty(testInputImage))
            {
                //videoPlayerToMatHelper.Initialize();
                OnPlayButtonClick();
            }
            else
            {
                /////////////////////
                // TEST

                var getFilePathAsync_0_Coroutine = Utils.getFilePathAsync("OpenCVForUnity/dnn/" + testInputImage, (result) =>
                {
                    string test_input_image_filepath = result;
                    if (string.IsNullOrEmpty(test_input_image_filepath)) Debug.Log("The file:" + testInputImage + " did not exist in the folder “Assets/StreamingAssets/OpenCVForUnity/dnn”.");

                    Mat img = Imgcodecs.imread(test_input_image_filepath);
                    if (img.empty())
                    {
                        img = new Mat(424, 640, CvType.CV_8UC3, new Scalar(0, 0, 0));
                        Imgproc.putText(img, testInputImage + " is not loaded.", new Point(5, img.rows() - 30), Imgproc.FONT_HERSHEY_SIMPLEX, 0.7, new Scalar(255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                        Imgproc.putText(img, "Please read console message.", new Point(5, img.rows() - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 0.7, new Scalar(255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                    }
                    else
                    {
                        TickMeter tm = new TickMeter();
                        tm.start();

                        Mat persons = personDetector.infer(img);

                        tm.stop();
                        Debug.Log("MediaPipePersonDetector Inference time (preprocess + infer + postprocess), ms: " + tm.getTimeMilli());

                        // Estimate the pose of each person
                        for (int i = 0; i < persons.rows(); ++i)
                        {
                            tm.reset();
                            tm.start();

                            // pose estimator inference
                            List<Mat> results = poseEstimator.infer(img, persons.row(i), mask);

                            tm.stop();
                            Debug.Log("MediaPipePoseEstimator Inference time (preprocess + infer + postprocess), ms: " + tm.getTimeMilli());
                            
                            if (!results[1].empty())
                                poseEstimator.visualize_mask(img, results[1], false);

                            poseEstimator.visualize(img, results[0], true, false);
                        }

                        //personDetector.visualize(img, persons, true, false);
                    }

                    gameObject.transform.localScale = new Vector3(img.width(), img.height(), 1);
                    float imageWidth = img.width();
                    float imageHeight = img.height();
                    float widthScale = (float)Screen.width / imageWidth;
                    float heightScale = (float)Screen.height / imageHeight;
                    if (widthScale < heightScale)
                    {
                        Camera.main.orthographicSize = (imageWidth * (float)Screen.height / (float)Screen.width) / 2;
                    }
                    else
                    {
                        Camera.main.orthographicSize = imageHeight / 2;
                    }
                    
                    Imgproc.cvtColor(img, img, Imgproc.COLOR_BGR2RGB);
                    Texture2D texture = new Texture2D(img.cols(), img.rows(), TextureFormat.RGB24, false);
                    Utils.matToTexture2D(img, texture);
                    gameObject.GetComponent<Renderer>().material.mainTexture = texture;
                });
                StartCoroutine(getFilePathAsync_0_Coroutine);

                /////////////////////
            }
        }

        /// <summary>
        /// Raises the webcam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized()
        {
            Debug.Log("OnWebCamTextureToMatHelperInitialized");

            Mat webCamTextureMat = videoPlayerToMatHelper.GetMat();

            texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);
            Utils.matToTexture2D(webCamTextureMat, texture);

            gameObject.GetComponent<Renderer>().material.mainTexture = texture;

            gameObject.transform.localScale = new Vector3(webCamTextureMat.cols(), webCamTextureMat.rows(), 1);
            Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            if (fpsMonitor != null)
            {
                fpsMonitor.Add("width", videoPlayerToMatHelper.GetWidth().ToString());
                fpsMonitor.Add("height", videoPlayerToMatHelper.GetHeight().ToString());
                fpsMonitor.Add("fps", videoPlayerToMatHelper.GetFPS().ToString());
                fpsMonitor.consoleText = null;
            }

            float width = webCamTextureMat.width();
            float height = webCamTextureMat.height();

            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale)
            {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            }
            else
            {
                Camera.main.orthographicSize = height / 2;
            }

            bgrMat = new Mat(webCamTextureMat.rows(), webCamTextureMat.cols(), CvType.CV_8UC3);

            persons = new Mat();
            results = new List<Mat>();
        }

        /// <summary>
        /// Raises the webcam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed()
        {
            Debug.Log("OnWebCamTextureToMatHelperDisposed");

            if (bgrMat != null)
                bgrMat.Dispose();
            if (persons != null)
                persons.Dispose();
            if (results != null)
            {
                foreach (var result in results)
                {
                    result.Dispose();
                }
            }

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
            if (fpsMonitor != null)
            {
                fpsMonitor.consoleText = "ErrorCode: " + errorCode;
            }
        }

        // Update is called once per frame
        void Update()
        {

            if (videoPlayerToMatHelper.IsPlaying() && videoPlayerToMatHelper.DidUpdateThisFrame())
            {

                Mat rgbaMat = videoPlayerToMatHelper.GetMat();

                if (personDetector == null || poseEstimator == null)
                {
                    Imgproc.putText(rgbaMat, "model file is not loaded.", new Point(5, rgbaMat.rows() - 30), Imgproc.FONT_HERSHEY_SIMPLEX, 0.7, new Scalar(255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                    Imgproc.putText(rgbaMat, "Please read console message.", new Point(5, rgbaMat.rows() - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 0.7, new Scalar(255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                }
                else
                {
                    Imgproc.cvtColor(rgbaMat, bgrMat, Imgproc.COLOR_RGBA2BGR);

                    if (!frameSkip || !imageOptimizationHelper.IsCurrentFrameSkipped())
                    {

                        //TickMeter tm = new TickMeter();
                        //tm.start();

                        persons = personDetector.infer(bgrMat);

                        //tm.stop();
                        //Debug.Log("MediaPipePersonDetector Inference time (preprocess + infer + postprocess), ms: " + tm.getTimeMilli());

                        // Estimate the pose of each person
                        for (int i = 0; i < persons.rows(); ++i)
                        {
                            //tm.reset();
                            //tm.start();

                            // pose estimator inference
                            results = poseEstimator.infer(bgrMat, persons.row(i), mask);

                            //tm.stop();
                            //Debug.Log("MediaPipePoseEstimator Inference time (preprocess + infer + postprocess), ms: " + tm.getTimeMilli());

                        }
                    }

                    // Estimate the pose of each person
                    for (int i = 0; i < persons.rows(); ++i)
                    {

                        if (!results[1].empty())
                            poseEstimator.visualize_mask(bgrMat, results[1], false);

                        poseEstimator.visualize(bgrMat, results[0], false, false);
                    }

                    //personDetector.visualize(bgrMat, persons, false, false);
                }

                Imgproc.cvtColor(bgrMat, rgbaMat, Imgproc.COLOR_BGR2RGBA);

                Utils.matToTexture2D(rgbaMat, texture);
            }

        }


        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy()
        {
            videoPlayerToMatHelper.Dispose();
            imageOptimizationHelper.Dispose();

            if (personDetector != null)
                personDetector.dispose();

            if (poseEstimator != null)
                poseEstimator.dispose();

            Utils.setDebugMode(false);

#if UNITY_WEBGL
            if (getFilePath_Coroutine != null)
            {
                StopCoroutine(getFilePath_Coroutine);
                ((IDisposable)getFilePath_Coroutine).Dispose();
            }
#endif
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
        /// Raises the mask toggle value changed event.
        /// </summary>
        public void OnMaskToggleValueChanged()
        {
            if (maskToggle.isOn != mask)
            {
                mask = maskToggle.isOn;
            }
        }

        /// <summary>
        /// Raises the frameskip toggle value changed event.
        /// </summary>
        public void OnFrameSkipToggleValueChanged()
        {
            if (frameSkipToggle.isOn != frameSkip)
            {
                frameSkip = frameSkipToggle.isOn;
            }
        }
    }
}

#endif