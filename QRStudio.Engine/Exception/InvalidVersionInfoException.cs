using System;
namespace QRStudio.Engine.ExceptionHandler
{
	[Serializable]
	public class InvalidVersionInfoException:VersionInformationException
	{
        internal string message = null;
		public override string Message
		{
			get
			{
				return message;
			}
			
		}
		
		public InvalidVersionInfoException(string message)
		{
			this.message = message;
		}
	}
}