using System;

namespace QRStudio.Engine.ExceptionHandler
{
	[Serializable]
	public class AlignmentPatternNotFoundException : ArgumentException
    {
        internal string message = null;

		public override string Message
		{
			get
			{
				return message;
			}
			
		}		
		public AlignmentPatternNotFoundException(string message)
		{
			this.message = message;
		}
	}
}