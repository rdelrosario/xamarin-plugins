using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushNotification.Plugin.Abstractions
{
    public static class  PushNotificationKey
    {
      public const string Type = "type";
      public const string Error = "error";
	  public const string DomainName = "CrossPushNotification";
      public const string Title = "title";
      public const string Text = "text";
      public const string Subtitle = "subtitle";
	  public const string Message = "message";
      public const string Silent = "silent";
	  public const string Token = "token";
	  public const string AppVersion = "appVersion";
      public const string IntentFromGcmMessage = "com.google.android.c2dm.intent.RECEIVE";
    }
}
