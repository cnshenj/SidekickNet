using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using SidekickNet.Aspect.DynamicInheritance;
using SidekickNet.Aspect.SimpleInjector;

using SimpleInjector;

using Xunit;

namespace SidekickNet.Aspect.Test
{
    public class AspectTest
    {
        [Fact]
        public void Property_Getter_Setter()
        {
            var container = this.Prepare();
            var a = container.GetInstance<IContract>();
            const string name = "Test";
            a.Name = name;
            var result = a.Name;
            Assert.Equal(name, result);

            var log = LoggingAdviceAttribute.Log;
            Assert.Equal($"{LoggingAdviceAttribute.EnterPrefix} set_{nameof(a.Name)}", log[0].message);
            Assert.Equal($"{LoggingAdviceAttribute.ExitPrefix} set_{nameof(a.Name)}", log[1].message);
            Assert.Equal($"{LoggingAdviceAttribute.EnterPrefix} get_{nameof(a.Name)}", log[2].message);
            Assert.Equal($"{LoggingAdviceAttribute.ExitPrefix} get_{nameof(a.Name)}", log[3].message);
        }

        [Fact]
        public void One_Advice()
        {
            var container = this.Prepare();
            var a = container.GetInstance<IContract>();
            var b = a.OneAdvice(container.GetInstance<IContract>(), 1000.0);
            Assert.Equal(1.0, b.X);

            var log = LoggingAdviceAttribute.Log;
            Assert.Equal($"{LoggingAdviceAttribute.EnterPrefix} {nameof(a.OneAdvice)}", log[0].message);
            Assert.Equal($"{LoggingAdviceAttribute.ExitPrefix} {nameof(a.OneAdvice)}", log[1].message);
        }

        [Fact]
        public void Chained_Advices()
        {
            var container = this.Prepare();
            var a = container.GetInstance<IContract>();
            a.X = 10.0;
            var b = a.ChainedAdvices(10.0);
            Assert.Equal(100.0, b.X);

            var log = LoggingAdviceAttribute.Log;
            Assert.Equal($"{LoggingAdviceAttribute.EnterPrefix} {nameof(a.ChainedAdvices)}", log[0].message);
            Assert.Equal($"{LoggingAdviceAttribute.ExitPrefix} {nameof(a.ChainedAdvices)}", log[1].message);

            Assert.True(ValidationAdviceAttribute.Validated);
            Assert.InRange(ValidationAdviceAttribute.Timestamp, log[0].timestamp, log[1].timestamp);
        }

        [Fact]
        public void Advice_Bundle()
        {
            var container = this.Prepare();
            var a = container.GetInstance<Base>();
            var value = a.AdviceBundle();
            Assert.Equal(nameof(a.AdviceBundle), value);

            var log = LoggingAdviceAttribute.Log;
            Assert.Equal($"{LoggingAdviceAttribute.EnterPrefix} {nameof(a.AdviceBundle)}", log[0].message);
            Assert.Equal($"2nd{LoggingAdviceAttribute.EnterPrefix} {nameof(a.AdviceBundle)}", log[1].message);
            Assert.Equal($"2nd{LoggingAdviceAttribute.ExitPrefix} {nameof(a.AdviceBundle)}", log[2].message);
            Assert.Equal($"{LoggingAdviceAttribute.ExitPrefix} {nameof(a.AdviceBundle)}", log[3].message);
        }

        [Fact]
        public void Advices_By_Types()
        {
            var container = this.Prepare();
            AdviceTypesAttribute.GetInstance = container.GetInstance;

            var a = container.GetInstance<Base>();
            var value = a.AdvicesByTypes();
            Assert.Equal(nameof(a.AdvicesByTypes), value);

            var log = LoggingAdviceAttribute.Log;
            Assert.Equal($"{LoggingAdviceAttribute.EnterPrefix} {nameof(a.AdvicesByTypes)}", log[0].message);
            Assert.Equal($"{LoggingAdviceAttribute.ExitPrefix} {nameof(a.AdvicesByTypes)}", log[1].message);
            Assert.True(ValidationAdviceAttribute.Validated);
            Assert.InRange(ValidationAdviceAttribute.Timestamp, log[0].timestamp, log[1].timestamp);
        }

        [Fact]
        public void Advices_By_Type_Bundle()
        {
            var container = this.Prepare();
            AdviceTypesAttribute.GetInstance = container.GetInstance;

            var a = container.GetInstance<Base>();
            var value = a.AdvicesByTypeBundle();
            Assert.Equal(nameof(a.AdvicesByTypeBundle), value);

            var log = LoggingAdviceAttribute.Log;
            Assert.Equal($"{LoggingAdviceAttribute.EnterPrefix} {nameof(a.AdvicesByTypeBundle)}", log[0].message);
            Assert.Equal($"{LoggingAdviceAttribute.ExitPrefix} {nameof(a.AdvicesByTypeBundle)}", log[1].message);
            Assert.True(ValidationAdviceAttribute.Validated);
            Assert.InRange(ValidationAdviceAttribute.Timestamp, log[0].timestamp, log[1].timestamp);
        }

