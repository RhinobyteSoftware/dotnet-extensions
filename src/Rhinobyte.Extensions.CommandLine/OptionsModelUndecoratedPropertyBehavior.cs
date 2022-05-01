using System.Reflection;

namespace Rhinobyte.Extensions.CommandLine;


/// <summary>
/// The behavior to apply 
/// </summary>
public enum OptionsModelUndecoratedPropertyBehavior
{
	/// <summary>
	/// The parser will try create an option symbol using the <see cref="MemberInfo.Name"/>
	/// </summary>
	Default = 0,

	/// <summary>
	/// The parser will throw if a settable property does not have either an explicit symbol decorator or an explicit <see cref="BinderIgnoredAttribute"/> decorator
	/// </summary>
	Error = 1,

	/// <summary>
	/// The parser will ignore the settable property if it does not have an explicit symbol decorator
	/// </summary>
	Ignore = 2
}
