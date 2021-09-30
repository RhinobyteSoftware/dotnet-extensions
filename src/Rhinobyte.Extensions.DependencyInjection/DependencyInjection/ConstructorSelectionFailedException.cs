using System;
using System.Runtime.Serialization;

namespace Rhinobyte.Extensions.DependencyInjection
{
	/// <summary>
	/// Subclass of <see cref="InvalidOperationException"/> thrown in scenarios when explicit constructor selection fails due to invalid state
	/// </summary>
	[Serializable]
	public class ConstructorSelectionFailedException : InvalidOperationException
	{
		/// <inheritdoc/>
		public ConstructorSelectionFailedException()
			: base()
		{
		}

		/// <inheritdoc/>
		public ConstructorSelectionFailedException(string? message)
			: base(message)
		{
		}

		/// <inheritdoc/>
		public ConstructorSelectionFailedException(string? message, Exception? innerException)
			: base(message, innerException)
		{
		}

		/// <inheritdoc/>
		protected ConstructorSelectionFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
