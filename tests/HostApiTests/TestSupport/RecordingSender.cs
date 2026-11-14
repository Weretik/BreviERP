using System.Reflection;
using Mediator;

namespace HostApiTests.TestSupport;

/// <summary>An <see cref="ISender"/> that records each message and answers with a given value.</summary>
public class RecordingSender : DispatchProxy
{
    public List<object> Messages { get; } = [];

    public object? Answer { get; set; }

    public static (ISender Sender, RecordingSender Recorder) Create(object? answer)
    {
        var sender = DispatchProxy.Create<ISender, RecordingSender>();
        var recorder = (RecordingSender)(object)sender;
        recorder.Answer = answer;
        return (sender, recorder);
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        Messages.Add(args![0]!);
        var returnType = targetMethod!.ReturnType;
        return Activator.CreateInstance(returnType, Answer);
    }
}
