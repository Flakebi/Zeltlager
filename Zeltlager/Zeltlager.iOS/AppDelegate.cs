using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Zeltlager.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
			UITableView.Appearance.SeparatorInset = UIEdgeInsets.Zero;
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

			// Get UIImage with a green color fill
			CGRect rect = new CGRect(0, 0, 1, 1);
			CGSize size = rect.Size;
			UIGraphics.BeginImageContext(size);
			CGContext currentContext = UIGraphics.GetCurrentContext();
			currentContext.SetFillColor(1, 0, 1, 1);
			currentContext.FillRect(rect);
			var backgroundImage = UIGraphics.GetImageFromCurrentImageContext();
			currentContext.Dispose();

			// This is the assembly full name which may vary by the Xamarin.Forms version installed.
			// NullReferenceException is raised if the full name is not correct.
			var t = Type.GetType("Xamarin.Forms.Platform.iOS.ContextActionsCell, Xamarin.Forms.Platform.iOS, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

			// Now change the static field value!
			var field = t.GetField("DestructiveBackground", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			field.SetValue(null, backgroundImage);

            return base.FinishedLaunching(app, options);
        }
    }
}
