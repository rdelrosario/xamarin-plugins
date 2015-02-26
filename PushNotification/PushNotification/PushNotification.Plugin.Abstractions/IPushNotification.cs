using System;
using System.Collections.Generic;

namespace PushNotification.Plugin.Abstractions
{
  /// <summary>
  /// Interface for PushNotification
  /// </summary>
  public interface IPushNotification
  {

	  string Token { get; }
      void Register();
      void Unregister();

 
  }

}
