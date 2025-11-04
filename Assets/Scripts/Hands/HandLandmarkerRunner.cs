using System.Collections;
using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;
using UnityEngine.Rendering;
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

        private void OnDestroy()
        {
            StopDetection();
        }

        public void StartDetection()
        {
            if (_isRunning) return;
            StartCoroutine(RunDetection());
        }

        public void StopDetection()
        {
            _isRunning = false;
            _textureFramePool?.Dispose();
            _textureFramePool = null;
            _taskApi = null;
        }

        private IEnumerator RunDetection()
        {
            Debug.Log("🟢 Preparing hand landmark model...");

            yield return AssetLoader.PrepareAssetAsync(config.ModelPath);

            var options = config.GetHandLandmarkerOptions(
                config.RunningMode == Tasks.Vision.Core.RunningMode.LIVE_STREAM
                    ? OnHandLandmarkDetectionOutput
                    : null
            );

            _taskApi = HandLandmarker.CreateFromOptions(options, GpuManager.GpuResources);

            // Wait for AR camera feed
            yield return new WaitUntil(() =>
                _arCameraBackground != null &&
                _arCameraBackground.material != null &&
                _arCameraBackground.material.GetTexture("_MainTex") != null
            );

            Debug.Log("✅ AR camera background ready.");

            var bgTexture = _arCameraBackground.material.GetTexture("_MainTex");
            _textureFramePool = new Experimental.TextureFramePool(
                bgTexture.width,
                bgTexture.height,
                TextureFormat.RGBA32,
                5
            );

            _isRunning = true;
            var waitForEndOfFrame = new WaitForEndOfFrame();

            while (_isRunning)
            {
                yield return waitForEndOfFrame;

                var frameTex = _arCameraBackground.material.GetTexture("_MainTex");
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
