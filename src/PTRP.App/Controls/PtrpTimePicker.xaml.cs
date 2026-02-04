using System;
using System.Windows;
using System.Windows.Controls;

namespace PTRP.App.Controls;

/// <summary>
/// Controllo TimePicker personalizzato con stile Material Design e supporto validazione.
/// </summary>
/// <remarks>
/// Supporta:
/// - Validazione required
/// - Validazione time range (MinTime, MaxTime)
/// - Formato 24 ore (HH:mm)
/// - Messaggio di errore personalizzabile
/// </remarks>
/// <example>
/// <code>
/// &lt;controls:PtrpTimePicker 
///     SelectedTime="{Binding StartTime}" 
///     Watermark="Ora Inizio"
///     IsRequired="True" /&gt;
/// </code>
/// </example>
public partial class PtrpTimePicker : UserControl
{
    public static readonly DependencyProperty SelectedTimeProperty =
        DependencyProperty.Register(
            nameof(SelectedTime),
            typeof(TimeSpan?),
            typeof(PtrpTimePicker),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedTimeChanged));

    public static readonly DependencyProperty WatermarkProperty =
        DependencyProperty.Register(
            nameof(Watermark),
            typeof(string),
            typeof(PtrpTimePicker),
            new PropertyMetadata("Seleziona ora"));

    public static readonly DependencyProperty IsRequiredProperty =
        DependencyProperty.Register(
            nameof(IsRequired),
            typeof(bool),
            typeof(PtrpTimePicker),
            new PropertyMetadata(false, OnValidationPropertyChanged));

    public static readonly DependencyProperty MinTimeProperty =
        DependencyProperty.Register(
            nameof(MinTime),
            typeof(TimeSpan?),
            typeof(PtrpTimePicker),
            new PropertyMetadata(null, OnValidationPropertyChanged));

    public static readonly DependencyProperty MaxTimeProperty =
        DependencyProperty.Register(
            nameof(MaxTime),
            typeof(TimeSpan?),
            typeof(PtrpTimePicker),
            new PropertyMetadata(null, OnValidationPropertyChanged));

    public static readonly DependencyProperty ValidationErrorProperty =
        DependencyProperty.Register(
            nameof(ValidationError),
            typeof(string),
            typeof(PtrpTimePicker),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty HasValidationErrorProperty =
        DependencyProperty.Register(
            nameof(HasValidationError),
            typeof(bool),
            typeof(PtrpTimePicker),
            new PropertyMetadata(false));

    /// <summary>
    /// Ora selezionata.
    /// </summary>
    public TimeSpan? SelectedTime
    {
        get => (TimeSpan?)GetValue(SelectedTimeProperty);
        set => SetValue(SelectedTimeProperty, value);
    }

    /// <summary>
    /// Testo placeholder quando nessuna ora è selezionata.
    /// </summary>
    public string Watermark
    {
        get => (string)GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    /// <summary>
    /// Indica se l'ora è obbligatoria.
    /// </summary>
    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    /// <summary>
    /// Ora minima consentita.
    /// </summary>
    public TimeSpan? MinTime
    {
        get => (TimeSpan?)GetValue(MinTimeProperty);
        set => SetValue(MinTimeProperty, value);
    }

    /// <summary>
    /// Ora massima consentita.
    /// </summary>
    public TimeSpan? MaxTime
    {
        get => (TimeSpan?)GetValue(MaxTimeProperty);
        set => SetValue(MaxTimeProperty, value);
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

    public PtrpTimePicker()
    {
        InitializeComponent();
    }

    private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PtrpTimePicker picker)
        {
            picker.ValidateTime();
        }
    }

    private static void OnValidationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PtrpTimePicker picker)
        {
            picker.ValidateTime();
        }
    }

    private void ValidateTime()
    {
        ValidationError = string.Empty;
        HasValidationError = false;

        // Required validation
        if (IsRequired && !SelectedTime.HasValue)
        {
            ValidationError = "L'ora è obbligatoria";
            HasValidationError = true;
            return;
        }

        if (!SelectedTime.HasValue)
            return;

        var time = SelectedTime.Value;

        // Min time validation
        if (MinTime.HasValue && time < MinTime.Value)
        {
            ValidationError = $"L'ora deve essere successiva alle {MinTime.Value:hh\\:mm}";
            HasValidationError = true;
            return;
        }

        // Max time validation
        if (MaxTime.HasValue && time > MaxTime.Value)
        {
            ValidationError = $"L'ora deve essere precedente alle {MaxTime.Value:hh\\:mm}";
            HasValidationError = true;
            return;
        }
    }

    /// <summary>
    /// Valida manualmente il controllo e restituisce true se valido.
    /// </summary>
    public bool Validate()
    {
        ValidateTime();
        return !HasValidationError;
    }
}
