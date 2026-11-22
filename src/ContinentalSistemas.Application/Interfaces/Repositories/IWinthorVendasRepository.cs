using ContinentalSistemas.Application.Dtos;

namespace ContinentalSistemas.Application.Interfaces.Repositories;

public interface IWinthorVendasRepository
{
    Task<List<long>> ObterNumerosVendasExistentesAsync(ServidorDto servidor, List<long> numerosVendas);

    Task<List<ExtracaoVendaItemDto>> ExtrairItensVendasPorEcfAsync(ServidorDto servidor, List<long> numerosVendas);
}
