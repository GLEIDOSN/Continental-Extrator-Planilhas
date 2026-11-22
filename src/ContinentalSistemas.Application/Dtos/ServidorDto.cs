namespace ContinentalSistemas.Application.Dtos;

public class ServidorDto
{
    public int Id { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public string Ip { get; set; } = string.Empty;

    public string CaminhoDb { get; set; } = string.Empty;

    public bool Ativo { get; set; }
}