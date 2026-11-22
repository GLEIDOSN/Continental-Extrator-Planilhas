using ContinentalSistemas.Application.Dtos;

namespace ContinentalSistemas.Application.Interfaces.Services;

public interface IExtracaoVendasService
{
    Task<List<long>> ObterNumerosVendasInexistentesAsync(ServidorDto servidor, List<long> numerosEcf);

    Task<List<ExtracaoVendaItemDto>> ExtrairItensVendasPorEcfAsync(ServidorDto servidor, List<long> numerosEcf);

    byte[] GerarPlanilhaExcel(List<ExtracaoVendaItemDto> itensExtraidos);
}
