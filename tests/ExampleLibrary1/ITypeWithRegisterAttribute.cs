using Rhinobyte.Extensions.DependencyInjection;

namespace ExampleLibrary1;

[RegisterForDependencyInjection(typeof(TypeWithRegisterAttribute))]
public interface ITypeWithRegisterAttribute
{
}

public class TypeWithRegisterAttribute : ITypeWithRegisterAttribute
{

}
