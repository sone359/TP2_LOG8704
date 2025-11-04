using System.Collections;
using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Mediapipe.Unity.Sample.HandLandmarkDetection
{
    public class HandLandmarkerRunner : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private ARCameraBackground _arCameraBackground;

        [Tooltip("Optional: can be removed if you don't need debug landmarks.")]
        [SerializeField] private HandLandmarkerResultAnnotationController _handLandmarkerResultAnnotationController;

        public readonly HandLandmarkDetectionConfig config = new HandLandmarkDetectionConfig();
        public event System.Action<IReadOnlyList<NormalizedLandmarks>> OnHandsDetected;

        private HandLandmarker _taskApi;
        private Experimental.TextureFramePool _textureFramePool;
        private bool _isRunning = false;
        private bool _isInitialized = false;

        private void OnDestroy()
        {
            StopDetection();
        }

        /// <summary>
        /// Loads model assets before starting detection.
        /// </summary>
        public IEnumerator Initialize()
        {
            if (_isInitialized) yield break;

            Debug.Log("🟢 Loading Hand Landmarker model...");
            yield return AssetLoader.PrepareAssetAsync(config.ModelPath);
            Debug.Log("✅ Model loaded successfully.");

            _isInitialized = true;
        }

        /// <summary>
        /// Safely starts detection. Will auto-initialize if needed.
        /// </summary>
        public void StartDetection()
        {
            if (_isRunning) return;

            if (!_isInitialized)
            {
                Debug.LogWarning("⚠️ HandLandmarkerRunner not initialized yet. Initializing now...");
                StartCoroutine(StartAfterInit());
            }
            else
            {
                StartCoroutine(RunDetection());
            }
        }

        private IEnumerator StartAfterInit()
        {
            yield return Initialize();
            yield return RunDetection();
        }

        public void StopDetection()
        {
            _isRunning = false;
            _textureFramePool?.Dispose();
            _textureFramePool = null;
            _taskApi = null;
            _isInitialized = false;
        }

        /// <summary>
        /// Safely get the AR camera texture, using the shader property that exists.
        /// </summary>
        private Texture GetCameraTexture()
        {
            if (_arCameraBackground == null || _arCameraBackground.material == null)
                return null;

            var mat = _arCameraBackground.material;

            // Use shader's actual property
            if (mat.HasProperty("_TextureSingle"))
            {
                var tex = mat.GetTexture("_TextureSingle");
                if (tex != null)
                    return tex;
            }

            // Fallback to mainTexture if exists
            if (mat.mainTexture != null)
                return mat.mainTexture;

            // Still no texture, return white placeholder
            return Texture2D.whiteTexture;
        }

        private IEnumerator RunDetection()
        {
            Debug.Log("🟢 Starting hand landmark detection...");

            // Wait until a usable camera texture exists
            yield return new WaitUntil(() => GetCameraTexture() != null);

            var bgTexture = GetCameraTexture();
            Debug.Log("✅ AR camera background ready.");

            _textureFramePool = new Experimental.TextureFramePool(
                bgTexture.width,
                bgTexture.height,
                TextureFormat.RGBA32,
                5
            );

            // Create the HandLandmarker task
            var options = config.GetHandLandmarkerOptions(
                config.RunningMode == Tasks.Vision.Core.RunningMode.LIVE_STREAM
                    ? OnHandLandmarkDetectionOutput
                    : null
            );

            _taskApi = HandLandmarker.CreateFromOptions(options, GpuManager.GpuResources);

            _isRunning = true;
            var waitForEndOfFrame = new WaitForEndOfFrame();

            while (_isRunning)
            {
                yield return waitForEndOfFrame;

                var frameTex = GetCameraTexture();
                if (frameTex == null) continue;

                if (!_textureFramePool.TryGetTextureFrame(out var textureFrame)) continue;

                textureFrame.ReadTextureOnGPU(frameTex, flipHorizontally: false, flipVertically: true);
                var image = textureFrame.BuildGPUImage(null);

                _taskApi.DetectAsync(image, GetCurrentTimestampMillisec(),
                    new Tasks.Vision.Core.ImageProcessingOptions(rotationDegrees: 0));
            }
        }

        private void OnHandLandmarkDetectionOutput(HandLandmarkerResult result, Image image, long timestamp)
        {
            if (_handLandmarkerResultAnnotationController != null)
                _handLandmarkerResultAnnotationController.DrawLater(result);

            if (result.handLandmarks != null && result.handLandmarks.Count > 0)
                OnHandsDetected?.Invoke(result.handLandmarks);
        }

        private long GetCurrentTimestampMillisec()
        {
            return (long)(Time.realtimeSinceStartup * 1000);
        }
    }
}