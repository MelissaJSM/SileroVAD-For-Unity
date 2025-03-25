using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

namespace VadDotNet
{
    public class SileroVadRunner : MonoBehaviour
    {
        [Header("ONNX Model Settings")]
        public string onnxModelFileName = "silero_vad.onnx";
        public int sampleRate = 16000;
        public float threshold = 0.5f;
        public int minSpeechDurationMs = 250;
        public float minSilenceDurationMs = 100;
        public float maxSpeechDurationSeconds = float.PositiveInfinity;
        public int speechPadMs = 30;

        [Header("Audio Settings")]
        public string audioFileName = "path_to_audio_file.wav";
        // Window length in seconds for microphone real-time analysis
        public float micAnalysisWindowSeconds = 1.0f;
        public bool isEcho = false;

        [Header("Visual Settings")]
        public Renderer targetRenderer; // Render for the object you want to color

        private SileroVadDetector vadDetector;
        private List<SileroSpeechSegment> speechSegments;
        private AudioSource audioSource;
        private string modelPath;

        // Microphone mode related variables
        private bool isMicModeActive = false;
        private string micDevice;

        private void Start()
        {
            modelPath = Path.Combine(Application.streamingAssetsPath, onnxModelFileName);
            vadDetector = new SileroVadDetector(modelPath, threshold, sampleRate, minSpeechDurationMs, maxSpeechDurationSeconds, (int)minSilenceDurationMs, speechPadMs);
        }

        // Press button to call: existing function to call WAV file and detect voice
        public void StartMusic()
        {
            string audioPath = Path.Combine(Application.streamingAssetsPath, audioFileName);
            audioSource = gameObject.AddComponent<AudioSource>();
            StartCoroutine(ProcessAudioClip(audioPath));
        }

        IEnumerator ProcessAudioClip(string audioPath)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + audioPath, AudioType.WAV))
            {
                yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                if (www.result != UnityWebRequest.Result.Success)
#else
                if (www.isNetworkError || www.isHttpError)
#endif
                {
                    Debug.LogError("Error loading audio clip: " + www.error);
                }
                else
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    Debug.Log("Audio clip loaded. Processing...");

                    speechSegments = vadDetector.GetSpeechSegmentListFromAudioClip(clip);

                    // 오디오 재생
                    audioSource.clip = clip;
                    audioSource.Play();
                }
            }
        }

        // Press button to call: enable live voice detection via microphone
        public void StartMicrophone()
        {
            if (Microphone.devices.Length == 0)
            {
                Debug.LogError("No microphone devices found.");
                return;
            }
            // Select the first available microphone
            micDevice = Microphone.devices[0];

            audioSource = gameObject.AddComponent<AudioSource>();
            // Start recording the microphone by creating a 10-second audio clip with loop:true
            audioSource.clip = Microphone.Start(micDevice, true, 10, sampleRate);

            // Wait for recording to begin
            while (!(Microphone.GetPosition(micDevice) > 0)) { }

            if (isEcho)
            { 
                audioSource.Play();
            }
            
            isMicModeActive = true;

            StartCoroutine(ProcessMicrophoneAudio());
        }

        // Microphone audio is analyzed at regular intervals to detect audio in real time
        IEnumerator ProcessMicrophoneAudio()
        {
            while (isMicModeActive)
            {
                int samplesToGet = Mathf.FloorToInt(sampleRate * micAnalysisWindowSeconds);
                int currentPos = Microphone.GetPosition(micDevice);
                int totalSamples = audioSource.clip.samples;

                int startPos = currentPos - samplesToGet;
                if (startPos < 0)
                    startPos += totalSamples;

                float[] samples = new float[samplesToGet];

                // Processing of circular buffers
                if (startPos + samplesToGet <= totalSamples)
                {
                    audioSource.clip.GetData(samples, startPos);
                }
                else
                {
                    int firstPart = totalSamples - startPos;
                    float[] firstBuffer = new float[firstPart];
                    float[] secondBuffer = new float[samplesToGet - firstPart];

                    audioSource.clip.GetData(firstBuffer, startPos);
                    audioSource.clip.GetData(secondBuffer, 0);

                    firstBuffer.CopyTo(samples, 0);
                    secondBuffer.CopyTo(samples, firstPart);
                }

                // ✅ Real-time VAD analysis based on float[]
                bool isSpeech = vadDetector.IsSpeechDetected(samples, audioSource.clip.channels);

                if (targetRenderer != null)
                {
                    targetRenderer.material.color = isSpeech ? Color.green : Color.red;
                }

                if (isSpeech)
                {
                    Debug.Log("Voice detected by microphone.");
                }

                yield return new WaitForSeconds(0.1f);
            }
        }


        void Update()
        {
            // Update when processing file-based audio (operates during audio clip playback only)
            if (speechSegments != null && audioSource != null && audioSource.clip != null && audioSource.isPlaying)
            {
                float currentTime = audioSource.time;
                bool isSpeech = false;

                foreach (var segment in speechSegments)
                {
                    if (currentTime >= segment.StartSecond && currentTime <= segment.EndSecond)
                    {
                        isSpeech = true;
                        break;
                    }
                }

                if (targetRenderer != null)
                {
                    targetRenderer.material.color = isSpeech ? Color.green : Color.red;
                }

                if (isSpeech)
                {
                    Debug.Log("Voice detected.");
                }
            }
        }
    }
}
