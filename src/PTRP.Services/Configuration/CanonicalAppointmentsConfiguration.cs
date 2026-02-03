using PTRP.Models.Enums;

namespace PTRP.Services.Configuration;

/// <summary>
/// Configurazione per la generazione automatica dei 4 appuntamenti canonici
/// di un Progetto Terapeutico.
/// Definisce i tipi di visita, gli offset temporali e l'ordine di generazione.
/// </summary>
public class CanonicalAppointmentsConfiguration
{
    /// <summary>
    /// Configurazione di default per i 4 appuntamenti canonici.
    /// </summary>
    public static CanonicalAppointmentsConfiguration Default { get; } = new()
    {
        Appointments = new List<CanonicalAppointmentDefinition>
        {
            new()
            {
                Type = VisitType.INTAKE,
                OffsetFromStart = TimeSpan.FromDays(90), // +3 mesi dalla StartDate
                Description = "Prima Apertura (INTAKE)"
            },
            new()
            {
                Type = VisitType.INTERMEDIATE,
                OffsetFromPrevious = TimeSpan.FromDays(180), // +6 mesi da INTAKE
                Description = "Verifica Intermedia"
            },
            new()
            {
                Type = VisitType.FINAL,
                OffsetFromPrevious = TimeSpan.FromDays(180), // +6 mesi da INTERMEDIATE
                Description = "Verifica Finale"
            },
            new()
            {
                Type = VisitType.DISCHARGE,
                OffsetFromPrevious = TimeSpan.FromDays(30), // +1 mese da FINAL
                Description = "Dimissioni"
            }
        }
    };

    /// <summary>
    /// Lista ordinata dei 4 appuntamenti canonici.
    /// L'ordine definisce la sequenza di generazione.
    /// </summary>
    public List<CanonicalAppointmentDefinition> Appointments { get; init; } = new();
}

/// <summary>
/// Definizione di un singolo appuntamento canonico.
/// </summary>
public class CanonicalAppointmentDefinition
{
    /// <summary>
    /// Tipo di visita (INTAKE, INTERMEDIATE, FINAL, DISCHARGE)
    /// </summary>
    public VisitType Type { get; init; }

    /// <summary>
    /// Offset dalla data di inizio progetto (solo per il primo appuntamento).
    /// Se null, usa OffsetFromPrevious.
    /// </summary>
    public TimeSpan? OffsetFromStart { get; init; }

    /// <summary>
    /// Offset dall'appuntamento precedente nella sequenza.
    /// Se null, usa OffsetFromStart.
    /// </summary>
    public TimeSpan? OffsetFromPrevious { get; init; }

    /// <summary>
    /// Descrizione testuale dell'appuntamento (opzionale, per note iniziali).
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Calcola la data schedulata per questo appuntamento.
    /// </summary>
    /// <param name="startDate">Data di inizio progetto</param>
    /// <param name="previousDate">Data dell'appuntamento precedente (se applicabile)</param>
    /// <returns>Data schedulata calcolata</returns>
    public DateTime CalculateScheduledDate(DateTime startDate, DateTime? previousDate = null)
    {
        if (OffsetFromStart.HasValue)
        {
            return startDate.Add(OffsetFromStart.Value);
        }

        if (OffsetFromPrevious.HasValue && previousDate.HasValue)
        {
            return previousDate.Value.Add(OffsetFromPrevious.Value);
        }

        throw new InvalidOperationException(
            $"Cannot calculate scheduled date for {Type}: " +
            "either OffsetFromStart or (OffsetFromPrevious + previousDate) must be provided.");
    }
}
