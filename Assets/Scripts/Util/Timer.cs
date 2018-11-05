using UnityEngine;
using System;
using System.Collections.Generic;

namespace Timer
{
    public enum TimerListerType
    {
        NULL,
        TIMER_POURTIME,
    }
    public class TimeListener : IDisposable
    {
        public int sort = 0;
        public DateTime m_triggerTime;
        public System.Action m_handler;
        public TimerListerType m_timerType;

        //public TimeListener(System.Action callBackHandler, float fTriggerTime, TimerListerType type)
        //{
        //    m_timerType = type;
        //    m_handler = callBackHandler;
        //    m_triggerTime = TimeMgr.CurrentDateTime.AddSeconds(fTriggerTime);
        //}
        public TimeListener(System.Action callBackHandler, DateTime fTriggerTime, TimerListerType type)
        {
            m_timerType = type;
            m_handler = callBackHandler;
            if (fTriggerTime > Timer.CurrentDateTime())
                m_triggerTime = fTriggerTime;
            else
                Debug.LogWarning("Time is expired");
        }

        public void Dispose()
        {
            Timer.RemoveListener(this);
        }

        public void AddTo(ICollection<IDisposable> container )
        {
            container.Add(this);
        }
    }

    public static class Timer
    {
        //public static int ServerTime { get { return TimeMgr.CurrentTime; } }

        private static List<TimeListener> m_listTimeListener = new List<TimeListener>();

        /* 排序时间 **/
        private static void sortTimeListener()
        {
            m_listTimeListener.Sort(delegate(TimeListener a, TimeListener b)
            {
                if (a.m_triggerTime < b.m_triggerTime)
                    return -1;
                else if (a.m_triggerTime > b.m_triggerTime)
                    return 1;
                else if (a.sort < b.sort)
                    return -1;
                else if (a.sort > b.sort)
                    return 1;
                else
                    return 0;
            });
            m_listTimeListener.Reverse();
        }

        /* 相同时间的 根据添加顺序调用 **/
        private static TimeListener adjustEquilTime(TimeListener dateTime)
        {
            for (int i = 0, count = m_listTimeListener.Count; i < count; i++)
            {
                TimeListener listener = m_listTimeListener[i];
                if (dateTime.m_triggerTime == listener.m_triggerTime)
                {
                    dateTime.sort = listener.sort + 1;
                    return dateTime;
                }
            }
            return dateTime;
        }

        public static bool Contain(TimeListener listener)
        {
            return m_listTimeListener.Contains(listener);
        }

        public static TimeListener AddTimeListener(System.Action callBackHandler, float fTriggerTime, TimerListerType type = TimerListerType.NULL)
        {
            DateTime m_triggerTime = CurrentDateTime().AddSeconds(fTriggerTime);
            TimeListener timeListener = new TimeListener(callBackHandler, m_triggerTime, type);
            m_listTimeListener.Add(adjustEquilTime(timeListener));
            sortTimeListener();
            return timeListener;
        }
        public static TimeListener AddTimeListener(System.Action callBackHandler, DateTime dateTime, TimerListerType type = TimerListerType.NULL)
        {
            TimeListener timeListener = new TimeListener(callBackHandler, dateTime, type);
            m_listTimeListener.Add(adjustEquilTime(timeListener));
            sortTimeListener();
            return timeListener;
        }

        /** 移除定时器 */
        public static void RemoveListener(TimeListener target)
        {
            m_listTimeListener.Remove(target);
        }

        public static void DeleteTimeListener(TimerListerType type)
        {
            TimeListener timeLlister = m_listTimeListener.Find(delegate(TimeListener timer) { return timer.m_timerType == type; });
            m_listTimeListener.Remove(timeLlister);
            sortTimeListener();
        }
        public static void Update()
        {
            //事件监听的时时检测
            for (int i = m_listTimeListener.Count - 1; i >= 0; i--)
            {
                TimeListener timeListener = m_listTimeListener[i];
                
                if(timeListener == null)
                    continue;
                
                if (CurrentDateTime() >= timeListener.m_triggerTime)
                {
                    if (timeListener.m_handler != null)
                    {
                        m_listTimeListener.RemoveAt(i);
                        timeListener.m_handler();
                    }

                }
            }
            
        }

        //TODO 替换服务器时间
        public static System.DateTime CurrentDateTime()
        {
            return System.DateTime.Now;
        }

        //同步服务器时间
        [Obsolete("服务器时间同步在TimeMgr里面完成 Timer只作为定时器")]
        public static void SetServeTime(int nServerTime)
        {

        }
    }
}