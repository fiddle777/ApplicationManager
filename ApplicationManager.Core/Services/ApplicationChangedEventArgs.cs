using System;
using ApplicationManager.Core.Models;

namespace ApplicationManager.Core.Services
{
	public sealed class ApplicationChangedEventArgs : EventArgs
	{
		public string Action { get; }
		public Application Application { get; }

		public ApplicationChangedEventArgs(string action, Application application)
		{
			Action = action;
			Application = application;
		}
	}
}
