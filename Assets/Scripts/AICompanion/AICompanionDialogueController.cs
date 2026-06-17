using System;
using System.Collections;
using System.Collections.Generic;
using DesktopPet.UI;
using UnityEngine;

namespace DesktopPet.AICompanion
{
    public class AICompanionDialogueController : MonoBehaviour
    {
        private const string RuntimeObjectName = "AICompanionDialogueController";
        private const float PromptDurationSeconds = 3f;

        private static AICompanionDialogueController instance;

        [SerializeField] private AICompanionSettings settings = new AICompanionSettings();

        private readonly Dictionary<AICompanionEventType, float> lastTriggerTimes = new Dictionary<AICompanionEventType, float>();
        private readonly AICompanionMemoryStore memoryStore = new AICompanionMemoryStore();

        private ThePet pet;
        private ThePetStatsManager statsManager;
        private ThePetInputManager inputManager;
        private PetPromptUI promptUI;
        private bool isRequesting;
        private bool startupGreetingChecked;
        private bool wasHungry;
        private bool wasLowEnergy;
        private float startedAtTime;
        private float nextReferenceRefreshTime;
        private float nextStatusCheckTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeAfterSceneLoad()
        {
            GetOrCreate();
        }

        public static AICompanionDialogueController GetOrCreate()
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindFirstObjectByType<AICompanionDialogueController>();
            if (instance != null)
            {
                return instance;
            }

            GameObject controllerObject = new GameObject(RuntimeObjectName);
            instance = controllerObject.AddComponent<AICompanionDialogueController>();
            return instance;
        }

        public static bool NotifyEvent(AICompanionEventType eventType)
        {
            AICompanionDialogueController controller = GetOrCreate();
            return controller != null && controller.RequestDialogue(eventType);
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            settings ??= new AICompanionSettings();
            settings.Sanitize();
            startedAtTime = Time.time;
            RefreshReferences(true);
        }

        private void Update()
        {
            if (settings == null || !settings.enableCompanionDialogue)
            {
                return;
            }

            RefreshReferences(false);
            TickStartupGreeting();
            TickStatusWarnings();
            TickIdleGreeting();
        }

        public bool RequestDialogue(AICompanionEventType eventType)
        {
            if (settings == null || !settings.enableCompanionDialogue)
            {
                return false;
            }

            settings.Sanitize();
            RefreshReferences(false);

            if (!PassesTriggerProbability(eventType) || !CanTrigger(eventType))
            {
                return false;
            }

            AICompanionPriority priority = settings.GetPriority(eventType);
            if (isRequesting)
            {
                if (priority == AICompanionPriority.Low)
                {
                    return false;
                }

                MarkTriggered(eventType);
                ShowFallback(eventType);
                return true;
            }

            MarkTriggered(eventType);
            AICompanionContext context = BuildContext(eventType);
            if (!CanAttemptAi())
            {
                ShowFallback(eventType);
                return true;
            }

            StartCoroutine(RequestAiDialogue(eventType, context));
            return true;
        }

        private void TickStartupGreeting()
        {
            if (startupGreetingChecked || Time.time - startedAtTime < settings.startupGreetingDelaySeconds)
            {
                return;
            }

            startupGreetingChecked = true;
            if (HasUrgentNeed())
            {
                return;
            }

            int hour = DateTime.Now.Hour;
            if (hour >= 6 && hour < 12)
            {
                RequestDialogue(AICompanionEventType.MorningGreeting);
                return;
            }

            if (hour >= 21 || hour < 2)
            {
                RequestDialogue(AICompanionEventType.NightGreeting);
            }
        }

        private void TickStatusWarnings()
        {
            if (Time.time < nextStatusCheckTime)
            {
                return;
            }

            nextStatusCheckTime = Time.time + settings.statusCheckIntervalSeconds;
            ThePetStats stats = statsManager != null ? statsManager.current_stats : null;
            if (stats == null)
            {
                return;
            }

            bool hungry = stats.satiety < settings.hungryThreshold;
            if (hungry && (!wasHungry || IsCooldownReady(AICompanionEventType.HungryWarning)))
            {
                RequestDialogue(AICompanionEventType.HungryWarning);
            }

            wasHungry = hungry;

            bool lowEnergy = !hungry && stats.energy < settings.lowEnergyThreshold;
            if (lowEnergy && (!wasLowEnergy || IsCooldownReady(AICompanionEventType.LowEnergyWarning)))
            {
                RequestDialogue(AICompanionEventType.LowEnergyWarning);
            }

            wasLowEnergy = lowEnergy;
        }

