//#define _Q_EVENT_SYSTEM_
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFrame
{
#if _Q_EVENT_SYSTEM_
    public struct EventSubscription : System.IDisposable
    {
        UserEvents.Events eventKey;
        QFramework.OnEvent onEvent;

        public EventSubscription(UserEvents.Events eventKey, QFramework.OnEvent onEvent)
        {
            this.eventKey = eventKey;
            this.onEvent = onEvent;
        }

        public void Dispose()
        {
            QFramework.QEventSystem.UnRegisterEvent<UserEvents.Events>(eventKey, onEvent);
        }
    }
#endif
}
