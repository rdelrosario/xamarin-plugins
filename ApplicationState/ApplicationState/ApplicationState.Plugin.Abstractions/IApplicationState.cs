using System;

namespace ApplicationState.Plugin.Abstractions
{
  /// <summary>
  /// Interface for ApplicationState
  /// </summary>
  public interface IApplicationState
  {
       bool IsBackground {get;}
       bool IsForeground {get;}
  }
}
