#if !UNITY_WEBGL
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using TMPro;
using System;
using static UnityEngine.Rendering.DebugUI.Table;

/// <summary>
/// Continuously records audio from the microphone, detects silence for a given duration,
/// stops that chunk of recording, sends it to Whisper, and appends the transcribed text
/// to a TMP_InputField.
/// 
/// Attach this script to a GameObject in your Scene. Assign the 'transcriptField' in the Inspector.
/// 
/// In production: 
/// - You might want a more robust WAV converter (multi-channel, streaming, etc.).
/// - Hiding your HF token is recommended.
/// - Be mindful of memory usage for long recordings.
/// </summary>
public class ContinuousMicToWhisper : MonoBehaviour
{
    [Header("Hugging Face Whisper Endpoint")]
    [SerializeField] private string endpoint = "https://api-inference.huggingface.co/models/openai/whisper-large-v3-turbo";
    [SerializeField] private string token = "hf_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";

    [Header("UI References")]
    public TMP_InputField transcriptField;
    public TMP_Text statusText;

    [Header("Recording Settings")]
    // Sample rate for the mic. 16k or 44.1k are common.
    public int sampleRate = 16000;
    const int numSamples = 160;
    // If you have more than one microphone, you can choose the device name here (or pick from Microphone.devices).
    private string microphoneDevice;

    [Header("Chunking & Silence Detection")]
    // How many seconds of audio to accumulate before sending a chunk to Whisper
    public float chunkDuration = 3.0f;
    // Volume threshold below which we consider "silence"
    public float silenceVolumeThreshold = 0.01f;
    // How long we must remain below the threshold to consider the user "done"
    public float silenceDurationToStop = 3.0f;

    private AudioClip recordingClip;
    private bool isCapturing = false;
    private float silenceTimer = 0f;

    // Used to store chunk data between transmissions
    private List<float> recordedSamples = new List<float>();
    private float timeSinceLastChunk = 0f;

