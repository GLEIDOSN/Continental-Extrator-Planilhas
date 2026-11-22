using ContinentalSistemas.Application.Dtos;
using ContinentalSistemas.Data.Repositories;
using ContinentalSistemas.Data.WinthorQueries;
using Microsoft.Extensions.Configuration;

namespace ContinentalSistemas.Data.tests.Repositories;

[TestFixture]
public class WinthorVendasRepositoryTests
{
    private static readonly ServidorDto Servidor = new() { Descricao = "Winthor", Ip = "localhost" };

    [TestCase(true)]
    [TestCase(false)]
    public async Task ObterNumerosVendasExistentesAsync_SemNumeros_RetornaListaVaziaSemAcessarConfiguracao(bool listaNula)
    {
        var repository = CriarRepositorio(new Dictionary<string, string?>());

        var resultado = await repository.ObterNumerosVendasExistentesAsync(Servidor, listaNula ? null! : []);

        Assert.That(resultado, Is.Empty);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task ExtrairItensVendasPorEcfAsync_SemNumeros_RetornaListaVaziaSemAcessarConfiguracao(bool listaNula)
    {
        var repository = CriarRepositorio(new Dictionary<string, string?>());

        var resultado = await repository.ExtrairItensVendasPorEcfAsync(Servidor, listaNula ? null! : []);

        Assert.That(resultado, Is.Empty);
    }

    [TestCase("UserId", null)]
    [TestCase("UserId", "")]
    [TestCase("UserId", "  ")]
    [TestCase("Password", null)]
    [TestCase("Password", "")]
    [TestCase("Password", "  ")]
    [TestCase("SID", null)]
    [TestCase("SID", "")]
    [TestCase("SID", "  ")]
    public void ObterNumerosVendasExistentesAsync_ConfiguracaoInvalida_LancaErroAntesDaConexao(string chave, string? valor)
    {
        var repository = CriarRepositorioComConfiguracaoInvalida(chave, valor);

        var erro = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await repository.ObterNumerosVendasExistentesAsync(Servidor, [123]));

        Assert.That(erro!.Message, Does.Contain("WinthorOracle:UserId").And.Contain("WinthorOracle:Password").And.Contain("WinthorOracle:SID"));
    }

    [TestCase("UserId", null)]
    [TestCase("UserId", "")]
    [TestCase("UserId", "  ")]
    [TestCase("Password", null)]
    [TestCase("Password", "")]
    [TestCase("Password", "  ")]
    [TestCase("SID", null)]
    [TestCase("SID", "")]
    [TestCase("SID", "  ")]
    public void ExtrairItensVendasPorEcfAsync_ConfiguracaoInvalida_LancaErroAntesDaConexao(string chave, string? valor)
    {
        var repository = CriarRepositorioComConfiguracaoInvalida(chave, valor);

        var erro = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await repository.ExtrairItensVendasPorEcfAsync(Servidor, [123]));

        Assert.That(erro!.Message, Does.Contain("WinthorOracle:UserId").And.Contain("WinthorOracle:Password").And.Contain("WinthorOracle:SID"));
    }

    [Test]
    public void NumerosVendasExistentes_UsaParametrosEExcluiVendasCanceladas()
    {
        var sql = VendaWinthorQueries.NumerosVendasExistentes(":numeroVenda0, :numeroVenda1");

        Assert.Multiple(() =>
        {
            Assert.That(sql, Does.Contain("NUMPED IN (:numeroVenda0, :numeroVenda1)"));
            Assert.That(sql, Does.Contain("POSICAO <> 'C'"));
            Assert.That(sql, Does.Contain("FROM PCPEDC"));
        });
    }

    [Test]
    public void VendasPorPeriodo_UsaParametrosAgrupaItensEExcluiVendasCanceladas()
    {
        var sql = VendaWinthorQueries.VendasPorPeriodo(":numeroVenda0, :numeroVenda1");

        Assert.Multiple(() =>
        {
            Assert.That(sql, Does.Contain("C.NUMPED IN (:numeroVenda0, :numeroVenda1)"));
            Assert.That(sql, Does.Contain("C.POSICAO <> 'C'"));
            Assert.That(sql, Does.Contain("SUM(I.QT) AS QUANTIDADE"));
            Assert.That(sql, Does.Contain("SUM(I.QT * NVL(I.PVENDA, 0)) AS CUSTO_TOTAL"));
            Assert.That(sql, Does.Contain("GROUP BY"));
        });
    }

    private static WinthorVendasRepository CriarRepositorioComConfiguracaoInvalida(string chave, string? valor)
    {
        var configuracao = new Dictionary<string, string?>
        {
            ["WinthorOracle:UserId"] = "usuario-teste",
            ["WinthorOracle:Password"] = "senha-teste",
            ["WinthorOracle:SID"] = "sid-teste"
        };
        configuracao[$"WinthorOracle:{chave}"] = valor;
        return CriarRepositorio(configuracao);
    }

    private static WinthorVendasRepository CriarRepositorio(Dictionary<string, string?> configuracao)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configuracao).Build();
        return new WinthorVendasRepository(configuration);
    }
}
