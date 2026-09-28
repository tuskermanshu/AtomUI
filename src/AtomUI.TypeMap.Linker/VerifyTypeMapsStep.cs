using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Linker;
using Mono.Linker.Steps;

namespace AtomUI.TypeMap.Linker;

internal sealed class VerifyTypeMapsStep(IReadOnlyList<MapSlot> slots, VerificationReceipt? receipt = null) : IStep
{
    private readonly IReadOnlyList<MapSlot> _slots = slots;
    public void Process(LinkContext context)
    {
        try
        {
            foreach (var slot in _slots) Verify(context, slot);
            if (receipt is not null) receipt.SweepVerified = true;
            context.LogMessage($"AtomUI TypeMap ABI 1: verified {_slots.Count} surviving accessor(s) after Sweep.");
        }
        catch (BackendDiagnostic diagnostic)
        {
            diagnostic.Report(context);
        }
    }

    private static void Verify(LinkContext context, MapSlot slot)
    {
        var method = slot.Accessor;
        // Keep the exact pre-Sweep MethodDefinitions. Sweep may discard the identifying attributes.
        if (!method.HasBody || method.DeclaringType is null || !method.DeclaringType.Methods.Contains(method) ||
            !RegistrationAbi.AllTypes(method.Module.Types).Contains(method.DeclaringType) ||
            context.Annotations.GetAction(method.Module.Assembly) != AssemblyAction.Link ||
            !slot.Group.NestedTypes.Any(t => t.Methods.Contains(slot.Create) && t.Methods.Contains(slot.Add) && t.Methods.Contains(slot.Complete)) ||
            slot.Entries.Any(e => !RegistrationAbi.AllTypes(method.Module.Types).Contains(e.Target)))
            throw BackendDiagnostic.Unlowered($"Accessor/helper/target for '{slot.AccessorIdentity}' did not survive Sweep with its exact marked identity.");
        slot.Contract.Validate(method.Module.Assembly, method);
    }
}
