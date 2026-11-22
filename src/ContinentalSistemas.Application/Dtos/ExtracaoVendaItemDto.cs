namespace ContinentalSistemas.Application.Dtos;

public class ExtracaoVendaItemDto
{
    public string ProCodigo { get; set; } = string.Empty;

    public string CodR3Magazine { get; set; } = string.Empty;

    public string ProDescricao { get; set; } = string.Empty;

    public string ProReferencia { get; set; } = string.Empty;

    public decimal Quantidade { get; set; }

    public string Venda { get; set; } = string.Empty;

    public decimal Custo { get; set; }

    public string PrecoLoja { get; set; } = string.Empty;

    public decimal CustoTotal { get; set; }
}
