using Mono.Cecil.Cil;
using Mono.Linker;
using Mono.Linker.Steps;

namespace AtomUI.TypeMap.Linker;

/// <summary>ILLink build-only ABI 1 entry point. Inject exactly once immediately after MarkStep.</summary>
public sealed class MaterializeTypeMapsStep : IStep
{
    private bool _processed;

    public void Process(LinkContext context)
    {
        try
        {
            var receiptPath = VerificationReceipt.Prepare(context);
            if (_processed) throw BackendDiagnostic.Unsupported("The same TypeMap materialization step was invoked twice.");
            _processed = true;
            ToolchainContract.Validate(context);
            // Validate every slot before modifying any body. A bad package never leaves a partially lowered pipeline.
            var slots = new RegistrationAbi(context).Read();
            foreach (var slot in slots) Materialize(slot);
            var receipt = VerificationReceipt.Schedule(context, receiptPath, slots);
            context.Pipeline.AddStepAfter(typeof(SweepStep), new VerifyTypeMapsStep(slots, receipt));
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            BackendDiagnostic.Unsupported($"Cannot prepare TypeMap verification receipt: {error.Message}").Report(context);
        }
        catch (BackendDiagnostic diagnostic)
        {
            diagnostic.Report(context);
        }
    }

    private static void Materialize(MapSlot slot)
    {
        var method = slot.Accessor;
        method.DebugInformation.SequencePoints.Clear();
        method.DebugInformation.Scope = null;
        method.DebugInformation.StateMachineKickOffMethod = null;
        method.DebugInformation.CustomDebugInformations.Clear();
        method.CustomDebugInformations.Clear();
        method.Body = new MethodBody(method) { InitLocals = false, MaxStackSize = slot.Entries.Count == 0 ? 1 : 4 };
        var il = method.Body.GetILProcessor();
        il.Emit(OpCodes.Call, slot.Create);
        foreach (var entry in slot.Entries)
        {
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldstr, entry.Key);
            // Add already owns GetTypeFromHandle and all dictionary dependencies; do not introduce a new call after Mark.
            il.Emit(OpCodes.Ldtoken, entry.Target);
            il.Emit(OpCodes.Call, slot.Add);
        }
        il.Emit(OpCodes.Call, slot.Complete);
        il.Emit(OpCodes.Ret);
    }
}
