using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UtilsModule;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.Video;

namespace VideoPlayerWithOpenCVForUnity.UnityUtils.Helper
{
    /// <summary>
    /// VideoPlayer to mat helper.
    /// v 1.0.0
    /// </summary>
    [RequireComponent(typeof(VideoPlayer))]
    public class VideoPlayerToMatHelper : MonoBehaviour
    {

        /// <summary>
        /// Select the output color format.
        /// </summary>
        [SerializeField, FormerlySerializedAs("outputColorFormat"), TooltipAttribute("Select the output color format.")]
        protected ColorFormat _outputColorFormat = ColorFormat.RGBA;

        public virtual ColorFormat outputColorFormat
        {
            get { return _outputColorFormat; }
            set
            {
                if (_outputColorFormat != value)
                {
                    _outputColorFormat = value;
                    if (hasInitDone)
                        Initialize();
                }
            }
        }

        /// <summary>
        /// UnityEvent that is triggered when this instance is initialized.
        /// </summary>
        public UnityEvent onInitialized;

        /// <summary>
        /// UnityEvent that is triggered when this instance is disposed.
        /// </summary>
        public UnityEvent onDisposed;

        /// <summary>
        /// UnityEvent that is triggered when this instance is error Occurred.
        /// </summary>
        public ErrorUnityEvent onErrorOccurred;

        /// <summary>
        /// The videoPlayer
        /// </summary>
        protected VideoPlayer videoPlayer;

        /// <summary>
        /// 
        /// </summary>
        protected bool didUpdateThisFrame = false;

        /// <summary>
        /// The texture.
        /// </summary>
        protected Texture2D texture;

        /// <summary>
        /// The frame mat.
        /// </summary>
        protected Mat frameMat;

        /// <summary>
        /// The base mat.
        /// </summary>
        protected Mat baseMat;

        /// <summary>
        /// The frame index
        /// </summary>
        protected long frameIndex;

        /// <summary>
        /// The useAsyncGPUReadback
        /// </summary>
        protected bool useAsyncGPUReadback;

        /// <summary>
        /// The base color format.
        /// </summary>
        protected ColorFormat baseColorFormat = ColorFormat.RGBA;

        /// <summary>
        /// Indicates whether this instance is waiting for initialization to complete.
        /// </summary>
        protected bool isInitWaiting = false;

        /// <summary>
        /// Indicates whether this instance has been initialized.
        /// </summary>
        protected bool hasInitDone = false;

        /// <summary>
        /// The initialization coroutine.
        /// </summary>
        protected IEnumerator initCoroutine;

        public enum ColorFormat : int
        {
            GRAY = 0,
            RGB,
            BGR,
            RGBA,
            BGRA,
        }

        [Serializable]
        public class ErrorUnityEvent : UnityEvent<string>
        {

        }

