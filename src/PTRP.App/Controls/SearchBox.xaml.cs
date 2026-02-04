using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace PTRP.App.Controls;

/// <summary>
/// Controllo di ricerca con debouncing automatico e pulsante clear.
/// </summary>
/// <remarks>
/// - Debouncing: 300ms di ritardo prima di triggerare SearchCommand dopo l'ultima digitazione
/// - Pulsante Clear: appare automaticamente quando c'è testo e pulisce il campo
/// - SearchCommand: comando eseguito dopo il debouncing
/// </remarks>
/// <example>
/// <code>
/// &lt;controls:SearchBox 
///     SearchText="{Binding SearchTerm}" 
///     SearchCommand="{Binding SearchCommand}"
///     Watermark="Cerca pazienti..." /&gt;
/// </code>
/// </example>
public partial class SearchBox : UserControl
{
    private DispatcherTimer? _debounceTimer;
    private const int DebounceMilliseconds = 300;

    public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.Register(
            nameof(SearchText),
            typeof(string),
            typeof(SearchBox),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSearchTextChanged));

    public static readonly DependencyProperty WatermarkProperty =
        DependencyProperty.Register(
            nameof(Watermark),
            typeof(string),
            typeof(SearchBox),
            new PropertyMetadata("Cerca..."));

    public static readonly DependencyProperty SearchCommandProperty =
        DependencyProperty.Register(
            nameof(SearchCommand),
            typeof(ICommand),
            typeof(SearchBox),
            new PropertyMetadata(null));

    public static readonly DependencyProperty HasTextProperty =
        DependencyProperty.Register(
            nameof(HasText),
            typeof(bool),
            typeof(SearchBox),
            new PropertyMetadata(false));

    /// <summary>
    /// Testo di ricerca.
    /// </summary>
    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    /// <summary>
    /// Testo placeholder.
    /// </summary>
    public string Watermark
    {
        get => (string)GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    /// <summary>
    /// Comando eseguito dopo il debouncing (300ms).
    /// </summary>
    public ICommand? SearchCommand
    {
        get => (ICommand?)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    /// <summary>
    /// Indica se c'è testo nella searchbox (per mostrare il pulsante Clear).
    /// </summary>
    public bool HasText
    {
        get => (bool)GetValue(HasTextProperty);
        private set => SetValue(HasTextProperty, value);
    }

    public SearchBox()
    {
        InitializeComponent();
        InitializeDebounceTimer();
    }

    private void InitializeDebounceTimer()
    {
        _debounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(DebounceMilliseconds)
        };
        _debounceTimer.Tick += OnDebounceTimerTick;
    }

    private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SearchBox searchBox)
        {
            searchBox.HasText = !string.IsNullOrWhiteSpace(searchBox.SearchText);
        }
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        // Reset debounce timer ogni volta che l'utente digita
        _debounceTimer?.Stop();
        _debounceTimer?.Start();
    }

    private void OnDebounceTimerTick(object? sender, EventArgs e)
    {
        _debounceTimer?.Stop();

        // Trigger SearchCommand dopo il debouncing
        if (SearchCommand?.CanExecute(SearchText) == true)
        {
            SearchCommand.Execute(SearchText);
        }
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        SearchText = string.Empty;
        InternalTextBox.Focus();

        // Trigger immediato quando si pulisce
        if (SearchCommand?.CanExecute(string.Empty) == true)
        {
            SearchCommand.Execute(string.Empty);
        }
    }
}
