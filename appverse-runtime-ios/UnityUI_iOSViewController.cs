/*
 Copyright (c) 2012 GFT Appverse, S.L., Sociedad Unipersonal.

 This Source  Code Form  is subject to the  terms of  the Appverse Public License 
 Version 2.0  (“APL v2.0”).  If a copy of  the APL  was not  distributed with this 
 file, You can obtain one at http://appverse.org/legal/appverse-license/.

 Redistribution and use in  source and binary forms, with or without modification, 
 are permitted provided that the  conditions  of the  AppVerse Public License v2.0 
 are met.

 THIS SOFTWARE IS PROVIDED BY THE  COPYRIGHT HOLDERS  AND CONTRIBUTORS "AS IS" AND
 ANY EXPRESS  OR IMPLIED WARRANTIES, INCLUDING, BUT  NOT LIMITED TO,   THE IMPLIED
 WARRANTIES   OF  MERCHANTABILITY   AND   FITNESS   FOR A PARTICULAR  PURPOSE  ARE
 DISCLAIMED. EXCEPT IN CASE OF WILLFUL MISCONDUCT OR GROSS NEGLIGENCE, IN NO EVENT
 SHALL THE  COPYRIGHT OWNER  OR  CONTRIBUTORS  BE LIABLE FOR ANY DIRECT, INDIRECT,
 INCIDENTAL,  SPECIAL,   EXEMPLARY,  OR CONSEQUENTIAL DAMAGES  (INCLUDING, BUT NOT
 LIMITED TO,  PROCUREMENT OF SUBSTITUTE  GOODS OR SERVICES;  LOSS OF USE, DATA, OR
 PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT(INCLUDING NEGLIGENCE OR OTHERWISE) 
 ARISING  IN  ANY WAY OUT  OF THE USE  OF THIS  SOFTWARE,  EVEN  IF ADVISED OF THE 
 POSSIBILITY OF SUCH DAMAGE.
 */
using System;
using System.Drawing;
using Unity.Platform.IPhone;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Unity.Core.System;


namespace UnityUI.iOS
{
	public partial class UnityUI_iOSViewController : UIViewController
	{
		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public UnityUI_iOSViewController ()
			: base (UserInterfaceIdiomIsPhone ? "UnityUI_iOSViewController_iPhone" : "UnityUI_iOSViewController_iPad", null)
		{
			log ("Constructor default.");
			Initialize ();
		}

		private bool IsLogging = true;
		SplashScreenView splashView = null;
		bool splashscreenShownOnStartupTime = false;
		private bool orientationSupportedPortrait = false;
		private bool orientationSupportedPortraitUpsideDown = false;
		private bool orientationSupportedLandscapeLeft = false;
		private bool orientationSupportedLandscapeRight = false;
		private NSArray supportedOrientations = null;
		
		bool isTopController = true;
		
		#region Constructors
		
		// The IntPtr and initWithCoder constructors are required for controllers that need 
		// to be able to be created from a xib rather than from managed code
		
		public UnityUI_iOSViewController (IntPtr handle) : base(handle)
		{
			log ("Constructor with IntPrt.");
			Initialize ();
		}
		
		public UnityUI_iOSViewController (NSCoder coder) : base(coder)
		{
			log ("Constructor with NSCoder.");
			Initialize ();
		}
		
		void Initialize ()
		{
			log ("Initialize view");
			this.ShowSplashScreenOnStartupTime( UIApplication.SharedApplication.StatusBarOrientation);
			
			NSObject supportedOrientationsObj = NSBundle.MainBundle.ObjectForInfoDictionary ("CustomUISupportedInterfaceOrientations");
			if (supportedOrientationsObj != null) {
				if (supportedOrientationsObj is NSArray) {
					supportedOrientations = (NSArray)supportedOrientationsObj;
					
					for (uint index = 0; index < supportedOrientations.Count; index++) {
						NSString mySupportedOrientation = new NSString (supportedOrientations.ValueAt (index));
						if (("UIInterfaceOrientation" + UIInterfaceOrientation.Portrait.ToString ()) == mySupportedOrientation.ToString ()) {
							orientationSupportedPortrait = true;
						}
						if (("UIInterfaceOrientation" + UIInterfaceOrientation.PortraitUpsideDown.ToString ()) == mySupportedOrientation.ToString ()) {
							orientationSupportedPortraitUpsideDown = true;
						}
						if (("UIInterfaceOrientation" + UIInterfaceOrientation.LandscapeLeft.ToString ()) == mySupportedOrientation.ToString ()) {
							orientationSupportedLandscapeLeft = true;
						}
						if (("UIInterfaceOrientation" + UIInterfaceOrientation.LandscapeRight.ToString ()) == mySupportedOrientation.ToString ()) {
							orientationSupportedLandscapeRight = true;
						}
					}
					
				} 
			}
		}
		
