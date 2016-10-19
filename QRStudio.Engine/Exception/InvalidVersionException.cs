using System;
namespace QRStudio.Engine.ExceptionHandler
{
	[Serializable]
	public class InvalidVersionException:VersionInformationException
	{
        internal string message;
		public override string Message
		{
			get
			{
				return message;
			}
			
		}
		
		public InvalidVersionException(string message)
		{
			this.message = message;
		}
	}
}