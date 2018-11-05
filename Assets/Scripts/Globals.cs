using System;
using System.Collections;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine;
using UniRx;
using UniRx.Diagnostics;

public class Globals : MonoBehaviour
{
    public bool CanDebug;
    //编辑器可以面板设置
#if UNITY_EDITOR
    public bool IsLockMap = true;
    //移动端根据独立版和植入版区分是否默认解锁关卡
#elif UNITY_MOBILE || UNITY_ANDROID || UNITY_IOS
#if _STANDALONE_MODE_
    public bool IsLockMap = false;
#else
    public bool IsLockMap = true;
#endif

#endif
    static Globals _instance;
    public static Globals Instance
    {
        get
        {
            return _instance;
        }
    }

#if UNITY_EDITOR
    UniRx.Diagnostics.Logger logger = new UniRx.Diagnostics.Logger("UnirxLog:");
#endif
 

    UIFrame.UIManager uiMgr;
    UIFrame.UIDataRepo uiDataRepo; 
    public event Action OnUpdateEvent;
    public event Action OnLateUpdateEvent;
    public event Action OnFixedUpdateEvent;
 

    public UIFrame.UIManager UIMgr
    {
        get
        {
            return uiMgr;
        }
    }
 
    public UIFrame.UIDataRepo UIDataRepo
    {
        get
        {
            return uiDataRepo;
        }
    }
     

    private void onUnhandledException(object sender, UnhandledExceptionEventArgs unhandledEx)
    {
        //var sepChar = System.IO.Path.DirectorySeparatorChar;
        //var outputPath = Application.temporaryCachePath +  sepChar ;
        var ex = unhandledEx.ExceptionObject as System.Exception;
        if (ex != null)
        {
            System.Text.StringBuilder stb = new System.Text.StringBuilder();
            stb.AppendLine(ex.Message);
            stb.AppendLine(ex.StackTrace);
            stb.AppendLine(ex.Source);
            if (ex.Data != null)
            {
                foreach (var key in ex.Data.Keys)
                {
                    var value = ex.Data[key];
                    stb.AppendLine(string.Concat(key, " ", value));
                }
            }
            var str = stb.ToString();
            try
            {
                Debug.Log("##error " + string.Concat(ex.Message));
                var data = System.Text.Encoding.UTF8.GetBytes(str);
            }
            catch (System.Exception e)
            {

            }

        }

    }


    private void Awake()
    {
        System.AppDomain.CurrentDomain.UnhandledException += onUnhandledException;

//#if UNITY_IOS
//        System.AppDomain.CurrentDomain.UnhandledException += (sender, unhandledEx) =>
//        {
//            Debug.Log("##error " + string.Concat(unhandledEx.ExceptionObject));
//        };
//#endif
#if _ENABLE_BUGLY_
        BuglyPlatforms buglyPlatforms = BuglyPlatforms.Create();
        buglyPlatforms.Init();
#endif

#if _STANDALONE_MODE_ || UNITY_ANDROID
        Screen.orientation = ScreenOrientation.Landscape;
#endif
        //非单机版本 不输出log到stdout
#if !_STANDALONE_MODE_ && !UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
        UnityEngine.Application.logMessageReceived += (string condition, string stackTrace, LogType type) => {
            
        };
#endif
        Application.targetFrameRate = 25;

        if(SystemInfo.batteryLevel > 0 && SystemInfo.batteryLevel < 0.6f){
            var curResolution = Screen.currentResolution;
            Screen.SetResolution(curResolution.width, curResolution.height, true, 30);

            UnityEngine.QualitySettings.vSyncCount = 2;
        }

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        _instance = this;

        GRoot.inst.SetContentScaleFactor(1920,1080,UIContentScaler.ScreenMatchMode.MatchWidthOrHeight);

    }



    private WaitForSeconds cachedWaitSeconds = new WaitForSeconds(0.3f);

    public IEnumerator Start()
    { 
        
        uiMgr = UIFrame.UIManager.Instance;
        uiDataRepo = UIFrame.UIDataRepo.Instance;

        yield return null;
    }

    private void Update()
    {
        if (OnUpdateEvent!=null)
        {
            OnUpdateEvent();
        } 

#if UNITY_ANDROID
        //DeviceManager 权限管理loop
        DeviceManager.AsyncTask.OnUpdate();
#endif
    }

    private void LateUpdate()
    {
        if (OnLateUpdateEvent != null)
        {
            OnLateUpdateEvent();
        }

        Timer.Timer.Update();

    }

    private void FixedUpdate()
    {
        if (OnFixedUpdateEvent != null)
        {
            OnFixedUpdateEvent();
        }
    }

    public void OnSingletonInit()
    {
#if UNITY_EDITOR
        //开启Unirx log 
        
        UniRx.Diagnostics.ObservableLogger.Listener.Subscribe((log) =>
        {
            Debug.unityLogger.Log(log.LogType, log.LoggerName, log.Message);
        });
        
#endif

    }

    [UnityEngine.RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void OnAppStart()
    {
        return;
        var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        var scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
        Debug.Log("$$ name:" + sceneName + " path:" + scenePath);

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/main.unity")
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Scenes/main", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

}