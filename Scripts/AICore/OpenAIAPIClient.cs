using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class OpenAIAPIClient : MonoBehaviour
{
    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public class OpenAIRequest
    {
        public string model = "gpt-4o-mini";
        public List<Message> messages;
        public int max_tokens = 1000;
    }

    [System.Serializable]
    public class Choice
    {
        public Message message;
    }

    [System.Serializable]
    public class OpenAIResponse
    {
        public List<Choice> choices;
    }

    public string prompt;
    public string input;
    public string responseString;
    public string content;
    public int questionCount = 5;
    public TriviaQuestionList triviaQuestions;

    void Awake()
    {
        //read from quizPrompt.txt file from Application.dataPath
        var path = Application.dataPath + "/quizPrompt.txt";
        prompt = System.IO.File.ReadAllText(path);

    }
    public void SendPrompt()
    {
        string theme = input;
        if (!string.IsNullOrEmpty(theme))
        {
            List<Message> messages = new List<Message>
            {
                new Message { role = "system", content = prompt },
                new Message { role = "user", content = $"Generate a trivia quiz with the theme '{theme}' and { questionCount } questions " }// +
                //$"This is my json data structure. please format it to be compatible\r\n\r\n[System.Serializable]\r\npublic class TriviaQuestion\r\n{{\r\n    public string question;\r\n    public List<string> options;\r\n    public string correctAnswer;\r\n}}\r\n\r\n[System.Serializable]\r\npublic class TriviaQuestionList\r\n{{\r\n    public List<TriviaQuestion> questions;\r\n}}" }
            };

            StartCoroutine(SendRequest(messages));
        }
    }

    private IEnumerator SendRequest(List<Message> messages)
    {
        OpenAIRequest requestBody = new OpenAIRequest
        {
            messages = messages
        };

        string jsonData = JsonConvert.SerializeObject(requestBody);

        using (UnityWebRequest webRequest = new UnityWebRequest(TrellisClient.endpoints.openAIEndpoint, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", "Bearer " + TrellisClient.endpoints.openAIKey);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                responseString = webRequest.downloadHandler.text;
                Debug.Log("Response: " + responseString);

                try
                {
                    OpenAIResponse response = JsonConvert.DeserializeObject<OpenAIResponse>(responseString);
                    if (response.choices != null && response.choices.Count > 0)
                    {
                        content = response.choices[0].message.content;
                        Debug.Log(content);
                        triviaQuestions = JsonConvert.DeserializeObject<TriviaQuestionList>(content);
                        QuestionManager.OnQuestionsGenerated(triviaQuestions);
                        //DisplayQuestions();
                    }
                    else
                    {
                        Debug.LogError("No choices found in the response.");
                        responseString = "Error: No choices found in the response.";
                    }
                }
                catch (JsonException e)
                {
                    Debug.LogError("Failed to parse response: " + e.Message);
                    responseString = "Error parsing response.";
                }
            }
            else
            {
                Debug.LogError("Request failed: " + webRequest.error);
                responseString = "Error: " + webRequest.error;
            }
        }
    }

    private void DisplayQuestions()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var question in triviaQuestions.questions)
        {
            sb.AppendLine($"Question: {question.question}");
            for (int i = 0; i < question.options.Count; i++)
            {
                sb.AppendLine($"{i + 1}. {question.options[i]}");
            }
            sb.AppendLine($"Correct Answer: {question.correctAnswer}\n");
        }
        //responseString = sb.ToString();
        Debug.Log(responseString);
    }

    private void OnGUI()
    {
        input = GUI.TextField(new Rect(10, 120, 300, 30), input);

        if (GUI.Button(new Rect(10, 10, 150, 100), "Send Prompt"))
        {
            SendPrompt();
        }
    }
}