        [Fact]
        public void Shortcut_Advice()
        {
            var container = this.Prepare();
            var a = container.GetInstance<IContract>();
            var b = a.ChainedAdvices(10.0);
            Assert.NotEmpty(LoggingAdviceAttribute.Log);
            Assert.True(ValidationAdviceAttribute.Validated);

            LoggingAdviceAttribute.Log.Clear();
            ValidationAdviceAttribute.Validated = false;
            var c = a.ChainedAdvices(10.0);
            Assert.Same(b, c);
            Assert.Empty(LoggingAdviceAttribute.Log);
            Assert.False(ValidationAdviceAttribute.Validated);
        }

        [Fact]
        public void Generic_Value_Type()
        {
            var container = this.Prepare();
            var a = container.GetInstance<Base>();

            const double c = 3.0;
            var n = a.Generic(c);
            Assert.Equal(c, n);

            var now = DateTime.UtcNow;
            var t = a.Generic(now);
            Assert.Equal(now, t);
        }

        [Fact]
        public void Generic_NonValue_Type()
        {
            var container = this.Prepare();
            var a = container.GetInstance<Base>();

            const string greeting = "Hello, world!";
            var str = a.Generic(greeting);
            Assert.Equal(greeting, str);

            var b = container.GetInstance<Base>();
            var obj = a.Generic(b);
            Assert.Same(b, obj);
        }

        [Fact]
        public void Invoke_Another_Pointcut_Method()
        {
            var container = this.Prepare();
            var a = container.GetInstance<Base>();
            a.Generic(3.0);

            var log = LoggingAdviceAttribute.Log;
            Assert.Equal($"{LoggingAdviceAttribute.EnterPrefix} {nameof(a.Generic)}", log[0].message);
            Assert.Equal($"{LoggingAdviceAttribute.EnterPrefix} Protected", log[1].message);
            Assert.Equal($"{LoggingAdviceAttribute.ExitPrefix} Protected", log[2].message);
            Assert.Equal($"{LoggingAdviceAttribute.ExitPrefix} {nameof(a.Generic)}", log[3].message);
        }

        [Fact]
        public void Async_Advice_Without_Result()
        {
            var container = this.Prepare();
            var a = container.GetInstance<IContract>();
            var task = a.DoAsync();
            task.Wait();
            var whenDo = a.WhenDo;
            Assert.NotEqual(DateTime.MinValue, AsyncAdvice1Attribute.WhenApply);
            Assert.True(AsyncAdvice1Attribute.WhenApply < whenDo);

            // AsyncAspect2 has a task that starts at the same time as the intercepted method, but last longer
            Assert.True(whenDo < AsyncAdvice2Attribute.WhenApply);
        }

        [Fact]
        public void Async_Advice_With_Result()
        {
            var container = this.Prepare();
            var a = container.GetInstance<IContract>();
            var value = a.GetValueAsync().Result;
            Assert.NotEqual(DateTime.MinValue, AsyncAdvice1Attribute.WhenApply);
            Assert.True(AsyncAdvice1Attribute.WhenApply < AsyncAdvice2Attribute.WhenApply);
            Assert.Equal(0.5, value);
            Assert.Equal(1, a.GetValueCount);
        }

        [Fact]
        public void Explicit_Interface()
        {
            var container = this.Prepare();
            var a = container.GetInstance<IContract>();

            var simple = (ISimple)a;
            var value = simple.Foobar();
            Assert.Equal(1, value);
            var log = LoggingAdviceAttribute.Log;
            Assert.Equal($"{LoggingAdviceAttribute.EnterPrefix} {nameof(simple.Foobar)}", log[0].message);
            Assert.Equal($"{LoggingAdviceAttribute.ExitPrefix} {nameof(simple.Foobar)}", log[1].message);

            log.Clear();
            var b = container.GetInstance<Base>();
            value = b.Foobar();
            Assert.Equal(1, value);
            Assert.Equal($"{LoggingAdviceAttribute.EnterPrefix} {nameof(b.Foobar)}", log[0].message);
            Assert.Equal($"{LoggingAdviceAttribute.ExitPrefix} {nameof(b.Foobar)}", log[1].message);
        }

