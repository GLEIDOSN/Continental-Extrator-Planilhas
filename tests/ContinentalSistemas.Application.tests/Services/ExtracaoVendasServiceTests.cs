using ContinentalSistemas.Application.Dtos;
using ContinentalSistemas.Application.Interfaces.Repositories;
using ContinentalSistemas.Application.Services;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace ContinentalSistemas.Application.tests.Services;

[TestFixture]
public class ExtracaoVendasServiceTests
{
    private RepositorioFalso repositorio = null!;
    private ExtracaoVendasService service = null!;
    private ServidorDto servidor = null!;

    [SetUp]
    public void Setup()
    {
        repositorio = new RepositorioFalso();
        service = new ExtracaoVendasService(repositorio);
        servidor = new ServidorDto { Id = 1, Descricao = "Winthor" };
    }

    [Test]
    public async Task ObterNumerosVendasInexistentesAsync_RetornaApenasAusentesNaOrdemInformada()
    {
        var numeros = new List<long> { 10, 20, 30, 20, 40 };
        repositorio.NumerosExistentes = [10, 30, 10];

        var resultado = await service.ObterNumerosVendasInexistentesAsync(servidor, numeros);

        Assert.Multiple(() =>
        {
            Assert.That(resultado, Is.EqualTo(new long[] { 20, 20, 40 }));
            Assert.That(repositorio.ServidorRecebido, Is.SameAs(servidor));
            Assert.That(repositorio.NumerosRecebidos, Is.SameAs(numeros));
            Assert.That(repositorio.ConsultasExistentes, Is.EqualTo(1));
            Assert.That(repositorio.ConsultasItens, Is.Zero);
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task ObterNumerosVendasInexistentesAsync_QuandoTodosExistemOuListaEstaVazia_RetornaListaVazia(bool listaVazia)
    {
        var numeros = listaVazia ? new List<long>() : new List<long> { 10, 20 };
        repositorio.NumerosExistentes = [10, 20];

        var resultado = await service.ObterNumerosVendasInexistentesAsync(servidor, numeros);

        Assert.That(resultado, Is.Empty);
        Assert.That(repositorio.NumerosRecebidos, Is.SameAs(numeros));
    }

    [Test]
    public async Task ObterNumerosVendasInexistentesAsync_QuandoNenhumExiste_RetornaTodos()
    {
        var numeros = new List<long> { 10, 20 };

        var resultado = await service.ObterNumerosVendasInexistentesAsync(servidor, numeros);

        Assert.That(resultado, Is.EqualTo(numeros));
    }

    [Test]
    public async Task ExtrairItensVendasPorEcfAsync_RetornaItensEEncaminhaParametrosAoRepositorio()
    {
        var numeros = new List<long> { 10, 20 };
        var itens = new List<ExtracaoVendaItemDto>
        {
            new() { CodR3Magazine = "123", Quantidade = 2 },
            new() { CodR3Magazine = "456", Quantidade = 3 }
        };
        repositorio.Itens = itens;

        var resultado = await service.ExtrairItensVendasPorEcfAsync(servidor, numeros);

        Assert.Multiple(() =>
        {
            Assert.That(resultado, Is.SameAs(itens));
            Assert.That(repositorio.ServidorRecebido, Is.SameAs(servidor));
            Assert.That(repositorio.NumerosRecebidos, Is.SameAs(numeros));
            Assert.That(repositorio.ConsultasItens, Is.EqualTo(1));
            Assert.That(repositorio.ConsultasExistentes, Is.Zero);
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Consultas_PropagamErroDoRepositorio(bool verificarExistentes)
    {
        var excecao = new InvalidOperationException("Falha na consulta");
        repositorio.Erro = excecao;
        var numeros = new List<long> { 10 };

        var capturada = verificarExistentes
            ? Assert.ThrowsAsync<InvalidOperationException>(async () => await service.ObterNumerosVendasInexistentesAsync(servidor, numeros))
            : Assert.ThrowsAsync<InvalidOperationException>(async () => await service.ExtrairItensVendasPorEcfAsync(servidor, numeros));

        Assert.That(capturada, Is.SameAs(excecao));
    }

    [Test]
    public void GerarPlanilhaExcel_SemItens_GeraPlanilhaComCabecalhos()
    {
        using var workbook = AbrirPlanilha(service.GerarPlanilhaExcel([]));
        var sheet = workbook.GetSheetAt(0);
        string[] cabecalhos = ["PRO_CODIGO", "COD_R3MAGAZINE", "PRO_DESCRICAO", "PRO_REFERENCIA", "QUANTIDADE", "VENDA", "CUSTO", "PRECO_LOJA", "CUSTO_TOTAL"];

        Assert.Multiple(() =>
        {
            Assert.That(workbook.NumberOfSheets, Is.EqualTo(1));
            Assert.That(sheet.SheetName, Is.EqualTo("Sheet 1"));
            Assert.That(sheet.PhysicalNumberOfRows, Is.EqualTo(1));
            Assert.That(sheet.GetRow(0).LastCellNum, Is.EqualTo(cabecalhos.Length));
            for (var i = 0; i < cabecalhos.Length; i++)
                Assert.That(sheet.GetRow(0).GetCell(i).StringCellValue, Is.EqualTo(cabecalhos[i]), $"Cabeçalho {i}");
        });
    }

    [Test]
    public void GerarPlanilhaExcel_ComItens_PreservaOrdemValoresETiposDasCelulas()
    {
        var itens = new List<ExtracaoVendaItemDto>
        {
            new() { ProCodigo = "001", CodR3Magazine = "000123", ProDescricao = "Produto A", ProReferencia = "789", Quantidade = 2.5m, Venda = "V1", Custo = 12.34m, PrecoLoja = "99", CustoTotal = 30.85m },
            new() { ProCodigo = "002", CodR3Magazine = "456", ProDescricao = "Produto B", ProReferencia = "012", Quantidade = 3m, Venda = "V2", Custo = 4m, CustoTotal = 12m }
        };

        using var workbook = AbrirPlanilha(service.GerarPlanilhaExcel(itens));
        var sheet = workbook.GetSheetAt(0);
        var primeira = sheet.GetRow(1);
        var segunda = sheet.GetRow(2);

        Assert.Multiple(() =>
        {
            Assert.That(sheet.PhysicalNumberOfRows, Is.EqualTo(3));
            Assert.That(primeira.GetCell(0).StringCellValue, Is.EqualTo("001"));
            Assert.That(primeira.GetCell(1).StringCellValue, Is.EqualTo("000123"));
            Assert.That(primeira.GetCell(2).StringCellValue, Is.EqualTo("Produto A"));
            Assert.That(primeira.GetCell(3).StringCellValue, Is.EqualTo("789"));
            Assert.That(primeira.GetCell(4).CellType, Is.EqualTo(CellType.Numeric));
            Assert.That(primeira.GetCell(4).NumericCellValue, Is.EqualTo(2.5));
            Assert.That(primeira.GetCell(5).StringCellValue, Is.EqualTo("V1"));
            Assert.That(primeira.GetCell(6).NumericCellValue, Is.EqualTo(12.34).Within(0.00001));
            Assert.That(primeira.GetCell(7).CellType, Is.EqualTo(CellType.Numeric));
            Assert.That(primeira.GetCell(7).NumericCellValue, Is.Zero);
            Assert.That(primeira.GetCell(8).NumericCellValue, Is.EqualTo(30.85).Within(0.00001));
            Assert.That(segunda.GetCell(0).StringCellValue, Is.EqualTo("002"));
            Assert.That(segunda.GetCell(1).StringCellValue, Is.EqualTo("456"));
            Assert.That(segunda.GetCell(2).StringCellValue, Is.EqualTo("Produto B"));
            Assert.That(segunda.GetCell(3).StringCellValue, Is.EqualTo("012"));
            Assert.That(segunda.GetCell(4).NumericCellValue, Is.EqualTo(3));
            Assert.That(segunda.GetCell(5).StringCellValue, Is.EqualTo("V2"));
            Assert.That(segunda.GetCell(6).NumericCellValue, Is.EqualTo(4));
            Assert.That(segunda.GetCell(7).NumericCellValue, Is.Zero);
            Assert.That(segunda.GetCell(8).NumericCellValue, Is.EqualTo(12));
        });
    }

    private static HSSFWorkbook AbrirPlanilha(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return new HSSFWorkbook(stream);
    }

    private sealed class RepositorioFalso : IWinthorVendasRepository
    {
        public List<long> NumerosExistentes { get; set; } = [];
        public List<ExtracaoVendaItemDto> Itens { get; set; } = [];
        public Exception? Erro { get; set; }
        public ServidorDto? ServidorRecebido { get; private set; }
        public List<long>? NumerosRecebidos { get; private set; }
        public int ConsultasExistentes { get; private set; }
        public int ConsultasItens { get; private set; }

        public Task<List<long>> ObterNumerosVendasExistentesAsync(ServidorDto servidor, List<long> numerosVendas)
        {
            ConsultasExistentes++;
            ServidorRecebido = servidor;
            NumerosRecebidos = numerosVendas;
            return Erro is null ? Task.FromResult(NumerosExistentes) : Task.FromException<List<long>>(Erro);
        }

        public Task<List<ExtracaoVendaItemDto>> ExtrairItensVendasPorEcfAsync(ServidorDto servidor, List<long> numerosVendas)
        {
            ConsultasItens++;
            ServidorRecebido = servidor;
            NumerosRecebidos = numerosVendas;
            return Erro is null ? Task.FromResult(Itens) : Task.FromException<List<ExtracaoVendaItemDto>>(Erro);
        }
    }
}
