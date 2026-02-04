using System;
using System.Windows;
using System.Windows.Controls;

namespace PTRP.App.Controls;

/// <summary>
/// Controllo DatePicker personalizzato con stile Material Design e supporto validazione.
/// </summary>
/// <remarks>
/// Supporta:
/// - Validazione required
/// - Validazione future date (data deve essere futura)
/// - Validazione date range (MinDate, MaxDate)
/// - Messaggio di errore personalizzabile
/// </remarks>
/// <example>
/// <code>
/// &lt;controls:PtrpDatePicker 
///     SelectedDate="{Binding StartDate}" 
///     Watermark="Data Inizio"
///     IsRequired="True"
///     MinDate="{Binding Today}" /&gt;
/// </code>
/// </example>
public partial class PtrpDatePicker : UserControl
{
    public static readonly DependencyProperty SelectedDateProperty =
        DependencyProperty.Register(
            nameof(SelectedDate),
            typeof(DateTime?),
            typeof(PtrpDatePicker),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedDateChanged));

    public static readonly DependencyProperty WatermarkProperty =
        DependencyProperty.Register(
            nameof(Watermark),
            typeof(string),
            typeof(PtrpDatePicker),
            new PropertyMetadata("Seleziona data"));

    public static readonly DependencyProperty IsRequiredProperty =
        DependencyProperty.Register(
            nameof(IsRequired),
            typeof(bool),
            typeof(PtrpDatePicker),
            new PropertyMetadata(false, OnValidationPropertyChanged));

    public static readonly DependencyProperty MinDateProperty =
        DependencyProperty.Register(
            nameof(MinDate),
            typeof(DateTime?),
            typeof(PtrpDatePicker),
            new PropertyMetadata(null, OnValidationPropertyChanged));

    public static readonly DependencyProperty MaxDateProperty =
        DependencyProperty.Register(
            nameof(MaxDate),
            typeof(DateTime?),
            typeof(PtrpDatePicker),
            new PropertyMetadata(null, OnValidationPropertyChanged));

    public static readonly DependencyProperty MustBeFutureDateProperty =
        DependencyProperty.Register(
            nameof(MustBeFutureDate),
            typeof(bool),
            typeof(PtrpDatePicker),
            new PropertyMetadata(false, OnValidationPropertyChanged));

    public static readonly DependencyProperty ValidationErrorProperty =
        DependencyProperty.Register(
            nameof(ValidationError),
            typeof(string),
            typeof(PtrpDatePicker),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty HasValidationErrorProperty =
        DependencyProperty.Register(
            nameof(HasValidationError),
            typeof(bool),
            typeof(PtrpDatePicker),
            new PropertyMetadata(false));

    /// <summary>
    /// Data selezionata.
    /// </summary>
    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    /// <summary>
    /// Testo placeholder quando nessuna data è selezionata.
    /// </summary>
    public string Watermark
    {
        get => (string)GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    /// <summary>
    /// Indica se la data è obbligatoria.
    /// </summary>
    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    /// <summary>
    /// Data minima consentita.
    /// </summary>
    public DateTime? MinDate
    {
        get => (DateTime?)GetValue(MinDateProperty);
        set => SetValue(MinDateProperty, value);
    }

    /// <summary>
    /// Data massima consentita.
    /// </summary>
    public DateTime? MaxDate
    {
        get => (DateTime?)GetValue(MaxDateProperty);
        set => SetValue(MaxDateProperty, value);
    }

    /// <summary>
    /// Indica se la data deve essere futura.
    /// </summary>
    public bool MustBeFutureDate
    {
        get => (bool)GetValue(MustBeFutureDateProperty);
        set => SetValue(MustBeFutureDateProperty, value);
    }

    /// <summary>
    /// Messaggio di errore di validazione.
    /// </summary>
    public string ValidationError
    {
        get => (string)GetValue(ValidationErrorProperty);
        private set => SetValue(ValidationErrorProperty, value);
    }

    /// <summary>
    /// Indica se ci sono errori di validazione.
    /// </summary>
    public bool HasValidationError
    {
        get => (bool)GetValue(HasValidationErrorProperty);
        private set => SetValue(HasValidationErrorProperty, value);
    }

    public PtrpDatePicker()
    {
        InitializeComponent();
    }

    private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PtrpDatePicker picker)
        {
            picker.ValidateDate();
        }
    }

    private static void OnValidationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PtrpDatePicker picker)
        {
            picker.ValidateDate();
        }
    }

    private void ValidateDate()
    {
        ValidationError = string.Empty;
        HasValidationError = false;

        // Required validation
        if (IsRequired && !SelectedDate.HasValue)
        {
            ValidationError = "La data è obbligatoria";
            HasValidationError = true;
            return;
        }

        if (!SelectedDate.HasValue)
            return;

        var date = SelectedDate.Value;

        // Future date validation
        if (MustBeFutureDate && date.Date < DateTime.Today)
        {
            ValidationError = "La data deve essere futura";
            HasValidationError = true;
            return;
        }

        // Min date validation
        if (MinDate.HasValue && date < MinDate.Value)
        {
            ValidationError = $"La data deve essere successiva al {MinDate.Value:dd/MM/yyyy}";
            HasValidationError = true;
            return;
        }

        // Max date validation
        if (MaxDate.HasValue && date > MaxDate.Value)
        {
            ValidationError = $"La data deve essere precedente al {MaxDate.Value:dd/MM/yyyy}";
            HasValidationError = true;
            return;
        }
    }

    /// <summary>
    /// Valida manualmente il controllo e restituisce true se valido.
    /// </summary>
    public bool Validate()
    {
        ValidateDate();
        return !HasValidationError;
    }
}