        private void TickIdleGreeting()
        {
            if (inputManager == null || HasUrgentNeed())
            {
                return;
            }

            if (inputManager.GetSecondsSinceInteraction() < settings.idleGreetingSeconds)
            {
                return;
            }

            if (IsCooldownReady(AICompanionEventType.IdleGreeting))
            {
                RequestDialogue(AICompanionEventType.IdleGreeting);
            }
        }

        private IEnumerator RequestAiDialogue(AICompanionEventType eventType, AICompanionContext context)
        {
            isRequesting = true;
            memoryStore.RecordAiRequest();

            string reply = null;
            string error = null;
            bool completed = false;
            bool fallbackShown = false;

            AIChatRequestConfig config = BuildRequestConfig();
            AIChatRequestOptions options = new AIChatRequestOptions
            {
                systemPrompt = AICompanionPromptBuilder.SystemPrompt,
                timeoutSeconds = Mathf.CeilToInt(settings.hardTimeoutSeconds),
                maxTokens = 40,
                temperature = 0.8f,
                disableThinking = true
            };

            List<AIChatMessageData> messages = AICompanionPromptBuilder.BuildMessages(context, settings.allowPetStateInPrompt);
            Coroutine requestCoroutine = StartCoroutine(AIChatService.SendChatCompletion(
                config,
                messages,
                options,
                content =>
                {
                    reply = content;
                    completed = true;
                },
                message =>
                {
                    error = message;
                    completed = true;
                }));

            float startTime = Time.realtimeSinceStartup;
            while (!completed && Time.realtimeSinceStartup - startTime < settings.hardTimeoutSeconds)
            {
                if (!fallbackShown && Time.realtimeSinceStartup - startTime >= settings.softTimeoutSeconds)
                {
                    ShowFallback(eventType);
                    fallbackShown = true;
                }

                yield return null;
            }

            if (!completed)
            {
                StopCoroutine(requestCoroutine);
                isRequesting = false;
                yield break;
            }

            if (TrySanitizeReply(reply, out string sanitizedReply))
            {
                ShowPrompt(sanitizedReply);
                RecordDialogue(eventType, sanitizedReply, true);
            }
            else if (!fallbackShown)
            {
                ShowFallback(eventType);
            }
            else if (!string.IsNullOrWhiteSpace(error))
            {
                Debug.LogWarning($"AI companion dialogue failed: {error}");
            }

            isRequesting = false;
        }

        private bool CanTrigger(AICompanionEventType eventType)
        {
            if (IsDailyGreeting(eventType) && memoryStore.WasDailyGreetingShown(eventType))
            {
                return false;
            }

            return IsCooldownReady(eventType);
        }

        private bool IsCooldownReady(AICompanionEventType eventType)
        {
            if (!lastTriggerTimes.TryGetValue(eventType, out float lastTime))
            {
                return true;
            }

            return Time.time - lastTime >= settings.GetCooldownSeconds(eventType);
        }

        private bool PassesTriggerProbability(AICompanionEventType eventType)
        {
            float probability = settings.GetTriggerProbability(eventType);
            return probability >= 1f || UnityEngine.Random.value <= probability;
        }

        private bool CanAttemptAi()
        {
            if (!settings.allowAiGeneration || settings.fallbackOnly)
            {
                return false;
            }

            AIChatRequestConfig config = BuildRequestConfig();
            if (string.IsNullOrWhiteSpace(config.endpoint) ||
                string.IsNullOrWhiteSpace(config.apiKey) ||
                string.IsNullOrWhiteSpace(config.model))
            {
                return false;
            }

            return memoryStore.CanUseHourlyAi(settings.maxAiRequestsPerHour);
        }