    private const int WAV_HEADER_SIZE = 44;
    IntPtr m_AECHandle;

    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            microphoneDevice = Microphone.devices[0];
        }
        else
        {
            if (statusText) statusText.text = "No microphone devices found!";
            Debug.LogError("No microphone devices found!");
        }
        m_AECHandle = AECInterop.WebRtcAec3_Create(sampleRate);
    }

    /// <summary>
    /// Start capturing audio in small chunks, sending them to Whisper, and automatically stop on silence.
    /// </summary>
    public void StartContinuousRecognition()
    {
        if (string.IsNullOrEmpty(microphoneDevice))
        {
            Debug.LogError("No microphone device available.");
            if (statusText) statusText.text = "No microphone device found!";
            return;
        }

        // Reset
        isCapturing = true;
        silenceTimer = 0f;
        timeSinceLastChunk = 0f;
        recordedSamples.Clear();
        transcriptField.text = "";

        // Start mic capture. Use a long length so it doesn't forcibly stop. We'll stop manually.
        // "loop: true" because we'll be manually pulling the data anyway.
        recordingClip = Microphone.Start(microphoneDevice, true, 300, sampleRate);

        if (statusText) statusText.text = "Recording... (speak now)";
        Debug.Log("Recording started...");

        // Begin the update loop as a coroutine
        StartCoroutine(UpdateRecording());
    }

    /// <summary>
    /// Force stop the capturing process.
    /// </summary>
    public void StopContinuousRecognition()
    {
        if (!isCapturing) return;

        isCapturing = false;
        Microphone.End(microphoneDevice);
        if (statusText) statusText.text = "Stopped recording.";
        Debug.Log("Stopped recording manually.");
    }

    /// <summary>
    /// This coroutine simulates a per-frame update that reads new audio from the microphone,
    /// accumulates it, sends chunks to Whisper every few seconds, and checks for silence to stop.
    /// </summary>
    private IEnumerator UpdateRecording()
    {
        int lastPosition = 0;
        while (isCapturing)
        {
            // Wait for a fraction of a second to reduce overhead
            yield return new WaitForSeconds(0.1f);

            int currentPosition = Microphone.GetPosition(microphoneDevice);
            if (currentPosition < lastPosition)
            {
                // Microphone looped around
                currentPosition += recordingClip.samples;
            }

            float amplitude = 0f;
            // If there's new data, copy it into our recordedSamples
            int samplesToCopy = currentPosition - lastPosition;
            if (samplesToCopy > 0)
            {
                float[] temp = new float[samplesToCopy * recordingClip.channels];
                recordingClip.GetData(temp, lastPosition % recordingClip.samples);
                recordedSamples.AddRange(temp);

                // Compute amplitude to check for silence
                amplitude = 0f;
                foreach (var sample in temp)
                {
                    float absVal = Mathf.Abs(sample);
                    if (absVal > amplitude) amplitude = absVal;
                }

                // If amplitude is below threshold, increment silenceTimer
                if (amplitude < silenceVolumeThreshold)
                {
                    silenceTimer += 0.1f;
                }
                else
                {
                    // Reset if we have some speaking
                    silenceTimer = 0f;
                }
            }

            lastPosition = currentPosition;

            // Now, check if we should send a chunk
            timeSinceLastChunk += 0.1f;
            float recordedAmplitude = 0f;
            foreach (var sample in recordedSamples)
            {
                float absVal = Mathf.Abs(sample);
                if (absVal > amplitude) recordedAmplitude = absVal;
            }
            if (timeSinceLastChunk >= chunkDuration && recordedSamples.Count > 0 && recordedAmplitude > silenceVolumeThreshold)
            {
                // Send chunk to transcription
                float[] chunkSamples = recordedSamples.ToArray();
                recordedSamples.Clear();   // Start fresh for the next chunk
                timeSinceLastChunk = 0f;

                // Fire off a transcription request (non-blocking)
                StartCoroutine(SendChunkToWhisper(chunkSamples));
            }

            // Check if we've been silent long enough to stop
            if (silenceTimer >= silenceDurationToStop && recordedSamples.Count > 0)
            {
                // We have a final chunk to send
                //float[] finalChunk =  ProcessedAudioChunk(recordedSamples.ToArray());
                recordedSamples.Clear();

                // Stop capturing so we don't keep reading
                isCapturing = false;
                Microphone.End(microphoneDevice);

                if (statusText) statusText.text = "Final chunk...";

                // Send the final chunk
                //StartCoroutine(SendChunkToWhisper(finalChunk, finalize: true));

                Debug.Log("Stopping due to prolonged silence.");
            }
            else if (silenceTimer >= silenceDurationToStop)
            {
                // No new data to send, just stop.
                isCapturing = false;
                Microphone.End(microphoneDevice);

                if (m_AECHandle != IntPtr.Zero)
                {
                    AECInterop.WebRtcAec3_Free(m_AECHandle);
                    m_AECHandle = IntPtr.Zero;
                }

                if (statusText) statusText.text = "Stopped recording - no new data to send.";
                Debug.Log("Stopping due to prolonged silence (no new data).");
            }
        }
    }

    /// <summary>
    /// Sends a chunk of float samples to Whisper. Optionally mark it as the final chunk.
    /// </summary>
    private IEnumerator SendChunkToWhisper(float[] samples, bool finalize = false)
    {
        if (statusText) statusText.text = "Sending chunk...";

        byte[] wavData = FloatArrayToWav(samples, recordingClip.channels, recordingClip.frequency);

        using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(wavData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/octet-stream");
            request.SetRequestHeader("Authorization", "Bearer " + token);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseJson = request.downloadHandler.text;
                Debug.Log("Whisper response: " + responseJson);

                var whisperResp = JsonConvert.DeserializeObject<WhisperResponse>(responseJson);
                if (whisperResp != null && !string.IsNullOrEmpty(whisperResp.text))
                {
                    // Append to our transcript UI
                    if (transcriptField)
                    {
                        if (string.IsNullOrEmpty(transcriptField.text))
                            transcriptField.text = whisperResp.text;
                        else
                            transcriptField.text += " " + whisperResp.text;
                    }
                }
                else
                {
                    Debug.LogWarning("Unexpected Whisper response: " + responseJson);
                }
            }
            else
            {
                Debug.LogError("Error transcribing chunk: " + request.error);
            }
        }

        // If this was the final chunk, we can update status
        if (finalize && statusText)
        {
            statusText.text = "Done transcribing.";
        }
    }

    /// <summary>
    /// Minimal WAV creation from float array. 16-bit PCM, single or stereo.
    /// </summary>
    private byte[] FloatArrayToWav(float[] samples, int channels, int freq)
    {
        if (samples == null || samples.Length == 0)
            return null;

        // Convert floats to 16-bit PCM
        short[] intData = new short[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)Mathf.Clamp(samples[i] * short.MaxValue, short.MinValue, short.MaxValue);
        }

        // Create a byte array
        byte[] bytesData = new byte[intData.Length * 2];
        Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);

        // Build WAV header
        byte[] wav = new byte[WAV_HEADER_SIZE + bytesData.Length];
        Encoding.ASCII.GetBytes("RIFF").CopyTo(wav, 0);
        System.BitConverter.GetBytes(wav.Length - 8).CopyTo(wav, 4);
        Encoding.ASCII.GetBytes("WAVE").CopyTo(wav, 8);
        Encoding.ASCII.GetBytes("fmt ").CopyTo(wav, 12);
        System.BitConverter.GetBytes(16).CopyTo(wav, 16); // Subchunk1 size
        System.BitConverter.GetBytes((short)1).CopyTo(wav, 20); // Audio format (1=PCM)
        System.BitConverter.GetBytes((short)channels).CopyTo(wav, 22);
        System.BitConverter.GetBytes(freq).CopyTo(wav, 24);
        System.BitConverter.GetBytes(freq * 2 * channels).CopyTo(wav, 28);
        System.BitConverter.GetBytes((short)(channels * 2)).CopyTo(wav, 32);
        System.BitConverter.GetBytes((short)16).CopyTo(wav, 34); // bits per sample
        Encoding.ASCII.GetBytes("data").CopyTo(wav, 36);
        System.BitConverter.GetBytes(bytesData.Length).CopyTo(wav, 40);

        // Copy PCM data
        bytesData.CopyTo(wav, WAV_HEADER_SIZE);

        return wav;
    }

    protected short[] FilteredAudio(short[] inputData)
    {
        //short[] outputData = new short[numSamples];
        short[] filterTmp = new short[numSamples];

        if (m_AECHandle == IntPtr.Zero)
        {
            m_AECHandle = AECInterop.WebRtcAec3_Create(sampleRate);
        }
        AECInterop.WebRtcAec3_BufferFarend(m_AECHandle, inputData);
        AECInterop.WebRtcAec3_Process(m_AECHandle, inputData, filterTmp);

        return filterTmp;
    }

    [System.Serializable]
    public class WhisperResponse
    {
        public string text;
    }

    // Optional onGUI for test
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 50), "Start Continuous"))
        {
            StartContinuousRecognition();
        }
        if (GUI.Button(new Rect(10, 70, 200, 50), "Stop Continuous"))
        {
            StopContinuousRecognition();
        }
    }
    private float[] ProcessedAudioChunk(float[] samples)
    {
        // Convert float array to short array
        short[] inputData = new short[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            inputData[i] = (short)Mathf.Clamp(samples[i] * short.MaxValue, short.MinValue, short.MaxValue);
        }

        // Prepare output array
        //short[] outputData = new short[inputData.Length];

        // Call the FilterAudio method
        short[] outputData = FilteredAudio(inputData);

        // Convert short array back to float array if needed
        float[] processedSamples = new float[outputData.Length];
        for (int i = 0; i < outputData.Length; i++)
        {
            processedSamples[i] = outputData[i] / (float)short.MaxValue;
        }

        return processedSamples;
        // Now you can use processedSamples as needed
    }
}
#endif