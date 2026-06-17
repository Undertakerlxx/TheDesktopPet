using UnityEngine;

namespace DesktopPet.AICompanion
{
    public static class AICompanionFallbackLines
    {
        private static readonly string[] petClicked =
        {
            "嘿嘿，有点痒。",
            "我在这里哦。",
            "今天也想陪着你。",
            "摸摸就有精神啦。",
            "刚刚是在叫我吗？"
        };

        private static readonly string[] petDragged =
        {
            "换个位置也不错。",
            "这里好像更舒服。",
            "你把我搬来啦。",
            "新位置，新的心情。",
            "我会乖乖待在这里。"
        };

        private static readonly string[] hungryWarning =
        {
            "肚子有点空空的。",
            "想吃点好吃的。",
            "可以加餐吗？",
            "闻到饭香就好了。",
            "我好像有点饿啦。"
        };

        private static readonly string[] lowEnergyWarning =
        {
            "我想休息一会儿。",
            "有点困啦。",
            "陪你待着也很好。",
            "今天慢一点也没关系。",
            "让我眯一小会儿。"
        };

        private static readonly string[] idleGreeting =
        {
            "我在这里陪你。",
            "安静待着也很好。",
            "你忙吧，我不打扰。",
            "我会乖乖等你。",
            "今天也在你旁边。"
        };

        private static readonly string[] morningGreeting =
        {
            "早呀，今天也一起加油。",
            "新的一天开始啦。",
            "早安，我已经醒啦。",
            "今天也陪你努力。",
            "早上好，先伸个懒腰。"
        };

        private static readonly string[] nightGreeting =
        {
            "已经很晚啦，别太累哦。",
            "夜深了，要记得休息。",
            "我陪你，但别熬太久。",
            "今天辛苦啦。",
            "晚一点也要照顾自己。"
        };

        public static string GetLine(AICompanionEventType eventType, string lastLine)
        {
            string[] lines = GetLines(eventType);
            if (lines == null || lines.Length == 0)
            {
                return string.Empty;
            }

            string selected = lines[Random.Range(0, lines.Length)];
            if (lines.Length <= 1 || selected != lastLine)
            {
                return selected;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                string candidate = lines[(i + 1) % lines.Length];
                if (candidate != lastLine)
                {
                    return candidate;
                }
            }

            return selected;
        }

        private static string[] GetLines(AICompanionEventType eventType)
        {
            switch (eventType)
            {
                case AICompanionEventType.PetClicked:
                    return petClicked;
                case AICompanionEventType.PetDragged:
                    return petDragged;
                case AICompanionEventType.HungryWarning:
                    return hungryWarning;
                case AICompanionEventType.LowEnergyWarning:
                    return lowEnergyWarning;
                case AICompanionEventType.IdleGreeting:
                    return idleGreeting;
                case AICompanionEventType.MorningGreeting:
                    return morningGreeting;
                case AICompanionEventType.NightGreeting:
                    return nightGreeting;
                default:
                    return idleGreeting;
            }
        }
    }
}