        [Fact]
        public void Non_Virtual_Method_Is_Invalid()
        {
            var container = new Container();

            var proxyFactory = new ProxyFactory();
            container.Intercept(
                type => type == typeof(Invalid),
                type => proxyFactory.GetProxyType(type).GetConstructors()[0]);
            Assert.Throws<ArgumentException>(
                () =>
                {
                    container.Register<IInvalid, Invalid>();
                });
        }

        [Fact]
        public void Invocation_Proceed_Twice()
        {
            var container = this.Prepare();
            var a = container.GetInstance<IContract>();
            var instance = a as Derived;
            var b = instance!.GetCount();
            Assert.Equal(2, b);
        }

        private Container Prepare()
        {
            var container = new Container();

            var proxyFactory = new ProxyFactory();
            container.InterceptTarget(
                type => proxyFactory.GetProxyType(type).GetConstructors()[0]);

            container.Register<LoggingAdviceAttribute>();
            container.Register<ValidationAdviceAttribute>();
            container.Register<LoggingValidationBundle>();
            container.Register<IContract, Derived>();
            container.Register<Base, Derived>();

            CachingAdviceAttribute.Cache = default;
            LoggingAdviceAttribute.Log.Clear();
            ValidationAdviceAttribute.Validated = false;
            ValidationAdviceAttribute.Timestamp = DateTime.MinValue;
            AsyncAdvice1Attribute.WhenApply = DateTime.MinValue;
            AsyncAdvice2Attribute.WhenApply = DateTime.MinValue;

            return container;
        }
    }

    public interface ISimple
    {
        int Foobar();
    }

    public interface IContract : ICloneable
    {
        string? Name { get; set; }

        double X { get; set; }

        int GetValueCount { get; }

        DateTime WhenDo { get; set; }

        IContract OneAdvice(IContract a, double y);

        IContract ChainedAdvices(double y);

        Task<double> GetValueAsync();

        Task DoAsync();

        T? ToType<T>() where T : class, IContract;
    }

    public abstract class Base : IContract, ISimple
    {
        public virtual string? Name { [LoggingAdvice]get; [LoggingAdvice]set; }

        [LoggingAdvice]
        public virtual int Foobar()
        {
            Console.WriteLine($"Executing {nameof(Base)}.{nameof(this.Foobar)}");
            return 1;
        }

        public double X { get; set; } = 99.0;

        public int GetValueCount { get; protected set; }

        public DateTime WhenDo { get; set; }

        public virtual IContract OneAdvice(IContract a, double y)
        {
            a.X /= y;
            return a;
        }

        public abstract IContract ChainedAdvices(double y);

        public virtual Task<double> GetValueAsync()
        {
            var random = new Random();
            return Task.FromResult(random.NextDouble());
        }

        public abstract Task DoAsync();

        public T? ToType<T>() where T : class, IContract => this as T;

        public object Clone() => this.MemberwiseClone();

        public abstract T Generic<T>(T value);

        [DoubleLogging]
        public virtual string AdviceBundle() => nameof(this.AdviceBundle);

        [AdviceTypes(typeof(LoggingAdviceAttribute), typeof(ValidationAdviceAttribute))]
        public virtual string AdvicesByTypes() => nameof(this.AdvicesByTypes);

        [AdviceTypes(typeof(LoggingValidationBundle))]
        public virtual string AdvicesByTypeBundle() => nameof(this.AdvicesByTypeBundle);

        [LoggingAdvice]
        protected virtual T Protected<T>(T value) => value;
    }

    public class Derived : Base
    {
        private int count;

        [LoggingAdvice]
        public override IContract OneAdvice(IContract a, double y)
        {
            a = base.OneAdvice(a, y);
            if (a.X < 1.0)
            {
                a.X = 1.0;
            }

            return a;
        }

        [CachingAdvice, LoggingAdvice(Order = 1), ValidationAdvice(Order = 2)]
        public override IContract ChainedAdvices(double y) => new Derived { X = this.X * y };

        [AsyncAdvice1, AsyncAdvice2(Order = 1)]
        public override async Task<double> GetValueAsync()
        {
            await Task.Delay(10).ConfigureAwait(false);
            ++this.GetValueCount;
            return 0.5;
        }

        [AsyncAdvice1, AsyncAdvice2(Order = 1)]
        public override async Task DoAsync()
        {
            await Task.Delay(10).ConfigureAwait(false);
            this.WhenDo = DateTime.Now;
        }

        [LoggingAdvice]
        public override T Generic<T>(T value) => this.Protected(value);

        [ProceedTwiceAdvice]
        public virtual int GetCount() => ++this.count;
    }

    public interface IInvalid
    {
        void Foobar();
    }

