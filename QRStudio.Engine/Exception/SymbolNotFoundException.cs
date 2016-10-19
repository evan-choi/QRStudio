using System;
namespace QRStudio.Engine.ExceptionHandler
{
	[Serializable]
	public class SymbolNotFoundException:System.ArgumentException
	{
        internal string message = null;

		public override string Message
		{
			get
			{
				return message;
			}
			
		}
		
		public SymbolNotFoundException(string message)
		{
			this.message = message;
		}
	}
}