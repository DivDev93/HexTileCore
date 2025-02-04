using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using TMPro;
using System;
//using UnityEngine.XR.Interaction.Toolkit.Samples.SpatialKeyboard;

// Example: Microphone + Whisper transcription script.
// Attach this to an empty GameObject in your scene, and
// add UI buttons or a simple OnGUI approach to start and stop recording.
public class MicrophoneToWhisper : MonoBehaviour
{
    // Hugging Face Inference API endpoint (Whisper model)
    private ServerConfigEndpoints endpoints;// = "https://api-inference.huggingface.co/models/openai/whisper-large-v3-turbo";

    // UI references (if using TMP)
    public TMP_Text statusText;
    public TMP_InputField transcriptField;
    //public XRKeyboard keyboard;

    private AudioClip recordedClip;
    private string microphoneDevice;
    private bool isRecording = false;

    // For WAV header construction
    const int HEADER_SIZE = 44;

    private void Awake()
    {
        endpoints = Resources.Load<ServerConfigEndpoints>("EndpointsConfig");
    }

    private void Start()
    {
        // If you have multiple microphones, choose the default or the first
        if (Microphone.devices.Length > 0)
        {
            microphoneDevice = Microphone.devices[0];
        }
        else
        {
            Debug.LogError("No microphone devices found!");
        }
    }

    public void StartRecording()
    {
        if (string.IsNullOrEmpty(microphoneDevice))
        {
            Debug.LogError("No microphone device available.");
            return;
        }

        // Optionally set a frequency like 44100 or 16000
        int sampleRate = 44100;
        // Start recording (10 seconds max, or you could do int.MaxValue for unlimited)
        recordedClip = Microphone.Start(microphoneDevice, false, 10, sampleRate);
        isRecording = true;

        if (statusText) statusText.text = "Recording...";
        Debug.Log("Recording started...");

        //StopCoroutine(PollForSilenceToStopRecording());
        //StartCoroutine(PollForSilenceToStopRecording());
    }

    public void ToggleRecording()
    {
        if(isRecording)
        {
            StopRecordingAndTranscribe();
        }
        else
        {
            StartRecording();
        }
    }

    IEnumerator PollForSilenceToStopRecording()
    {
        yield return new WaitForSeconds(0.5f);
        while (Microphone.IsRecording(microphoneDevice))
        {
            float[] samples = new float[recordedClip.samples * recordedClip.channels];
            recordedClip.GetData(samples, 0);
            if (IsLoudEnough(samples))
            {
                yield return null;
            }
            else
            {
                StopRecordingAndTranscribe();
            }
        }
    }
    public void StopRecordingAndTranscribe()
    {
        if (!isRecording) return;

        // Stop recording
        Microphone.End(microphoneDevice);
        isRecording = false;
        Debug.Log("Recording stopped.");

        if (statusText) statusText.text = "Processing / Transcribing...";

        // Now we have recordedClip as an AudioClip in memory
        // Convert it to WAV bytes
        byte[] wavData = AudioClipToWavBytes(recordedClip);

        // Send the wav data to Hugging Face endpoint
        StartCoroutine(SendAudioToHuggingFace(wavData));
    }