        private void MarkTriggered(AICompanionEventType eventType)
        {
            lastTriggerTimes[eventType] = Time.time;
        }

        private void ShowFallback(AICompanionEventType eventType)
        {
            string line = AICompanionFallbackLines.GetLine(eventType, memoryStore.LastLine);
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            ShowPrompt(line);
            RecordDialogue(eventType, line, false);
        }

        private void ShowPrompt(string line)
        {
            PetPromptUI resolvedPrompt = ResolvePromptUI();
            if (resolvedPrompt != null)
            {
                resolvedPrompt.ShowPrompt(line, PromptDurationSeconds);
                return;
            }

            Debug.Log(line);
        }

        private void RecordDialogue(AICompanionEventType eventType, string line, bool fromAi)
        {
            if (settings.recordToMemory)
            {
                memoryStore.RecordDialogue(eventType, line, fromAi);
            }
        }

        private bool TrySanitizeReply(string rawReply, out string sanitizedReply)
        {
            sanitizedReply = string.Empty;
            if (string.IsNullOrWhiteSpace(rawReply))
            {
                return false;
            }

            string line = rawReply.Trim()
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim('"', '\'', '“', '”', ' ');

            while (line.Contains("  "))
            {
                line = line.Replace("  ", " ");
            }

            if (string.IsNullOrWhiteSpace(line) || line.Length > settings.maxReplyCharacters)
            {
                return false;
            }

            string[] blockedTerms =
            {
                "我是AI",
                "我是 AI",
                "根据数据",
                "根据状态",
                "饱食度",
                "快乐值",
                "活力值",
                "亲密度",
                "规则"
            };

            foreach (string blockedTerm in blockedTerms)
            {
                if (line.Contains(blockedTerm))
                {
                    return false;
                }
            }

            if (line == memoryStore.LastLine)
            {
                return false;
            }

            sanitizedReply = line;
            return true;
        }

        private AICompanionContext BuildContext(AICompanionEventType eventType)
        {
            ThePetStats stats = statsManager != null ? statsManager.current_stats : null;
            return new AICompanionContext
            {
                eventType = eventType,
                eventName = GetEventName(eventType),
                timePeriod = GetTimePeriod(),
                petMood = GetPetMood(),
                intimacyLevel = GetIntimacyLevel(stats),
                happinessLevel = GetHappinessLevel(stats),
                satietyLevel = GetSatietyLevel(stats),
                energyLevel = GetEnergyLevel(stats),
                recentAction = GetRecentAction(eventType),
                lastLine = memoryStore.LastLine,
                hasUrgentNeed = HasUrgentNeed()
            };
        }

        private bool HasUrgentNeed()
        {
            ThePetStats stats = statsManager != null ? statsManager.current_stats : null;
            return stats != null && (stats.satiety < settings.hungryThreshold || stats.energy < settings.lowEnergyThreshold);
        }

        private void RefreshReferences(bool force)
        {
            if (!force && Time.time < nextReferenceRefreshTime)
            {
                return;
            }

            nextReferenceRefreshTime = Time.time + 2f;

            if (pet == null)
            {
                pet = FindFirstObjectByType<ThePet>();
            }

            if (statsManager == null && pet != null)
            {
                statsManager = pet.GetComponent<ThePetStatsManager>();
            }

            if (inputManager == null && pet != null)
            {
                inputManager = pet.GetComponent<ThePetInputManager>();
            }

            if (promptUI == null)
            {
                ResolvePromptUI();
            }
        }

        private PetPromptUI ResolvePromptUI()
        {
            if (promptUI != null)
            {
                return promptUI;
            }

            PetStatsDisplayUI statsDisplayUI = FindFirstObjectByType<PetStatsDisplayUI>();
            if (statsDisplayUI == null)
            {
                return null;
            }

            promptUI = statsDisplayUI.GetComponent<PetPromptUI>();
            if (promptUI == null)
            {
                promptUI = statsDisplayUI.gameObject.AddComponent<PetPromptUI>();
            }

            return promptUI;
        }

        private static AIChatRequestConfig BuildRequestConfig()
        {
            return new AIChatRequestConfig
            {
                endpoint = GameSettingsStore.GetAiChatEndpoint(),
                apiKey = GameSettingsStore.GetAiChatApiKey(),
                model = GameSettingsStore.GetAiChatModel()
            };
        }

