using System;
using DesktopPet.Storage;
using UnityEngine;

namespace DesktopPet.Timer
{
    public class WorkTimer
    {
        public bool IsTiming { get; private set; }
        public float ElapsedSeconds { get; private set; }
        public float EstimatedSeconds { get; private set; }
        public string CurrentTask { get; private set; }
        public bool IsOvertime => EstimatedSeconds > 0f && ElapsedSeconds > EstimatedSeconds;

        private float startTime;

        public void Start(string task)
        {
            CurrentTask = string.IsNullOrWhiteSpace(task) ? "未命名事项" : task.Trim();
            startTime = Time.time;
            ElapsedSeconds = 0f;
            IsTiming = true;
        }

        public TimerHistoryRecord Finish()
        {
            Tick();
            IsTiming = false;

            return new TimerHistoryRecord
            {
                task = CurrentTask,
                elapsedSeconds = ElapsedSeconds,
                finishedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }

        public void Tick()
        {
            if (IsTiming)
            {
                ElapsedSeconds = Time.time - startTime;
            }
        }

        public void SetEstimate(int hours, int minutes, int seconds)
        {
            minutes = Mathf.Clamp(minutes, 0, 59);
            seconds = Mathf.Clamp(seconds, 0, 59);
            EstimatedSeconds = Mathf.Max(0f, hours * 3600f + minutes * 60f + seconds);
        }

        public static string FormatDuration(float seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(Mathf.Max(0f, seconds));
            return $"{(int)timeSpan.TotalHours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
        }
    }
}
