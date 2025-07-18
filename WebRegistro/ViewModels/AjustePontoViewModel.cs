public class AjustePontoViewModel
{
    public int Dia { get; set; }
    public string Cpf { get; set; }
    public string Competencia { get; set; } // "yyyy-MM"
    public string? Entrada { get; set; } // "HH:mm"
    public string? SaidaAlmoco { get; set; }
    public string? VoltaAlmoco { get; set; }
    public string? Saida { get; set; }
    public string Motivo { get; set; }
}