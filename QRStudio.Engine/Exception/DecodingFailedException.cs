using System;
namespace QRStudio.Engine.ExceptionHandler
{	
	[Serializable]
	public class DecodingFailedException:System.ArgumentException
	{
        internal string message = null;

		public override string Message
		{
			get
			{
				return message;
			}
			
		}
		
		public DecodingFailedException(string message)
		{
			this.message = message;
		}
	}
}