    public class Invalid : IInvalid
    {
        [LoggingAdvice]
        public void Foobar() => Console.WriteLine($"Executing {nameof(this.Foobar)}");
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    internal class LoggingAdviceAttribute : AdviceAttribute
    {
        public const string EnterPrefix = ":Entering";

        public const string ExitPrefix = ":Exiting";

        public static IList<(DateTime timestamp, string message)> Log { get; } = new List<(DateTime, string)>();

        public string? Context { get; set; }

        public override void Apply(IInvocationInfo invocation)
        {
            WriteLog($"{this.Context}{EnterPrefix} {invocation.Method.Name}");
            this.Proceed(invocation);
            WriteLog($"{this.Context}{ExitPrefix} {invocation.Method.Name}");
        }

        private static void WriteLog(string message)
        {
            Log.Add((DateTime.UtcNow, message));
        }
    }

    internal class CachingAdviceAttribute : AdviceAttribute
    {
        public static object? Cache { get; set; }

        public override void Apply(IInvocationInfo invocation)
        {
            if (Cache == null)
            {
                this.Proceed(invocation);
                Cache = invocation.ReturnValue;
            }
            else
            {
                invocation.ReturnValue = Cache;
            }
        }
    }

    internal class ValidationAdviceAttribute : AdviceAttribute
    {
        public static bool Validated { get; set; }

        public static DateTime Timestamp { get; set; }

        public override void Apply(IInvocationInfo invocation)
        {
            var targetType = invocation.Target.GetType().BaseType!;
            if (invocation.Target is Base
                && (invocation.Method == targetType.GetMethod(nameof(Base.AdvicesByTypes))
                    || invocation.Method == targetType.GetMethod(nameof(Base.AdvicesByTypeBundle))))
            {
                Timestamp = DateTime.UtcNow;
                Validated = true;
            }
            else if (invocation.Target is IContract instance)
            {
                if (invocation.Method == targetType.GetMethod(nameof(instance.ChainedAdvices)))
                {
                    Timestamp = DateTime.UtcNow;

                    if (instance.X >= 100)
                    {
                        var clone = (IContract)instance.Clone();
                        clone.X = 100.0;
                        invocation.ReturnValue = clone;
                        return;
                    }

                    var y = (double)invocation.Arguments![0];
                    if (y > 1)
                    {
                        Validated = true;
                    }
                    else if (y < 0)
                    {
                        throw new ArgumentException();
                    }
                }
            }

            this.Proceed(invocation);
        }
    }

    internal class AsyncAdvice1Attribute : AsyncAdviceAttribute
    {
        public static DateTime WhenApply { get; set; }

        protected override async Task ApplyAsync(IInvocationInfo invocation)
        {
            invocation.InitializeAwait();
            await Task.Delay(10);
            WhenApply = DateTime.Now;
            this.Proceed(invocation);
            await ((Task)invocation.ReturnValue!).ConfigureAwait(false);
        }

        protected override async Task<T> ApplyAsync<T>(IInvocationInfo invocation)
        {
            invocation.InitializeAwait();
            await Task.Delay(10);
            WhenApply = DateTime.Now;
            this.Proceed(invocation);
            return await ((Task<T>)invocation.ReturnValue!).ConfigureAwait(false);
        }
    }

    internal class AsyncAdvice2Attribute : AsyncAdviceAttribute
    {
        public static DateTime WhenApply { get; set; }

        protected override Task ApplyAsync(IInvocationInfo invocation)
        {
            var myTask = Task.Delay(20).ContinueWith(_ => WhenApply = DateTime.Now);
            this.Proceed(invocation);
            return Task.WhenAll(myTask, (Task)invocation.ReturnValue!);
        }

        protected override async Task<T> ApplyAsync<T>(IInvocationInfo invocation)
        {
            invocation.InitializeAwait();
            await Task.Delay(10);
            WhenApply = DateTime.Now;
            this.Proceed(invocation);
            return await ((Task<T>)invocation.ReturnValue!).ConfigureAwait(false);
        }
    }

    internal class DoubleLoggingAttribute : AdviceBundleAttribute
    {
        public DoubleLoggingAttribute()
            : base(new LoggingAdviceAttribute(), new LoggingAdviceAttribute() { Context = "2nd" })
        {
        }
    }

    internal class LoggingValidationBundle : AdviceTypeBundle
    {
        public LoggingValidationBundle()
            : base(typeof(LoggingAdviceAttribute), typeof(ValidationAdviceAttribute))
        {
        }
    }

    internal class ProceedTwiceAdviceAttribute : AdviceAttribute
    {
        public override void Apply(IInvocationInfo invocation)
        {
            this.Proceed(invocation);
            this.Proceed(invocation);
        }
    }
}
