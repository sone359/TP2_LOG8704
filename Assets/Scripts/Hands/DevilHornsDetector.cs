using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Tasks.Components.Containers;
using System.Collections;

namespace Mediapipe.Unity.Sample.HandLandmarkDetection
{
    public class DevilHornsDetector : MonoBehaviour
    {
        [SerializeField] private HandLandmarkerRunner handLandmarkerRunner;

        [Header("Audio")]
        [SerializeField] private AudioClip moooohClip; // assign your sound in inspector
        private AudioSource audioSource;

        void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        private IEnumerator Start()
        {
            yield return handLandmarkerRunner.Initialize();
            handLandmarkerRunner.StartDetection();
            
        }
        

        void OnDisable()
        {
            if (handLandmarkerRunner != null)
            {
                handLandmarkerRunner.OnHandsDetected -= OnHandsDetected;
            }
        }

        private void OnHandsDetected(IReadOnlyList<NormalizedLandmarks> hands)
        {
            if (hands == null || hands.Count == 0) return;

            var hand = hands[0].landmarks;
            if (hand == null || hand.Count < 21) return;

            bool indexUp = hand[8].y < hand[6].y;
            bool pinkyUp = hand[20].y < hand[18].y;
            bool middleDown = hand[12].y > hand[10].y;
            bool ringDown = hand[16].y > hand[14].y;

            if (indexUp && pinkyUp && middleDown && ringDown)
            {
                // Play the Moooh sound
                if (moooohClip != null && audioSource != null)
                {
                    audioSource.Stop();
                    audioSource.clip = moooohClip;
                    audioSource.Play();
                }
            }
        }
    }
}
