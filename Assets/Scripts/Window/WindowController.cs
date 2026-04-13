using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// 在 Windows 平台上将 Unity 独立窗口配置为桌宠窗口。
/// </summary>
/// <remarks>
/// 该组件主要负责以下几件事：
/// <list type="bullet">
/// <item><description>准备用于透明背景的相机设置。</description></item>
/// <item><description>移除标准窗口边框和标题栏。</description></item>
/// <item><description>启用分层窗口透明效果。</description></item>
/// <item><description>设置窗口的初始位置与尺寸。</description></item>
/// </list>
/// </remarks>
public class WindowController : MonoBehaviour
{
    [Header("Window")]
    [SerializeField] private int windowWidth = 512;
    [SerializeField] private int windowHeight = 512;
    [SerializeField] private int screenMarginRight = 80;
    [SerializeField] private int screenMarginBottom = 120;
    [SerializeField] private bool topmost = true;

    [Header("Transparency")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool useColorKeyFallback = false;
    [SerializeField] private Color transparencyKey = new Color(1f, 0f, 1f, 1f);

    [Header("Debug")]
    [SerializeField] private bool allowEscapeToQuit = true;

    private IntPtr _hwnd = IntPtr.Zero;
    private bool _initialized;

    /// <summary>
    /// 在场景开始运行后启动窗口初始化流程。
    /// </summary>
    private void Start()
    {
        Debug.Log("WindowController Start");
        Application.runInBackground = true;
        StartCoroutine(InitializeWindowCoroutine());
    }

    /// <summary>
    /// 处理调试阶段的运行时输入，例如按 <see cref="KeyCode.Escape"/> 退出程序。
    /// </summary>
    private void Update()
    {
        if (!allowEscapeToQuit || !Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// 等待独立窗口准备完成后，再应用原生窗口配置。
    /// </summary>
    /// <returns>
    /// 供 Unity 协程系统调度的枚举器。
    /// </returns>
    private IEnumerator InitializeWindowCoroutine()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        yield return null;
        yield return new WaitForEndOfFrame();

        const int maxAttempts = 20;

        for (int i = 0; i < maxAttempts; i++)
        {
            _hwnd = GetUnityWindowHandle();

            if (_hwnd != IntPtr.Zero)
            {
                InitializeWindow(_hwnd);
                _initialized = true;
                Debug.Log($"WindowController: acquired window handle {_hwnd}.");
                yield break;
            }

            yield return new WaitForSeconds(0.1f);
        }

        Debug.LogWarning("WindowController: failed to get window handle.");
#else
        Debug.Log("WindowController: editor mode, skip native window init.");
        yield break;
#endif
    }

    /// <summary>
    /// 对指定窗口应用全部原生配置步骤。
    /// </summary>
    /// <param name="hwnd">Unity 窗口的原生句柄。</param>
    private void InitializeWindow(IntPtr hwnd)
    {
        PrepareTransparencyCamera();
        ApplyBorderlessStyle(hwnd);
        ApplyTransparentStyle(hwnd);
        SetWindowSizeAndPosition(hwnd, CalculateWindowX(), CalculateWindowY(), windowWidth, windowHeight);
        Debug.Log("WindowController: native window initialized.");
    }

    /// <summary>
    /// 尝试定位 Unity 独立窗口的句柄。
    /// </summary>
    /// <returns>
    /// 找到时返回原生窗口句柄；否则返回 <see cref="IntPtr.Zero"/>。
    /// </returns>
    private IntPtr GetUnityWindowHandle()
    {
        IntPtr hwnd = GetActiveWindow();
        if (hwnd != IntPtr.Zero)
        {
            return hwnd;
        }

        return FindWindow(null, Application.productName);
    }

    /// <summary>
    /// 配置目标相机，使场景背景能够被当作透明区域处理。
    /// </summary>
    private void PrepareTransparencyCamera()
    {
        Camera cameraToUse = targetCamera != null ? targetCamera : Camera.main;
        if (cameraToUse == null)
        {
            Debug.LogWarning("WindowController: no camera found for transparency setup.");
            return;
        }

        cameraToUse.clearFlags = CameraClearFlags.SolidColor;
        cameraToUse.backgroundColor = useColorKeyFallback ? transparencyKey : new Color(0f, 0f, 0f, 0f);
    }

    /// <summary>
    /// 移除目标窗口的标准标题栏与可缩放边框。
    /// </summary>
    /// <param name="hwnd">Unity 窗口的原生句柄。</param>
    private void ApplyBorderlessStyle(IntPtr hwnd)
    {
        long style = GetWindowLongPtr(hwnd, GWL_STYLE).ToInt64();
        style &= ~(WS_CAPTION | WS_THICKFRAME | WS_SYSMENU | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
        style |= WS_POPUP | WS_VISIBLE;

        SetWindowLongPtr(hwnd, GWL_STYLE, new IntPtr(style));
    }

    /// <summary>
    /// 为目标窗口启用分层透明效果。
    /// </summary>
    /// <param name="hwnd">Unity 窗口的原生句柄。</param>
    private void ApplyTransparentStyle(IntPtr hwnd)
    {
        long exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE).ToInt64();
        exStyle |= WS_EX_LAYERED | WS_EX_TOOLWINDOW;
        exStyle &= ~WS_EX_APPWINDOW;

        SetWindowLongPtr(hwnd, GWL_EXSTYLE, new IntPtr(exStyle));
        if (useColorKeyFallback)
        {
            SetLayeredWindowAttributes(hwnd, ToColorRef(transparencyKey), 255, LWA_COLORKEY | LWA_ALPHA);
        }
        else
        {
            SetLayeredWindowAttributes(hwnd, 0, 255, LWA_ALPHA);
        }

        try
        {
            MARGINS margins = new MARGINS { cxLeftWidth = -1 };
            DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }
        catch (DllNotFoundException)
        {
            Debug.Log("WindowController: dwmapi.dll unavailable, skipping frame extension.");
        }
        catch (EntryPointNotFoundException)
        {
            Debug.Log("WindowController: DWM frame extension unavailable on this system.");
        }
    }

    /// <summary>
    /// 在样式修改完成后，设置窗口的位置、尺寸和 Z 轴层级。
    /// </summary>
    /// <param name="hwnd">Unity 窗口的原生句柄。</param>
    /// <param name="x">目标窗口的屏幕 X 坐标。</param>
    /// <param name="y">目标窗口的屏幕 Y 坐标。</param>
    /// <param name="width">目标窗口宽度。</param>
    /// <param name="height">目标窗口高度。</param>
    private void SetWindowSizeAndPosition(IntPtr hwnd, int x, int y, int width, int height)
    {
        IntPtr insertAfter = topmost ? HWND_TOPMOST : HWND_NOTOPMOST;
        SetWindowPos(hwnd, insertAfter, x, y, width, height, SWP_FRAMECHANGED | SWP_SHOWWINDOW);
    }

    /// <summary>
    /// 根据当前显示器宽度和配置的右边距，计算窗口初始 X 坐标。
    /// </summary>
    /// <returns>窗口的初始屏幕 X 坐标。</returns>
    private int CalculateWindowX()
    {
        int screenWidth = GetSystemMetrics(SM_CXSCREEN);
        return Mathf.Max(0, screenWidth - windowWidth - screenMarginRight);
    }

    /// <summary>
    /// 根据当前显示器高度和配置的下边距，计算窗口初始 Y 坐标。
    /// </summary>
    /// <returns>窗口的初始屏幕 Y 坐标。</returns>
    private int CalculateWindowY()
    {
        int screenHeight = GetSystemMetrics(SM_CYSCREEN);
        return Mathf.Max(0, screenHeight - windowHeight - screenMarginBottom);
    }

    /// <summary>
    /// 将 Unity 的 <see cref="Color"/> 转换为 Win32 所需的 COLORREF 值。
    /// </summary>
    /// <param name="color">要转换的 Unity 颜色。</param>
    /// <returns>按 0x00BBGGRR 编码的 COLORREF 值。</returns>
    private static uint ToColorRef(Color color)
    {
        Color32 color32 = color;
        return (uint)(color32.r | (color32.g << 8) | (color32.b << 16));
    }

    /// <summary>
    /// 读取窗口的长整型属性，并同时兼容 32 位与 64 位进程。
    /// </summary>
    /// <param name="hWnd">目标窗口句柄。</param>
    /// <param name="nIndex">要读取的窗口属性索引。</param>
    /// <returns>读取到的窗口长整型属性值。</returns>
    private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
    {
        return IntPtr.Size == 8
            ? GetWindowLongPtr64(hWnd, nIndex)
            : new IntPtr(GetWindowLong32(hWnd, nIndex));
    }

    /// <summary>
    /// 写入窗口的长整型属性，并同时兼容 32 位与 64 位进程。
    /// </summary>
    /// <param name="hWnd">目标窗口句柄。</param>
    /// <param name="nIndex">要写入的窗口属性索引。</param>
    /// <param name="dwNewLong">新的属性值。</param>
    /// <returns>写入前的旧属性值。</returns>
    private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        return IntPtr.Size == 8
            ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
            : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
    }

    // Win32 原生互操作函数声明。
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMargins);