		private void ShowSplashScreenOnStartupTime(UIInterfaceOrientation orientation) {
			
			if(!this.splashscreenShownOnStartupTime) {
				try {
					var mykey = NSBundle.MainBundle.ObjectForInfoDictionary("Unity_HoldSplashScreenOnStartup");
					bool showSplashScreenOnStartup = Convert.ToBoolean(Convert.ToInt32(""+mykey));
#if DEBUG
					log ("Show SplashScreen OnStartup? " + showSplashScreenOnStartup);
#endif
					if(showSplashScreenOnStartup)  {
						if (this.ShowSplashScreen (orientation)) this.splashscreenShownOnStartupTime = true;
					}
					
				} catch(Exception ex) {
#if DEBUG
					log ("Exception getting 'Unity_HoldSplashScreenOnStartup' from application preferences: " + ex.Message);
#endif
				}
			} else {
				UIInterfaceOrientation currentOrientation = UIApplication.SharedApplication.StatusBarOrientation;
				if(orientation != currentOrientation && this.splashView != null) {
					this.splashView.SetSplashViewForOrientation(orientation);
				}
			}
		}
		
		public bool ShowSplashScreen (UIInterfaceOrientation orientation)
		{
			if(this.splashView == null) {
#if DEBUG
				log ("Showing SplashScreen...");
#endif
				splashView = new SplashScreenView(orientation);
				
				// Show splash screen as top view
				this.View.AddSubview(splashView);
				this.View.BringSubviewToFront(splashView);
				return true;
			} else {
#if DEBUG
				log ("Not able to show the splash screen. Reason: it is already shown.");
#endif
				return false;
			}
		}
		
		public bool DismissSplashScreen ()
		{
#if DEBUG
			log ("Dismissing SplashScreen...");
#endif
			
			if(splashView != null) {
				splashView.RemoveFromSuperview();
				splashView.Dispose();
				splashView = null;
#if DEBUG
				log ("Splashscreen dismissed.");
#endif
				return true;
			}
			
#if DEBUG
			log ("Not able to dismiss the splash screen. Reason: it is not shown.");
#endif
			
			return false;
		}
		
		
		
		private void log (string message)
		{
			if (IsLogging) {
				SystemLogger.Log (SystemLogger.Module.GUI, "ViewController: " + message);
			}
		}
		
		public void loadWebView (string urlPath)
		{
			NSUrl url = new NSUrl (urlPath);
			//NSUrlRequest request = new NSUrlRequest(url, NSUrlRequestCachePolicy.ReturnCacheDataElseLoad, 3600.0);
			// FIX (16-09-2013) - testing iOS 7 apps; resources are not refreshed when installing a new app version on the top of a previous one
			// We need to remove the cache data loaded
			NSUrlRequest request = new NSUrlRequest(url		, NSUrlRequestCachePolicy.ReloadIgnoringLocalAndRemoteCacheData, 3600.0);

			if (webView != null) {
				this.webView.LoadRequest (request);
			} else {
				log ("WebView is null.");
			}
			
		}
#endregion
		
		/// DEPRECATED for iOS 6 and later, but needed for iOS 5 and earlier to support additional orientations
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			bool shouldAutorotate = IPhoneUtils.GetInstance ().ShouldAutorotate ();
			
			if (shouldAutorotate) {
				// Check supported orientations
				if (supportedOrientations != null) {
					bool orientationSupported = false;
					for (uint index = 0; index < supportedOrientations.Count; index++) {
						NSString mySupportedOrientation = new NSString (supportedOrientations.ValueAt (index));
						if (("UIInterfaceOrientation" + toInterfaceOrientation.ToString ()) == mySupportedOrientation.ToString ()) {
							orientationSupported = true;
							break;
						}
					}
				    shouldAutorotate = orientationSupported;
				} else {
					log ("Supported orientations not configured. All orientations are supported by default");
				}
			} else {
				log (" ** Autorotation blocked by application at runtime. ");
			}
			
			log ("Should Autorotate to " + toInterfaceOrientation.ToString () + "? " + shouldAutorotate);
			
			if(shouldAutorotate) this.ShowSplashScreenOnStartupTime(toInterfaceOrientation);
			
			return shouldAutorotate;
			
		}
		
		/// Available in iOS 6.0 and later.
		public override bool ShouldAutorotate ()
		{ 
			bool shouldAutorotate = IPhoneUtils.GetInstance ().ShouldAutorotate ();
			
			if (!shouldAutorotate) {
				log (" ** Autorotation blocked by application at runtime. ");
			}
		
			log("ShouldAutorotate? " + shouldAutorotate);
			if (shouldAutorotate) {
				if(this.splashscreenShownOnStartupTime) {
					UIInterfaceOrientation currentOrientation = UIApplication.SharedApplication.StatusBarOrientation;
					if (this.splashView != null) {
						log ("Adjusting splashscreen to current orientation: " + currentOrientation);
						this.splashView.SetSplashViewForOrientation (currentOrientation);
					}
				}
			}

			return shouldAutorotate;
		}
		
