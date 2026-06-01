using System.Collections.Generic;
using UnityEngine;

namespace DesktopPet.Accounts
{
    public class LoginPanelController : MonoBehaviour
    {
        private const float PanelWidth = 620f;
        private const float PanelHeight = 560f;

        private LoginBootstrap bootstrap;
        private AccountService accountService;
        private string username = string.Empty;
        private string password = string.Empty;
        private string message = string.Empty;
        private bool rememberLogin = true;
        private Vector2 accountListScroll;
        private Font uiFont;
        private GUIStyle titleStyle;
        private GUIStyle hintStyle;
        private GUIStyle messageStyle;
        private GUIStyle labelStyle;
        private GUIStyle textFieldStyle;
        private GUIStyle buttonStyle;
        private GUIStyle toggleStyle;
        private GUIStyle accountButtonStyle;

        public void Initialize(LoginBootstrap bootstrap, AccountService accountService)
        {
            this.bootstrap = bootstrap;
            this.accountService = accountService;
        }

        private void OnGUI()
        {
            if (!enabled || accountService == null)
            {
                return;
            }

            EnsureStyles();

            Rect panelRect = new(
                (Screen.width - PanelWidth) * 0.5f,
                (Screen.height - PanelHeight) * 0.5f,
                PanelWidth,
                PanelHeight);

            GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), string.Empty);
            GUILayout.BeginArea(panelRect, GUI.skin.window);
            GUILayout.Label("\u672c\u5730\u8d26\u53f7\u767b\u5f55", titleStyle);
            GUILayout.Space(14f);

            if (accountService.HasPendingLegacyImport())
            {
                GUILayout.Label("\u68c0\u6d4b\u5230\u65e7\u7248\u672c\u5730\u5b58\u6863\uff0c\u7b2c\u4e00\u4e2a\u6ce8\u518c\u8d26\u53f7\u4f1a\u81ea\u52a8\u5bfc\u5165\u8fd9\u4e9b\u6570\u636e\u3002", hintStyle);
                GUILayout.Space(10f);
            }

            GUILayout.Label("\u7528\u6237\u540d", labelStyle);
            username = GUILayout.TextField(username ?? string.Empty, 32, textFieldStyle, GUILayout.Height(34f));
            GUILayout.Space(10f);

            GUILayout.Label("\u5bc6\u7801", labelStyle);
            password = GUILayout.PasswordField(password ?? string.Empty, '*', 32, textFieldStyle, GUILayout.Height(34f));
            GUILayout.Space(10f);

            rememberLogin = GUILayout.Toggle(rememberLogin, "\u8bb0\u4f4f\u767b\u5f55", toggleStyle);
            GUILayout.Space(14f);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("\u767b\u5f55", buttonStyle, GUILayout.Height(42f)))
            {
                TryLogin();
            }

            if (GUILayout.Button("\u6ce8\u518c", buttonStyle, GUILayout.Height(42f)))
            {
                TryRegister();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("\u5220\u9664\u8d26\u53f7", buttonStyle, GUILayout.Height(38f)))
            {
                TryDeleteAccount();
            }

            if (GUILayout.Button("\u9000\u51fa\u6e38\u620f", buttonStyle, GUILayout.Height(38f)))
            {
                QuitGame();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(12f);
            GUILayout.Label(string.IsNullOrEmpty(message) ? "\u8bf7\u8f93\u5165\u8d26\u53f7\u548c\u5bc6\u7801\u3002" : message, messageStyle);
            GUILayout.Space(14f);

            GUILayout.Label("\u5df2\u6709\u8d26\u53f7", labelStyle);
            accountListScroll = GUILayout.BeginScrollView(accountListScroll, GUILayout.Height(150f));
            List<AccountRecord> accounts = accountService.GetAllAccounts();
            if (accounts.Count == 0)
            {
                GUILayout.Label("\u6682\u65e0\u672c\u5730\u8d26\u53f7\u3002", hintStyle);
            }
            else
            {
                foreach (AccountRecord account in accounts)
                {
                    if (GUILayout.Button(account.username, accountButtonStyle, GUILayout.Height(32f)))
                    {
                        username = account.username;
                    }
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            Event currentEvent = Event.current;
            if (currentEvent != null && currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Return)
            {
                TryLogin();
            }
        }

        private void EnsureStyles()
        {
            if (uiFont == null)
            {
                uiFont = CreatePreferredFont();
            }

            if (titleStyle != null)
            {
                return;
            }

            titleStyle = CreateStyle(GUI.skin.label, 24, TextAnchor.MiddleCenter);
            titleStyle.fontStyle = FontStyle.Bold;

            hintStyle = CreateStyle(GUI.skin.label, 16, TextAnchor.MiddleLeft);
            hintStyle.wordWrap = true;

            messageStyle = CreateStyle(GUI.skin.label, 16, TextAnchor.MiddleLeft);
            messageStyle.wordWrap = true;

            labelStyle = CreateStyle(GUI.skin.label, 16, TextAnchor.MiddleLeft);

            textFieldStyle = CreateStyle(GUI.skin.textField, 16, TextAnchor.MiddleLeft);
            textFieldStyle.padding = new RectOffset(10, 10, 8, 8);

            buttonStyle = CreateStyle(GUI.skin.button, 16, TextAnchor.MiddleCenter);
            buttonStyle.fontStyle = FontStyle.Bold;

            toggleStyle = CreateStyle(GUI.skin.toggle, 15, TextAnchor.MiddleLeft);

            accountButtonStyle = CreateStyle(GUI.skin.button, 15, TextAnchor.MiddleLeft);
            accountButtonStyle.padding = new RectOffset(12, 12, 6, 6);
        }

        private GUIStyle CreateStyle(GUIStyle baseStyle, int fontSize, TextAnchor alignment)
        {
            GUIStyle style = new(baseStyle)
            {
                font = uiFont,
                fontSize = fontSize,
                alignment = alignment
            };

            return style;
        }

        private static Font CreatePreferredFont()
        {
            string[] preferredFonts =
            {
                "Microsoft YaHei UI",
                "Microsoft YaHei",
                "Microsoft JhengHei UI",
                "Microsoft JhengHei",
                "SimHei",
                "SimSun",
                "Arial Unicode MS"
            };

            Font font = Font.CreateDynamicFontFromOSFont(preferredFonts, 18);
            if (font != null)
            {
                return font;
            }

            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private void TryLogin()
        {
            AccountLoginResult result = accountService.Login(username, password, rememberLogin, out string loginMessage);
            message = loginMessage;
            if (result == AccountLoginResult.Success)
            {
                bootstrap?.HandleLoginSuccess();
            }
        }

        private void TryRegister()
        {
            AccountRegisterResult result = accountService.Register(username, password, rememberLogin, out string registerMessage);
            message = registerMessage;
            if (result == AccountRegisterResult.Success)
            {
                bootstrap?.HandleLoginSuccess();
            }
        }

        private void TryDeleteAccount()
        {
            if (accountService.DeleteAccount(username, password, out string deleteMessage))
            {
                password = string.Empty;
            }

            message = deleteMessage;
        }

        private static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
