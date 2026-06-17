using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace DesktopPet.UI
{
    [Serializable]
    public class AIChatMessageData
    {
        public string role;
        public string content;

        public AIChatMessageData(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }

    [Serializable]
    public class AIChatRequestConfig
    {
        public string endpoint;
        public string apiKey;
        public string model;
    }

    [Serializable]
    public class AIChatRequestOptions
    {
        public string systemPrompt;
        public int timeoutSeconds = 45;
        public int maxTokens;
        public float temperature = -1f;
        public bool disableThinking;
    }

    public static class AIChatService
    {
        private const string DefaultSystemPrompt = "You are a concise desktop pet assistant.";

        public static IEnumerator SendChatCompletion(
            AIChatRequestConfig config,
            List<AIChatMessageData> conversation,
            Action<string> onSuccess,
            Action<string> onError)
        {
            yield return SendChatCompletion(config, conversation, null, onSuccess, onError);
        }

        public static IEnumerator SendChatCompletion(
            AIChatRequestConfig config,
            List<AIChatMessageData> conversation,
            AIChatRequestOptions options,
            Action<string> onSuccess,
            Action<string> onError)
        {
            if (config == null)
            {
                onError?.Invoke("Missing AI chat config.");
                yield break;
            }

            if (string.IsNullOrWhiteSpace(config.endpoint))
            {
                onError?.Invoke("Missing endpoint URL.");
                yield break;
            }

            if (string.IsNullOrWhiteSpace(config.apiKey))
            {
                onError?.Invoke("Missing API key.");
                yield break;
            }

            if (string.IsNullOrWhiteSpace(config.model))
            {
                onError?.Invoke("Missing model.");
                yield break;
            }

            byte[] body = Encoding.UTF8.GetBytes(BuildRequestJson(config.model.Trim(), conversation, options));
            using UnityWebRequest request = new UnityWebRequest(config.endpoint.Trim(), UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = Mathf.Max(1, options?.timeoutSeconds ?? 45);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {config.apiKey.Trim()}");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                string errorMessage = TryReadErrorMessage(request.downloadHandler.text);
                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = request.error;
                }

                onError?.Invoke(errorMessage);
                yield break;
            }

            string responseText = request.downloadHandler.text;
            ChatCompletionsResponse response = JsonUtility.FromJson<ChatCompletionsResponse>(responseText);
            string content = ExtractContent(response);
            if (string.IsNullOrWhiteSpace(content))
            {
                string errorMessage = TryReadErrorMessage(responseText);
                onError?.Invoke(string.IsNullOrWhiteSpace(errorMessage) ? "Empty response from AI service." : errorMessage);
                yield break;
            }

            onSuccess?.Invoke(content.Trim());
        }

        private static string BuildRequestJson(string model, List<AIChatMessageData> conversation, AIChatRequestOptions options)
        {
            AIChatMessageData[] messages = BuildMessages(conversation, options?.systemPrompt);
            if (ShouldUseTunedRequest(options))
            {
                TunedChatCompletionsRequest tunedRequest = new TunedChatCompletionsRequest
                {
                    model = model,
                    messages = messages,
                    max_tokens = Mathf.Max(1, options.maxTokens),
                    temperature = options.temperature >= 0f ? Mathf.Clamp(options.temperature, 0f, 2f) : 0.8f,
                    thinking = options.disableThinking ? new ThinkingData { type = "disabled" } : null
                };

                return JsonUtility.ToJson(tunedRequest);
            }

            ChatCompletionsRequest request = new ChatCompletionsRequest
            {
                model = model,
                messages = messages
            };

            return JsonUtility.ToJson(request);
        }

        private static bool ShouldUseTunedRequest(AIChatRequestOptions options)
        {
            return options != null
                && (options.maxTokens > 0 || options.temperature >= 0f || options.disableThinking);
        }

        private static AIChatMessageData[] BuildMessages(List<AIChatMessageData> conversation, string systemPrompt)
        {
            string resolvedSystemPrompt = string.IsNullOrWhiteSpace(systemPrompt)
                ? DefaultSystemPrompt
                : systemPrompt.Trim();

            List<AIChatMessageData> messages = new List<AIChatMessageData>
            {
                new AIChatMessageData("system", resolvedSystemPrompt)
            };

            if (conversation != null && conversation.Count > 0)
            {
                messages.AddRange(conversation);
            }

            return messages.ToArray();
        }

        private static string ExtractContent(ChatCompletionsResponse response)
        {
            if (response?.choices == null || response.choices.Length == 0)
            {
                return null;
            }

            ChoiceData choice = response.choices[0];
            if (choice?.message != null && !string.IsNullOrWhiteSpace(choice.message.content))
            {
                return choice.message.content;
            }

            if (!string.IsNullOrWhiteSpace(choice?.text))
            {
                return choice.text;
            }

            return null;
        }

        private static string TryReadErrorMessage(string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
            {
                return null;
            }

            ErrorEnvelope envelope = JsonUtility.FromJson<ErrorEnvelope>(responseText);
            if (!string.IsNullOrWhiteSpace(envelope?.error?.message))
            {
                return envelope.error.message;
            }

            return null;
        }

        [Serializable]
        private class ChatCompletionsRequest
        {
            public string model;
            public AIChatMessageData[] messages;
        }

        [Serializable]
        private class TunedChatCompletionsRequest
        {
            public string model;
            public AIChatMessageData[] messages;
            public int max_tokens;
            public float temperature;
            public ThinkingData thinking;
        }

        [Serializable]
        private class ThinkingData
        {
            public string type;
        }

        [Serializable]
        private class ChatCompletionsResponse
        {
            public ChoiceData[] choices;
        }

        [Serializable]
        private class ChoiceData
        {
            public ResponseMessageData message;
            public string text;
        }

        [Serializable]
        private class ResponseMessageData
        {
            public string content;
        }

        [Serializable]
        private class ErrorEnvelope
        {
            public ErrorData error;
        }

        [Serializable]
        private class ErrorData
        {
            public string message;
        }
    }
}