    // 与 GetWindowLongPtr / SetWindowLongPtr 搭配使用的窗口属性索引。
    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;

    // 标准窗口样式。
    private const long WS_CAPTION = 0x00C00000L;
    private const long WS_THICKFRAME = 0x00040000L;
    private const long WS_SYSMENU = 0x00080000L;
    private const long WS_MINIMIZEBOX = 0x00020000L;
    private const long WS_MAXIMIZEBOX = 0x00010000L;
    private const long WS_POPUP = unchecked((int)0x80000000);
    private const long WS_VISIBLE = 0x10000000L;

    // 扩展窗口样式。
    private const long WS_EX_LAYERED = 0x00080000L;
    private const long WS_EX_TOOLWINDOW = 0x00000080L;
    private const long WS_EX_APPWINDOW = 0x00040000L;

    // 分层窗口标记。
    private const uint LWA_COLORKEY = 0x00000001;
    private const uint LWA_ALPHA = 0x00000002;

    // SetWindowPos 使用的标记。
    private const uint SWP_SHOWWINDOW = 0x0040;
    private const uint SWP_FRAMECHANGED = 0x0020;

    // 屏幕尺寸索引。
    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;

    // SetWindowPos 常用的 Z 序句柄。
    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

    /// <summary>
    /// 表示 <c>DwmExtendFrameIntoClientArea</c> 使用的边框扩展参数。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct MARGINS
    {
        /// <summary>左侧边框扩展宽度。</summary>
        public int cxLeftWidth;

        /// <summary>右侧边框扩展宽度。</summary>
        public int cxRightWidth;

        /// <summary>顶部边框扩展高度。</summary>
        public int cyTopHeight;

        /// <summary>底部边框扩展高度。</summary>
        public int cyBottomHeight;
    }
}