    private IEnumerator SendAudioToHuggingFace(byte[] wavData)
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(endpoints.speechToTextEndpoint, "POST"))
        {
            // We send raw binary, so the header is "application/octet-stream"
            webRequest.uploadHandler = new UploadHandlerRaw(wavData);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/octet-stream");
            webRequest.SetRequestHeader("Authorization", "Bearer " + endpoints.huggingFaceToken);

            Debug.Log("Sending WAV data to Hugging Face...");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Parse JSON response
                string responseJson = webRequest.downloadHandler.text;
                Debug.Log("Received Whisper response: " + responseJson);

                // The Whisper endpoint typically returns something like { "text": "transcribed text" }
                // So let's parse that
                WhisperResponse whisperResp = JsonConvert.DeserializeObject<WhisperResponse>(responseJson);

                if (whisperResp != null && !string.IsNullOrEmpty(whisperResp.text))
                {
                    if (statusText) statusText.text = "Transcription complete!";
                    if (transcriptField) transcriptField.text = whisperResp.text;
                    Debug.Log("Transcribed text: " + whisperResp.text);
                }
                else
                {
                    Debug.LogWarning("Unexpected response: " + responseJson);
                    if (statusText) statusText.text = "Unexpected Whisper response!";
                }
            }
            else
            {
                Debug.LogError("Whisper request error: " + webRequest.error);
                if (statusText) statusText.text = "Error transcribing audio.";
            }
            //GlobalNonNativeKeyboard.instance.keyboard.OnInputFieldValueChange(transcriptField.text);
        }
    }

    // Check if the audio is loud enough to send to the whisper model
    private bool IsLoudEnough(float[] samples)
    {
        float sum = 0;
        for (int i = 0; i < samples.Length; i++)
        {
            sum += Mathf.Abs(samples[i]);
        }

        float rms = Mathf.Sqrt(sum / samples.Length);
        float db = 20 * Mathf.Log10(rms);

        Debug.Log("Audio RMS: " + rms + " dB: " + db);

        // You can adjust this threshold as needed
        return db > -30;
    }



    //Coroutine to continuously check if the audio from the microphone is loud enough to send to the whisper model and transcribe
    //before clearing the audio buffer and continuing to listen
    // Add this field to store the silence duration threshold (in seconds)
    [SerializeField] private float silenceDurationThreshold = 1.2f;

    // Modify the ListenForAudio coroutine
    private IEnumerator ListenForAudio()
    {
        float silenceDuration = 0.0f;

        while (true)
        {
            // Check if the microphone is recording
            if (isRecording)
            {
                // Check if the audio is loud enough to send to the whisper model
                if (Microphone.IsRecording(microphoneDevice))
                {
                    // Get the audio data from the microphone
                    float[] samples = new float[recordedClip.samples * recordedClip.channels];
                    recordedClip.GetData(samples, 0);

                    // Check if the audio is loud enough to send to the whisper model
                    if (IsLoudEnough(samples))
                    {
                        // Reset silence duration
                        silenceDuration = 0.0f;
                    }
                    else
                    {
                        // Increment silence duration
                        silenceDuration += Time.deltaTime;

                        // Check if silence duration exceeds the threshold
                        if (silenceDuration >= silenceDurationThreshold)
                        {
                            // Stop recording
                            Microphone.End(microphoneDevice);
                            isRecording = false;
                            Debug.Log("Recording stopped due to silence.");

                            if (statusText) statusText.text = "Processing / Transcribing...";                           

                            // Start recording again
                            StartRecording();
                        }
                    }

                    // Convert the audio data to a WAV byte array
                    byte[] wavData = AudioClipToWavBytes(recordedClip);

                    // Send the audio data to the whisper model
                    yield return SendAudioToHuggingFace(wavData);
                }
            }

            // Wait for a short time before checking again
            yield return new WaitForSeconds(0.1f);
        }
    }


    [System.Serializable]
    public class WhisperResponse
    {
        public string text;
    }

    /// <summary>
    /// Converts an AudioClip to WAV byte array in memory.
    /// This is a minimal approach—works for single-channel or stereo.
    /// For more advanced usage (e.g., multi-channel, big-endian, etc.), a more robust WAV utility is recommended.
    /// </summary>
    private byte[] AudioClipToWavBytes(AudioClip clip)
    {
        // Get the samples from the clip
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        // Convert float array to 16-bit PCM data
        short[] intData = new short[samples.Length];
        // Rescale float to short
        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * short.MaxValue);
        }

        // Convert to byte array
        byte[] bytesData = new byte[intData.Length * 2];
        Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);

        // Prepare WAV header
        byte[] wav = new byte[HEADER_SIZE + bytesData.Length];
        // RIFF header
        Encoding.ASCII.GetBytes("RIFF").CopyTo(wav, 0);
        // Chunk size
        System.BitConverter.GetBytes(wav.Length - 8).CopyTo(wav, 4);
        // Format
        Encoding.ASCII.GetBytes("WAVE").CopyTo(wav, 8);
        // Subchunk1 ID
        Encoding.ASCII.GetBytes("fmt ").CopyTo(wav, 12);
        // Subchunk1 size (16 for PCM)
        System.BitConverter.GetBytes(16).CopyTo(wav, 16);
        // Audio format (1 = PCM)
        System.BitConverter.GetBytes((short)1).CopyTo(wav, 20);
        // Num channels
        System.BitConverter.GetBytes((short)clip.channels).CopyTo(wav, 22);
        // Sample rate
        System.BitConverter.GetBytes(clip.frequency).CopyTo(wav, 24);
        // Byte rate (Sample Rate * BitsPerSample * Channels / 8)
        System.BitConverter.GetBytes(clip.frequency * 2 * clip.channels).CopyTo(wav, 28);
        // Block align (channels * bitsPerSample/8)
        System.BitConverter.GetBytes((short)(clip.channels * 2)).CopyTo(wav, 32);
        // Bits per sample
        System.BitConverter.GetBytes((short)16).CopyTo(wav, 34);
        // Subchunk2 ID
        Encoding.ASCII.GetBytes("data").CopyTo(wav, 36);
        // Subchunk2 size
        System.BitConverter.GetBytes(bytesData.Length).CopyTo(wav, 40);

        // Finally, the PCM data
        bytesData.CopyTo(wav, HEADER_SIZE);

        return wav;
    }

    // Optional OnGUI to test quickly without a separate UI
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 50), "Start Recording"))
        {
            StartRecording();
        }
        if (GUI.Button(new Rect(10, 70, 150, 50), "Stop & Transcribe"))
        {
            StopRecordingAndTranscribe();
        }
    }
}
