namespace Crm.Domain.Embroidery.Calculation;

/// <summary>
/// The «Розрахунок» form (EMB-001). One unit is one embroidery or one chevron.
/// </summary>
/// <param name="UnitsPerHoop">«Шевронів в одному зап’ялюванні»; 1 for embroidery on a garment.</param>
/// <param name="FabricLayers">Chevron fabric layers; ignored for embroidery on a garment.</param>
/// <param name="ProgramMinutes">Development time for a new or adjusted program; ignored for a ready one.</param>
/// <param name="MachineMinutes">Machine time of the whole batch; needed only by a «Машинна година» cost.</param>
/// <param name="ExpenseOverrides">Extra costs ticked or unticked for this calculation only, by id.</param>
public sealed record EmbroideryInput(
    EmbroideryMode Mode,
    int Quantity,
    int StitchesPerUnit,
    string HoopId,
    int UnitsPerHoop,
    int StabilizerLayers,
    int FabricLayers,
    EmbroideryProgram Program,
    int ProgramMinutes,
    decimal? MachineMinutes,
    EmbroideryMaterialMode MaterialMode,
    IReadOnlyDictionary<string, bool> ExpenseOverrides);

public enum EmbroideryMode
{
    /// <summary>«Вишивка на виробі».</summary>
    Garment = 0,

    /// <summary>«Шеврони».</summary>
    Chevron = 1,
}

public enum EmbroideryProgram
{
    New = 0,
    Adjustment = 1,

    /// <summary>The program exists: development costs nothing.</summary>
    Ready = 2,
}

/// <summary>How stabilizer and fabric are charged; one choice for both materials.</summary>
public enum EmbroideryMaterialMode
{
    /// <summary>«Лише використані заготовки»: the blanks used; spare blanks stay in the workshop.</summary>
    UsedBlanks = 0,

    /// <summary>«Весь відріз на замовлення»: the whole cut length.</summary>
    FullCut = 1,
}
