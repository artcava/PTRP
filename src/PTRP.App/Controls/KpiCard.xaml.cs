using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PTRP.App.Controls;

/// <summary>
/// Card per visualizzare KPI (Key Performance Indicator) con icona, valore, titolo e trend opzionale.
/// </summary>
/// <remarks>
/// Perfetta per dashboard e report.
/// - Icona: PackIconKind da MaterialDesign
/// - Valore: string o numero formattato
/// - Titolo: descrizione del KPI
/// - Trend: valore decimale opzionale (es: +5.2, -3.1) con icona auto-determinata
/// - CardColor: colore di sfondo personalizzabile
/// </remarks>
/// <example>
/// <code>
/// &lt;controls:KpiCard 
///     Title="Pazienti Attivi" 
///     Value="48"
///     Icon="Account"
///     CardColor="#2196F3"
///     TrendValue="5.2" /&gt;
/// </code>
/// </example>
public partial class KpiCard : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(KpiCard),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(string),
            typeof(KpiCard),
            new PropertyMetadata("0"));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(PackIconKind),
            typeof(KpiCard),
            new PropertyMetadata(PackIconKind.ChartLine));

    public static readonly DependencyProperty CardColorProperty =
        DependencyProperty.Register(
            nameof(CardColor),
            typeof(Brush),
            typeof(KpiCard),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(33, 150, 243)))) // Material Blue 500
;

    public static readonly DependencyProperty TrendValueProperty =
        DependencyProperty.Register(
            nameof(TrendValue),
            typeof(decimal?),
            typeof(KpiCard),
            new PropertyMetadata(null, OnTrendValueChanged));

    public static readonly DependencyProperty TrendDisplayProperty =
        DependencyProperty.Register(
            nameof(TrendDisplay),
            typeof(string),
            typeof(KpiCard),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty TrendIconProperty =
        DependencyProperty.Register(
            nameof(TrendIcon),
            typeof(PackIconKind),
            typeof(KpiCard),
            new PropertyMetadata(PackIconKind.TrendingUp));

    public static readonly DependencyProperty HasTrendProperty =
        DependencyProperty.Register(
            nameof(HasTrend),
            typeof(bool),
            typeof(KpiCard),
            new PropertyMetadata(false));

    /// <summary>
    /// Titolo del KPI.
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Valore principale del KPI (formattato come string).
    /// </summary>
    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Icona MaterialDesign da visualizzare.
    /// </summary>
    public PackIconKind Icon
    {
        get => (PackIconKind)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Colore di sfondo della card.
    /// </summary>
    public Brush CardColor
    {
        get => (Brush)GetValue(CardColorProperty);
        set => SetValue(CardColorProperty, value);
    }

    /// <summary>
    /// Valore del trend (es: +5.2 per +5.2%, -3.1 per -3.1%).
    /// Se null, il trend non viene visualizzato.
    /// </summary>
    public decimal? TrendValue
    {
        get => (decimal?)GetValue(TrendValueProperty);
        set => SetValue(TrendValueProperty, value);
    }

    /// <summary>
    /// Testo del trend formattato (es: "+5.2%", "-3.1%").
    /// </summary>
    public string TrendDisplay
    {
        get => (string)GetValue(TrendDisplayProperty);
        private set => SetValue(TrendDisplayProperty, value);
    }

    /// <summary>
    /// Icona del trend (auto-determinata: TrendingUp se positivo, TrendingDown se negativo).
    /// </summary>
    public PackIconKind TrendIcon
    {
        get => (PackIconKind)GetValue(TrendIconProperty);
        private set => SetValue(TrendIconProperty, value);
    }

    /// <summary>
    /// Indica se il trend è presente.
    /// </summary>
    public bool HasTrend
    {
        get => (bool)GetValue(HasTrendProperty);
        private set => SetValue(HasTrendProperty, value);
    }

    public KpiCard()
    {
        InitializeComponent();
    }

    private static void OnTrendValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KpiCard card)
        {
            card.UpdateTrend();
        }
    }

    private void UpdateTrend()
    {
        if (!TrendValue.HasValue)
        {
            HasTrend = false;
            TrendDisplay = string.Empty;
            return;
        }

        HasTrend = true;
        var value = TrendValue.Value;

        // Determina icona
        TrendIcon = value >= 0 ? PackIconKind.TrendingUp : PackIconKind.TrendingDown;

        // Formatta display
        var sign = value >= 0 ? "+" : string.Empty;
        TrendDisplay = $"{sign}{value:F1}%";
    }
}