        private string GetPetMood()
        {
            string stateName = pet != null && pet.states != null && pet.states.current != null
                ? pet.states.current.GetType().Name
                : string.Empty;

            switch (stateName)
            {
                case nameof(HappyState):
                    return "开心";
                case nameof(HungryState):
                    return "饥饿";
                case nameof(SleepState):
                    return "困倦";
                case nameof(SadState):
                    return "低落";
                case nameof(StretchState):
                    return "放松";
                case nameof(DragState):
                    return "被移动中";
                case nameof(LyingState):
                    return "平静";
                default:
                    return "平静";
            }
        }

        private static string GetTimePeriod()
        {
            int hour = DateTime.Now.Hour;
            if (hour >= 6 && hour < 12)
            {
                return "上午";
            }

            if (hour >= 12 && hour < 18)
            {
                return "下午";
            }

            if (hour >= 18 && hour < 23)
            {
                return "晚上";
            }

            return "深夜";
        }

        private static string GetEventName(AICompanionEventType eventType)
        {
            switch (eventType)
            {
                case AICompanionEventType.PetClicked:
                    return "玩家点击了宠物";
                case AICompanionEventType.PetDragged:
                    return "玩家拖动了宠物";
                case AICompanionEventType.HungryWarning:
                    return "宠物有点饿";
                case AICompanionEventType.LowEnergyWarning:
                    return "宠物活力较低";
                case AICompanionEventType.IdleGreeting:
                    return "玩家长时间没有互动";
                case AICompanionEventType.MorningGreeting:
                    return "上午问候";
                case AICompanionEventType.NightGreeting:
                    return "夜晚提醒休息";
                default:
                    return "日常互动";
            }
        }

        private static string GetRecentAction(AICompanionEventType eventType)
        {
            switch (eventType)
            {
                case AICompanionEventType.PetClicked:
                    return "玩家刚刚摸了宠物";
                case AICompanionEventType.PetDragged:
                    return "玩家刚刚把宠物移动到新位置";
                case AICompanionEventType.HungryWarning:
                    return "宠物需要被照顾";
                case AICompanionEventType.LowEnergyWarning:
                    return "宠物想休息一会儿";
                case AICompanionEventType.IdleGreeting:
                    return "玩家正在忙自己的事情";
                case AICompanionEventType.MorningGreeting:
                    return "新的一天刚开始";
                case AICompanionEventType.NightGreeting:
                    return "现在已经比较晚了";
                default:
                    return string.Empty;
            }
        }

        private static string GetIntimacyLevel(ThePetStats stats)
        {
            if (stats == null)
            {
                return "普通";
            }

            if (stats.intimacy < 50f)
            {
                return "陌生";
            }

            if (stats.intimacy < 200f)
            {
                return "熟悉";
            }

            return "亲近";
        }

        private static string GetHappinessLevel(ThePetStats stats)
        {
            if (stats == null)
            {
                return "平静";
            }

            if (stats.happiness < 40f)
            {
                return "低落";
            }

            if (stats.happiness <= 70f)
            {
                return "平静";
            }

            return "开心";
        }

        private static string GetSatietyLevel(ThePetStats stats)
        {
            if (stats == null)
            {
                return "正常";
            }

            if (stats.satiety < 30f)
            {
                return "很饿";
            }

            if (stats.satiety <= 60f)
            {
                return "有点饿";
            }

            return "正常";
        }

        private static string GetEnergyLevel(ThePetStats stats)
        {
            if (stats == null)
            {
                return "普通";
            }

            if (stats.energy < 40f)
            {
                return "疲惫";
            }

            if (stats.energy <= 100f)
            {
                return "普通";
            }

            return "精神不错";
        }

        private static bool IsDailyGreeting(AICompanionEventType eventType)
        {
            return eventType == AICompanionEventType.MorningGreeting
                || eventType == AICompanionEventType.NightGreeting;
        }

        private void OnValidate()
        {
            settings ??= new AICompanionSettings();
            settings.Sanitize();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