        protected virtual void LateUpdate()
        {

            if (!hasInitDone)
                return;

            if (didUpdateThisFrame)
            {

                didUpdateThisFrame = false;
            }

        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        protected virtual void OnDestroy()
        {
            Dispose();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public virtual void Initialize()
        {

#if !OPENCV_DONT_USE_UNSAFE_CODE
            useAsyncGPUReadback = SystemInfo.supportsAsyncGPUReadback;
#else
            useAsyncGPUReadback = false;
#endif
            //useAsyncGPUReadback = false;
            Debug.Log("useAsyncGPUReadback: " + useAsyncGPUReadback);

            if (isInitWaiting)
            {
                CancelInitCoroutine();
                ReleaseResources();
            }

            if (onInitialized == null)
                onInitialized = new UnityEvent();
            if (onDisposed == null)
                onDisposed = new UnityEvent();
            if (onErrorOccurred == null)
                onErrorOccurred = new ErrorUnityEvent();

            initCoroutine = _Initialize();
            StartCoroutine(initCoroutine);

        }

        /// <summary>
        /// Initializes this instance by coroutine.
        /// </summary>
        protected virtual IEnumerator _Initialize()
        {
            if (hasInitDone)
            {
                ReleaseResources();

                if (onDisposed != null)
                    onDisposed.Invoke();
            }

            isInitWaiting = true;

            videoPlayer = GetComponent<VideoPlayer>();

            videoPlayer.sendFrameReadyEvents = true;
            videoPlayer.frameReady += FrameReady;
            videoPlayer.prepareCompleted += PrepareCompleted;
            videoPlayer.errorReceived += ErrorReceived;

            videoPlayer.Prepare();

            yield return null;
            
        }

        void PrepareCompleted(VideoPlayer vp)
        {

            //Debug.Log("Video Url: " + vp.url);
            //Debug.Log("width: " + vp.width + " height: " + vp.height);
            //Debug.Log("canSetSkipOnDrop: " + vp.canSetSkipOnDrop);

            int frameWidth = (int)vp.width;
            int frameHeight = (int)vp.height;

            if (!useAsyncGPUReadback)
            {
                texture = new Texture2D(frameWidth, frameHeight, TextureFormat.RGBA32, false);
            }
            baseMat = new Mat(frameHeight, frameWidth, CvType.CV_8UC4, new Scalar(0,0,0,255));

            if (baseColorFormat == outputColorFormat)
            {
                //frameMat = baseMat;
                frameMat = baseMat.clone();
            }
            else
            {
                frameMat = new Mat(baseMat.rows(), baseMat.cols(), CvType.CV_8UC(Channels(outputColorFormat)));
            }

            Debug.Log("VideoCaptrueToMatHelper:: " + " fileUrl:" + videoPlayer.url + " width:" + frameMat.width() + " height:" + frameMat.height() + " fps:" + videoPlayer.frameRate);

            isInitWaiting = false;
            hasInitDone = true;
            initCoroutine = null;

            if (onInitialized != null)
                onInitialized.Invoke();

            videoPlayer.Play();
        }

        void ErrorReceived(VideoPlayer source, string message)
        {
            Debug.Log("ErrorReceived: " + message);

            if (onErrorOccurred != null)
                onErrorOccurred.Invoke(message);
        }

        void FrameReady(VideoPlayer vp, long frameIndex)
        {
            //Debug.Log("FrameReady " + frameIndex);

            if (!useAsyncGPUReadback)
            {
                Utils.textureToTexture2D(vp.texture, texture);

                //                Utils.texture2DToMat(videoTexture, rgbaMat);
                Utils.fastTexture2DToMat(texture, baseMat);

                didUpdateThisFrame = true;
                this.frameIndex = frameIndex;
            }
            else
            {
                AsyncGPUReadback.Request(vp.texture, 0, TextureFormat.RGBA32, (request) => { OnCompleteReadback(request, frameIndex); });
            }
        }

        void OnCompleteReadback(AsyncGPUReadbackRequest request, long frameIndex)
        {
            //Debug.Log("OnCompleteReadback");

            if (request.hasError)
            {
                Debug.Log("GPU readback error detected. " + frameIndex);

            }
            else if (request.done)
            {
                //Debug.Log("Start GPU readback done. "+frameIndex);

                //Debug.Log("Thread.CurrentThread.ManagedThreadId " + Thread.CurrentThread.ManagedThreadId);

#if !OPENCV_DONT_USE_UNSAFE_CODE
                MatUtils.copyToMat(request.GetData<byte>(), baseMat);
#endif

                Core.flip(baseMat, baseMat, 0);

                didUpdateThisFrame = true;
                this.frameIndex = frameIndex;

                //Debug.Log("End GPU readback done. " + frameIndex);

            }
        }

        /// <summary>
        /// Indicates whether this instance has been initialized.
        /// </summary>
        /// <returns><c>true</c>, if this instance has been initialized, <c>false</c> otherwise.</returns>
        public virtual bool IsInitialized()
        {
            return hasInitDone;
        }

        /// <summary>
        /// Starts the video.
        /// </summary>
        public virtual void Play()
        {
            if (hasInitDone)
            {
                videoPlayer.Play();
            }
        }

        /// <summary>
        /// Pauses the video.
        /// </summary>
        public virtual void Pause()
        {
            if (hasInitDone)
                videoPlayer.Pause();
        }

        /// <summary>
        /// Stops the video.
        /// </summary>
        public virtual void Stop()
        {
            if (hasInitDone)
                videoPlayer.Stop();
        }

        /// <summary>
        /// Indicates whether the video is currently playing.
        /// </summary>
        /// <returns><c>true</c>, if the video is playing, <c>false</c> otherwise.</returns>
        public virtual bool IsPlaying()
        {
            return hasInitDone ? videoPlayer.isPlaying : false;
        }

        /// <summary>
        /// Indicates whether the video is paused.
        /// </summary>
        /// <returns><c>true</c>, if the video is playing, <c>false</c> otherwise.</returns>
        public virtual bool IsPaused()
        {
            return hasInitDone ? videoPlayer.isPaused : false;
        }

        /// <summary>
        /// Returns the video width.
        /// </summary>
        /// <returns>The video width.</returns>
        public virtual int GetWidth()
        {
            if (!hasInitDone)
                return -1;
            return frameMat.width();
        }

        /// <summary>
        /// Returns the video height.
        /// </summary>
        /// <returns>The video height.</returns>
        public virtual int GetHeight()
        {
            if (!hasInitDone)
                return -1;
            return frameMat.height();
        }

        /// <summary>
        /// Returns the video framerate.
        /// </summary>
        /// <returns>The video framerate.</returns>
        public virtual float GetFPS()
        {
            return hasInitDone ? videoPlayer.frameRate : -1f;
        }

        /// <summary>
        /// Returns the video framerate.
        /// </summary>
        /// <returns>The video framerate.</returns>
        public virtual float GetFrameIndex()
        {
            return hasInitDone ? frameIndex : 0;
        }

        /// <summary>
        /// Returns the video base color format.
        /// </summary>
        /// <returns>The video base color format.</returns>
        public virtual ColorFormat GetBaseColorFormat()
        {
            return baseColorFormat;
        }

        /// <summary>
        /// Returns the VideoCapture instance.
        /// </summary>
        /// <returns>The VideoCapture instance.</returns>
        public virtual VideoPlayer GetVideoPlayer()
        {
            return hasInitDone ? videoPlayer : null;
        }

        /// <summary>
        /// Indicates whether the video buffer of the frame has been updated.
        /// </summary>
        /// <returns><c>true</c>, if the video buffer has been updated <c>false</c> otherwise.</returns>
        public virtual bool DidUpdateThisFrame()
        {
            if (!hasInitDone)
                return false;

            return didUpdateThisFrame;
        }

        /// <summary>
        /// Gets the mat of the current frame.
        /// The Mat object's type is 'CV_8UC4' or 'CV_8UC3' or 'CV_8UC1' (ColorFormat is determined by the outputColorFormat setting).
        /// </summary>
        /// <returns>The mat of the current frame.</returns>
        public virtual Mat GetMat()
        {
            if (!hasInitDone)
            {
                return frameMat;
            }

            if (baseColorFormat == outputColorFormat)
            {
                baseMat.copyTo(frameMat);
            }
            else
            {
                Imgproc.cvtColor(baseMat, frameMat, ColorConversionCodes(baseColorFormat, outputColorFormat));
            }

            return frameMat;
        }

        protected virtual int Channels(ColorFormat type)
        {
            switch (type)
            {
                case ColorFormat.GRAY:
                    return 1;
                case ColorFormat.RGB:
                case ColorFormat.BGR:
                    return 3;
                case ColorFormat.RGBA:
                case ColorFormat.BGRA:
                    return 4;
                default:
                    return 4;
            }
        }
        protected virtual int ColorConversionCodes(ColorFormat srcType, ColorFormat dstType)
        {
            if (srcType == ColorFormat.GRAY)
            {
                if (dstType == ColorFormat.RGB) return Imgproc.COLOR_GRAY2RGB;
                else if (dstType == ColorFormat.BGR) return Imgproc.COLOR_GRAY2BGR;
                else if (dstType == ColorFormat.RGBA) return Imgproc.COLOR_GRAY2RGBA;
                else if (dstType == ColorFormat.BGRA) return Imgproc.COLOR_GRAY2BGRA;
            }
            else if (srcType == ColorFormat.RGB)
            {
                if (dstType == ColorFormat.GRAY) return Imgproc.COLOR_RGB2GRAY;
                else if (dstType == ColorFormat.BGR) return Imgproc.COLOR_RGB2BGR;
                else if (dstType == ColorFormat.RGBA) return Imgproc.COLOR_RGB2RGBA;
                else if (dstType == ColorFormat.BGRA) return Imgproc.COLOR_RGB2BGRA;
            }
            else if (srcType == ColorFormat.BGR)
            {
                if (dstType == ColorFormat.GRAY) return Imgproc.COLOR_BGR2GRAY;
                else if (dstType == ColorFormat.RGB) return Imgproc.COLOR_BGR2RGB;
                else if (dstType == ColorFormat.RGBA) return Imgproc.COLOR_BGR2RGBA;
                else if (dstType == ColorFormat.BGRA) return Imgproc.COLOR_BGR2BGRA;
            }
            else if (srcType == ColorFormat.RGBA)
            {
                if (dstType == ColorFormat.GRAY) return Imgproc.COLOR_RGBA2GRAY;
                else if (dstType == ColorFormat.RGB) return Imgproc.COLOR_RGBA2RGB;
                else if (dstType == ColorFormat.BGR) return Imgproc.COLOR_RGBA2BGR;
                else if (dstType == ColorFormat.BGRA) return Imgproc.COLOR_RGBA2BGRA;
            }
            else if (srcType == ColorFormat.BGRA)
            {
                if (dstType == ColorFormat.GRAY) return Imgproc.COLOR_BGRA2GRAY;
                else if (dstType == ColorFormat.RGB) return Imgproc.COLOR_BGRA2RGB;
                else if (dstType == ColorFormat.BGR) return Imgproc.COLOR_BGRA2BGR;
                else if (dstType == ColorFormat.RGBA) return Imgproc.COLOR_BGRA2RGBA;
            }

            return -1;
        }

        /// <summary>
        /// Cancel Init Coroutine.
        /// </summary>
        protected virtual void CancelInitCoroutine()
        {

            if (initCoroutine != null)
            {
                StopCoroutine(initCoroutine);
                ((IDisposable)initCoroutine).Dispose();
                initCoroutine = null;
            }
        }

        /// <summary>
        /// To release the resources.
        /// </summary>
        protected virtual void ReleaseResources()
        {
            isInitWaiting = false;
            hasInitDone = false;

            didUpdateThisFrame = false;

            frameIndex = 0;

            if (videoPlayer != null)
            {
                videoPlayer.sendFrameReadyEvents = false;
                videoPlayer.frameReady -= FrameReady;
                videoPlayer.prepareCompleted -= PrepareCompleted;
                videoPlayer.errorReceived -= ErrorReceived;

                videoPlayer.Stop();
                //WebCamTexture.Destroy(videoPlayer);
                videoPlayer = null;
            }
            if (texture != null)
            {
                Texture2D.Destroy(texture);
                texture = null;
            }
            if (frameMat != null)
            {
                frameMat.Dispose();
                frameMat = null;
            }
            if (baseMat != null)
            {
                baseMat.Dispose();
                baseMat = null;
            }

        }

        /// <summary>
        /// Releases all resource used by the <see cref="VideoCaptrueToMatHelper"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="VideoCaptrueToMatHelper"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="VideoCaptrueToMatHelper"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the <see cref="VideoCaptrueToMatHelper"/> so
        /// the garbage collector can reclaim the memory that the <see cref="VideoCaptrueToMatHelper"/> was occupying.</remarks>
        public virtual void Dispose()
        {
            if (useAsyncGPUReadback)
            {
                AsyncGPUReadback.WaitAllRequests();
            }

            if (isInitWaiting)
            {
                CancelInitCoroutine();
                ReleaseResources();
            }
            else if (hasInitDone)
            {
                ReleaseResources();

                if (onDisposed != null)
                    onDisposed.Invoke();
            }
        }
    }
}