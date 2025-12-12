using Foundation;
using UIKit;

namespace RoMoteNoAds.Platforms.MacCatalyst;

[Register("SceneDelegate")]
public class SceneDelegate : MauiUISceneDelegate
{
    public override UIWindow? Window { get; set; }

    public override void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
    {
        base.WillConnect(scene, session, connectionOptions);

        if (scene is UIWindowScene windowScene)
        {
            // Set up the window
            Window = new KeyboardWindow(windowScene);
        }
    }
}

/// <summary>
/// Custom UIWindow that intercepts keyboard events.
/// </summary>
public class KeyboardWindow : UIWindow
{
    public KeyboardWindow(UIWindowScene windowScene) : base(windowScene)
    {
    }

    public override void PressesBegan(NSSet<UIPress> presses, UIPressesEvent evt)
    {
        foreach (UIPress press in presses)
        {
            if (press.Key != null)
            {
                var keyCode = press.Key.KeyCode;
                var characters = press.Key.Characters;
                var modifiers = press.Key.ModifierFlags;

                // Raise the event
                KeyboardEventService.RaiseKeyPressed(keyCode, characters, modifiers);
            }
        }

        base.PressesBegan(presses, evt);
    }
}
