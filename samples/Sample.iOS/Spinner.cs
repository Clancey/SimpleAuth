using System;

namespace Sample.iOS
{
	public class Spinner : IDisposable
	{
		public Spinner (string title)
		{
			BigTed.BTProgressHUD.ShowContinuousProgress (title, BigTed.ProgressHUD.MaskType.Clear);
		}

		#region IDisposable implementation

		public void Dispose ()
		{
			BigTed.BTProgressHUD.Dismiss ();
		}

		#endregion
	}
}

