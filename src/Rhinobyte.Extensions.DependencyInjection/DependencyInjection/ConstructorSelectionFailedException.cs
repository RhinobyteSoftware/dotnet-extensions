using System;
using System.Runtime.Serialization;

namespace Rhinobyte.Extensions.DependencyInjection
{
	[Serializable]
	public class ConstructorSelectionFailedException : InvalidOperationException
	{
		public ConstructorSelectionFailedException(string? message)
			: base(message)
		{
		}

		public ConstructorSelectionFailedException(string? message, Exception? innerException)
			: base(message, innerException)
		{
		}

		protected ConstructorSelectionFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
