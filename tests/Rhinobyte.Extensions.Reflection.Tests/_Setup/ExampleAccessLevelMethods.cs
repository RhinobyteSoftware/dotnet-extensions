namespace Rhinobyte.Extensions.Reflection.Tests.Setup;

public class ExampleAccessLevelMethods
{
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0040 // Add accessibility modifiers
	void DefaultAccessInstanceMethod() { }
#pragma warning restore IDE0040 // Add accessibility modifiers

	private void PrivateInstanceMethod() { }
#pragma warning restore IDE0051 // Remove unused private members

	protected internal void ProtectedInternalInstanceMethod() { }

	protected void ProtectedInstanceMethod() { }

	public void PublicInstanceMethod() { }

	internal void InternalInstanceMethod() { }
}
