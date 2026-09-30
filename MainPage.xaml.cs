using System.Globalization;
namespace Calculatrice;

public partial class MainPage : ContentPage
{
    static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    const string Err = "Erreur";
    decimal? _a; string _op = ""; string _cur = "0"; bool _reset;

    public MainPage() => InitializeComponent();

    static string Fmt(decimal d) => d.ToString("0.##########", Inv);
    void Refresh() => ResultLabel.Text = _cur;
    bool IsErr => _cur == Err;
    decimal Cur => decimal.Parse(_cur, Inv);

    void OnDigit(object s, EventArgs e)
    {
        var d = ((Button)s).Text;
        if (IsErr) OnClear(s, e);
        if (_reset || _cur == "0") { _cur = d; _reset = false; }
        else if (_cur.Length < 15) _cur += d;
        Refresh();
    }

    void OnDot(object s, EventArgs e)
    {
        if (IsErr) OnClear(s, e);
        if (_reset) { _cur = "0"; _reset = false; }
        if (!_cur.Contains('.')) _cur += ".";
        Refresh();
    }

    void OnOperator(object s, EventArgs e)
    {
        if (IsErr) return;
        if (_a.HasValue && !_reset && _op != "") { if (!Compute()) return; }
        else _a = Cur;
        _op = ((Button)s).Text; _reset = true;
        OperationLabel.Text = $"{Fmt(_a!.Value)} {_op}";
    }

    void OnEquals(object s, EventArgs e)
    {
        if (IsErr || _op == "" || !_a.HasValue) return;
        var expr = $"{Fmt(_a.Value)} {_op} {Fmt(Cur)} =";
        if (Compute()) OperationLabel.Text = expr;
        _op = ""; _a = null; _reset = true;
    }

    bool Compute()
    {
        try
        {
            var b = Cur; decimal r;
            switch (_op)
            {
                case "+": r = _a!.Value + b; break;
                case "−": r = _a!.Value - b; break;
                case "×": r = _a!.Value * b; break;
                case "÷":
                    if (b == 0) return Fail("Division par zéro impossible");
                    r = _a!.Value / b; break;
                default: return true;
            }
            _a = r; _cur = Fmt(r); Refresh(); return true;
        }
        catch (OverflowException) { return Fail("Nombre trop grand"); }
    }

    bool Fail(string msg)
    {
        _cur = Err; _a = null; _op = ""; _reset = true;
        Refresh(); OperationLabel.Text = msg; return false;
    }

    void OnClear(object s, EventArgs e)
    { _a = null; _op = ""; _cur = "0"; _reset = false; OperationLabel.Text = ""; Refresh(); }

    void OnBackspace(object s, EventArgs e)
    {
        if (IsErr || _reset) return;
        _cur = _cur.Length <= 1 || (_cur.Length == 2 && _cur[0] == '-') ? "0" : _cur[..^1];
        Refresh();
    }

    void OnSign(object s, EventArgs e)
    {
        if (IsErr || _cur == "0") return;
        _cur = _cur.StartsWith('-') ? _cur[1..] : "-" + _cur; Refresh();
    }

    void OnPercent(object s, EventArgs e)
    {
        if (IsErr) return;
        _cur = Fmt(_a.HasValue && _op != "" ? _a.Value * Cur / 100 : Cur / 100);
        Refresh();
    }

    void OnSquare(object s, EventArgs e)
    {
        if (IsErr) return;
        try { _cur = Fmt(Cur * Cur); Refresh(); } catch (OverflowException) { Fail("Nombre trop grand"); }
    }

    void OnSqrt(object s, EventArgs e)
    {
        if (IsErr) return;
        if (Cur < 0) { Fail("Racine d'un nombre négatif"); return; }
        _cur = Fmt((decimal)Math.Sqrt((double)Cur)); Refresh();
    }
}