		/// Available in iOS 6.0 and later.
		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations ()
		{
			UIInterfaceOrientationMask supportedOrientationMask = UIInterfaceOrientationMask.All;
			
			if(orientationSupportedPortrait && orientationSupportedLandscapeLeft && orientationSupportedLandscapeRight && !orientationSupportedPortraitUpsideDown)  {
				supportedOrientationMask = UIInterfaceOrientationMask.AllButUpsideDown;
			} else {
				if(orientationSupportedPortrait && !orientationSupportedLandscapeLeft && !orientationSupportedLandscapeRight && !orientationSupportedPortraitUpsideDown) {
					supportedOrientationMask = UIInterfaceOrientationMask.Portrait;
				}
				
				if(orientationSupportedPortrait && !orientationSupportedLandscapeLeft && !orientationSupportedLandscapeRight && orientationSupportedPortraitUpsideDown) {
					supportedOrientationMask = UIInterfaceOrientationMask.Portrait;
				}
				
				if(!orientationSupportedPortrait && !orientationSupportedLandscapeLeft && !orientationSupportedLandscapeRight && orientationSupportedPortraitUpsideDown) {
					supportedOrientationMask = UIInterfaceOrientationMask.PortraitUpsideDown;
				}
				
				if(!orientationSupportedPortrait && orientationSupportedLandscapeLeft && !orientationSupportedLandscapeRight && !orientationSupportedPortraitUpsideDown) {
					supportedOrientationMask = UIInterfaceOrientationMask.LandscapeLeft;
				}
				
				if(!orientationSupportedPortrait && !orientationSupportedLandscapeLeft && orientationSupportedLandscapeRight && !orientationSupportedPortraitUpsideDown) {
					supportedOrientationMask = UIInterfaceOrientationMask.LandscapeRight;
				}
				
				if(!orientationSupportedPortrait && orientationSupportedLandscapeLeft && orientationSupportedLandscapeRight && !orientationSupportedPortraitUpsideDown) {
					supportedOrientationMask = UIInterfaceOrientationMask.Landscape;
				}
			}
			
			log("SupportedInterfaceOrientations: " + supportedOrientationMask);
			return supportedOrientationMask;
			
		}
		
		public override bool ShouldAutomaticallyForwardRotationMethods {
			get {
				log ("ShouldAutomaticallyForwardRotationMethods true");
				//return base.ShouldAutomaticallyForwardRotationMethods;
				return true;
			}
		}
		
		/*
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
		}
		*/
		
		public override void WillRotate (UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillRotate (toInterfaceOrientation, duration);
			
			log ("WillRotate to Interface Orientation: " + toInterfaceOrientation);
			switch (toInterfaceOrientation) {
			case UIInterfaceOrientation.Portrait:
				webView.EvaluateJavascript (@"window.__defineGetter__('orientation',function(){return 0;});window.onorientationchange();");
				break;
			case UIInterfaceOrientation.LandscapeLeft:
				webView.EvaluateJavascript (@"window.__defineGetter__('orientation',function(){return 90;});window.onorientationchange();");
				break;
			case UIInterfaceOrientation.LandscapeRight:
				webView.EvaluateJavascript (@"window.__defineGetter__('orientation',function(){return -90;});window.onorientationchange();");
				break;
			case UIInterfaceOrientation.PortraitUpsideDown:
				webView.EvaluateJavascript (@"window.__defineGetter__('orientation',function(){return 180;});window.onorientationchange();");
				break;
			default:
				break;
			}
			
			this.ShowSplashScreenOnStartupTime(toInterfaceOrientation);
			
			//webView.EvaluateJavascript(@"window.onorientationchange();");
			
		}
		
		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate (fromInterfaceOrientation);
			
			log ("DidRotate from Interface Orientation: " + fromInterfaceOrientation);
			webView.EvaluateJavascript (@"refreshOrientation();");
			//webView.EvaluateJavascript(@"window.onorientationchange();");
		}

		public override void ViewWillLayoutSubviews () {
			log ("ViewWillLayoutSubviews");
			if(!this.isTopController) {
				log ("ViewWillLayoutSubviews: main UIViewController is NOT the top controller... executing refreshOrientation() JS function");
				webView.EvaluateJavascript (@"refreshOrientation();");
				this.isTopController = true;
			}
		}

		public void SetAsTopController(bool topController) {
			this.isTopController = topController; 
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.
		}

		/*
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			// Clear any references to subviews of the main view in order to
			// allow the Garbage Collector to collect them sooner.
			//
			// e.g. myOutlet.Dispose (); myOutlet = null;
			
			ReleaseDesignerOutlets ();
		}
		*/
	}
}

