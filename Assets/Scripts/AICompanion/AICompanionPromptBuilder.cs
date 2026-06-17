using System.Collections.Generic;
using System.Text;
using DesktopPet.UI;

namespace DesktopPet.AICompanion
{
    public static class AICompanionPromptBuilder
    {
        public const string SystemPrompt =
            "你是一只中文猫咪桌宠。" +
            "请根据当前事件和宠物状态，生成一句自然、简短、有陪伴感的气泡台词。" +
            "不要说自己是 AI。不要解释规则。不要输出多句话。不要超过 24 个中文字符。" +
            "不要直接提到具体数值。语气温柔可爱，但不要过度卖萌。";

        public static List<AIChatMessageData> BuildMessages(AICompanionContext context, bool includePetState)
        {
            return new List<AIChatMessageData>
            {
                new AIChatMessageData("user", BuildUserPrompt(context, includePetState))
            };
        }

        private static string BuildUserPrompt(AICompanionContext context, bool includePetState)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"当前事件：{context.eventName}");
            builder.AppendLine($"时间段：{context.timePeriod}");

            if (includePetState)
            {
                builder.AppendLine($"宠物心情：{context.petMood}");
                builder.AppendLine($"亲密度：{context.intimacyLevel}");
                builder.AppendLine($"快乐状态：{context.happinessLevel}");
                builder.AppendLine($"饱食状态：{context.satietyLevel}");
                builder.AppendLine($"活力状态：{context.energyLevel}");
            }

            if (!string.IsNullOrWhiteSpace(context.recentAction))
            {
                builder.AppendLine($"最近行为：{context.recentAction}");
            }

            if (!string.IsNullOrWhiteSpace(context.lastLine))
            {
                builder.AppendLine($"最近一句台词：{context.lastLine}");
            }

            builder.AppendLine(GetEventInstruction(context.eventType));
            builder.AppendLine("请只输出一句新的气泡台词。");
            return builder.ToString();
        }

        private static string GetEventInstruction(AICompanionEventType eventType)
        {
            switch (eventType)
            {
                case AICompanionEventType.HungryWarning:
                    return "倾向：自然表达想吃东西，但不要直接提饱食度。";
                case AICompanionEventType.LowEnergyWarning:
                    return "倾向：表达困倦、想休息或安静陪伴。";
                case AICompanionEventType.PetClicked:
                    return "倾向：表达被玩家互动后的轻松回应。";
                case AICompanionEventType.PetDragged:
                    return "倾向：表达被移动后的生活化反应。";
                case AICompanionEventType.IdleGreeting:
                    return "倾向：表达安静陪伴，不打扰玩家。";
                case AICompanionEventType.MorningGreeting:
                    return "倾向：表达早安和今天一起努力。";
                case AICompanionEventType.NightGreeting:
                    return "倾向：温柔提醒玩家注意休息。";
                default:
                    return "倾向：自然回应当前情境。";
            }
        }
    }
}
