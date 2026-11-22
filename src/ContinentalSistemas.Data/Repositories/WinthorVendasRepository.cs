using ContinentalSistemas.Application.Dtos;
using ContinentalSistemas.Application.Interfaces.Repositories;
using ContinentalSistemas.Data.WinthorQueries;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace ContinentalSistemas.Data.Repositories;

public class WinthorVendasRepository : IWinthorVendasRepository
{
    private readonly IConfiguration _configuration;

    public WinthorVendasRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private (string UserId, string Password, string Sid) ObterConfiguracaoOracle()
    {
        var userId = _configuration["WinthorOracle:UserId"];
        var password = _configuration["WinthorOracle:Password"];
        var sid = _configuration["WinthorOracle:SID"];

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(sid))
            throw new InvalidOperationException("As configurações WinthorOracle:UserId, WinthorOracle:Password e WinthorOracle:SID são obrigatórias.");

        return (userId, password, sid);
    }

    public async Task<List<long>> ObterNumerosVendasExistentesAsync(ServidorDto servidor, List<long> numerosVendas)
    {
        var resultado = new List<long>();
        if (numerosVendas == null || numerosVendas.Count == 0)
            return resultado;

        var configuracao = ObterConfiguracaoOracle();
        var connectionString = new OracleConnectionStringBuilder
        {
            UserID = configuracao.UserId,
            Password = configuracao.Password,
            DataSource = $"(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={servidor.Ip})(PORT=1521))(CONNECT_DATA=(SID = {configuracao.Sid})))"
        }.ConnectionString;

        using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        var parametros = numerosVendas.Select((_, index) => $":numeroVenda{index}").ToArray();
        using var command = new OracleCommand(VendaWinthorQueries.NumerosVendasExistentes(string.Join(", ", parametros)), connection)
        {
            CommandTimeout = 30
        };

        for (var index = 0; index < numerosVendas.Count; index++)
        {
            command.Parameters.Add(new OracleParameter($"numeroVenda{index}", OracleDbType.Int64)
            {
                Value = numerosVendas[index]
            });
        }

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            resultado.Add(Convert.ToInt64(reader["NUMPED"]));
        }

        return resultado;
    }

    public async Task<List<ExtracaoVendaItemDto>> ExtrairItensVendasPorEcfAsync(ServidorDto servidor, List<long> numerosVendas)
    {
        var resultado = new List<ExtracaoVendaItemDto>();

        if (numerosVendas == null || numerosVendas.Count == 0)
            return resultado;

        var configuracao = ObterConfiguracaoOracle();
        var connectionString = new OracleConnectionStringBuilder
        {
            UserID = configuracao.UserId,
            Password = configuracao.Password,
            DataSource = $"(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={servidor.Ip})(PORT=1521))(CONNECT_DATA=(SID = {configuracao.Sid})))"
        }.ConnectionString;

        using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        var parametros = numerosVendas.Select((_, index) => $":numeroVenda{index}").ToArray();
        using var command = new OracleCommand(VendaWinthorQueries.VendasPorPeriodo(string.Join(", ", parametros)), connection)
        {
            CommandTimeout = 30
        };

        for (var index = 0; index < numerosVendas.Count; index++)
        {
            command.Parameters.Add(new OracleParameter($"numeroVenda{index}", OracleDbType.Int64)
            {
                Value = numerosVendas[index]
            });
        }

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            resultado.Add(new ExtracaoVendaItemDto
            {
                ProCodigo = string.Empty,
                CodR3Magazine = reader["COD_R3MAGAZINE"]?.ToString() ?? string.Empty,
                ProDescricao = reader["PRO_DESCRICAO"]?.ToString() ?? string.Empty,
                ProReferencia = reader["PRO_REFERENCIA"]?.ToString() ?? string.Empty,
                Quantidade = reader["QUANTIDADE"] != DBNull.Value ? Convert.ToDecimal(reader["QUANTIDADE"]) : 0,
                Venda = string.Empty,
                Custo = reader["CUSTO"] != DBNull.Value ? Convert.ToDecimal(reader["CUSTO"]) : 0,
                PrecoLoja = string.Empty,
                CustoTotal = reader["CUSTO_TOTAL"] != DBNull.Value ? Convert.ToDecimal(reader["CUSTO_TOTAL"]) : 0
            });
        }

        return resultado;
    }
}
