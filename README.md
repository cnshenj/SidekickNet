# SidekickNet
SidekickNet provides common utilities for .NET (.NET Standard 2.0 and above).

## Aspect-oriented Programming
One of the important utilities provided by SidekickNet is [Aspect-oriented Programming](https://en.wikipedia.org/wiki/Aspect-oriented_programming).
There are three projects for AOP:
- `Aspect` provides the framework and interfaces of AOP. It uses proxy objects for add advices to objects.
  - It doesn't provide implementations of proxy objects and proxy creation, because there are many possible implementations.
    Two reference implemenations are provided below that can be directly used.
- `Aspect.DynamicInheritance` implements proxy objects using dynamic inheritance.
  - This implemenation is optimized for performance by generating IL code directly.
- `Aspect.SimpleInjector` implements automatic proxy creation using [`SimpleInjector`](https://github.com/simpleinjector/SimpleInjector).
  - Dependency Injection is a convenient way of proxy creation. But other implementation are also possible.

### AOP Example
Assume we want to log method entrance easily.
First, create an "advice" that specifies what to do upon method entrace.
```cs
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal class LoggingAdviceAttribute : AdviceAttribute
{
    public override void Apply(IInvocationInfo invocation)
    {
        // Advice action - log the entrance of the method
        Console.WriteLine($"Entering {invocation.Method.Name}");

        // Continue with the original method
        this.Proceed(invocation);
    }
}
```

Then, apply the "advice" to the target method.
```cs
public class Foo : IFoo
{
    // Adding the advice attribute is enough to automatically run the logging code
    // Note the method must be "virtual" for dynamic inheritance to work
    [LoggingAdvice]
    public virtual void DoSomething()
    {
        // Business logic
    }
}
```

Finally, register how proxies will be created. In this case we use Simple Injector.
```cs
public void Initialize()
{
    // Create the DI container
    var container = new Container();

    // The proxy factory provided by an implementation, such as Dynamic Inheritance
    var proxyFactory = new ProxyFactory();

    // Tell the DI container to create a proxy object when an object is requested
    container.InterceptTarget(
        type => proxyFactory.GetProxyType(type).GetConstructors()[0]);

    // Register the advice in the container
    container.Register<LoggingAdviceAttribute>();

    // Register the regular types
    container.Register<IFoo, Foo>();

    // The "foo" object obtained from the DI container
    // is actually a proxy that will apply the advice and invoke the origianl method
    var foo = container.Get<IFoo>();

    // Print "Entering DoSomething" before actualy do something
    foo.DoSomething();
}
```

The `Aspect.Test` unit test project contains more examples, such as advice bundles.

## Other Utilities
- `Utilities` provides common helpers for strings, collections, exceptions, etc.
- `Utilities.AspNetCore` provides ASP.NET Core helpers, such as YAML (instead of JSON) configuration files.
