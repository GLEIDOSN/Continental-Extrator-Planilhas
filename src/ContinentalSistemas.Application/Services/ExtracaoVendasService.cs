using ContinentalSistemas.Application.Dtos;
using ContinentalSistemas.Application.Interfaces.Repositories;
using ContinentalSistemas.Application.Interfaces.Services;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace ContinentalSistemas.Application.Services;

public class ExtracaoVendasService : IExtracaoVendasService
{
    private readonly IWinthorVendasRepository _winthorVendasRepository;

    public ExtracaoVendasService(
        IWinthorVendasRepository winthorVendasRepository)
    {
        _winthorVendasRepository = winthorVendasRepository;
    }

    public async Task<List<long>> ObterNumerosVendasInexistentesAsync(ServidorDto servidor, List<long> numerosEcf)
    {
        var numerosExistentes = await _winthorVendasRepository.ObterNumerosVendasExistentesAsync(servidor, numerosEcf);

        var numerosExistentesSet = numerosExistentes.ToHashSet();
        return numerosEcf.Where(numero => !numerosExistentesSet.Contains(numero)).ToList();
    }

    public async Task<List<ExtracaoVendaItemDto>> ExtrairItensVendasPorEcfAsync(ServidorDto servidor, List<long> numerosEcf)
    {
        return await _winthorVendasRepository.ExtrairItensVendasPorEcfAsync(servidor, numerosEcf);
    }

    public byte[] GerarPlanilhaExcel(List<ExtracaoVendaItemDto> itensExtraidos)
    {
        using var workbook = new HSSFWorkbook();
        var sheet = workbook.CreateSheet("Sheet 1");
        var dataFormat = workbook.CreateDataFormat();

        var fontPadrao = workbook.CreateFont();
        fontPadrao.FontName = "Arial";
        fontPadrao.FontHeightInPoints = 10;

        var fontHeader = workbook.CreateFont();
        fontHeader.FontName = "Arial";
        fontHeader.FontHeightInPoints = 10;
        fontHeader.IsBold = true;

        var styleHeader = workbook.CreateCellStyle();
        styleHeader.SetFont(fontHeader);
        styleHeader.Alignment = HorizontalAlignment.Center;
        styleHeader.VerticalAlignment = VerticalAlignment.Center;
        styleHeader.FillForegroundColor = IndexedColors.Grey25Percent.Index;
        styleHeader.FillPattern = FillPattern.SolidForeground;
        styleHeader.BorderTop = BorderStyle.Thin;
        styleHeader.BorderBottom = BorderStyle.Thin;
        styleHeader.BorderLeft = BorderStyle.Thin;
        styleHeader.BorderRight = BorderStyle.Thin;

        var styleTexto = workbook.CreateCellStyle();
        styleTexto.SetFont(fontPadrao);
        styleTexto.Alignment = HorizontalAlignment.Left;
        styleTexto.VerticalAlignment = VerticalAlignment.Center;
        styleTexto.DataFormat = dataFormat.GetFormat("@");
        styleTexto.BorderTop = BorderStyle.Thin;
        styleTexto.BorderBottom = BorderStyle.Thin;
        styleTexto.BorderLeft = BorderStyle.Thin;
        styleTexto.BorderRight = BorderStyle.Thin;

        var styleNumero = workbook.CreateCellStyle();
        styleNumero.SetFont(fontPadrao);
        styleNumero.Alignment = HorizontalAlignment.Right;
        styleNumero.VerticalAlignment = VerticalAlignment.Center;
        styleNumero.DataFormat = dataFormat.GetFormat("#,##0");
        styleNumero.BorderTop = BorderStyle.Thin;
        styleNumero.BorderBottom = BorderStyle.Thin;
        styleNumero.BorderLeft = BorderStyle.Thin;
        styleNumero.BorderRight = BorderStyle.Thin;

        var styleMoeda = workbook.CreateCellStyle();
        styleMoeda.SetFont(fontPadrao);
        styleMoeda.Alignment = HorizontalAlignment.Right;
        styleMoeda.VerticalAlignment = VerticalAlignment.Center;
        styleMoeda.DataFormat = dataFormat.GetFormat("\"R$\" #,##0.00");
        styleMoeda.BorderTop = BorderStyle.Thin;
        styleMoeda.BorderBottom = BorderStyle.Thin;
        styleMoeda.BorderLeft = BorderStyle.Thin;
        styleMoeda.BorderRight = BorderStyle.Thin;

        string[] cabecalhos = { "PRO_CODIGO", "COD_R3MAGAZINE", "PRO_DESCRICAO", "PRO_REFERENCIA", "QUANTIDADE", "VENDA", "CUSTO", "PRECO_LOJA", "CUSTO_TOTAL" };
        var headerRow = sheet.CreateRow(0);

        for (int i = 0; i < cabecalhos.Length; i++)
        {
            var cell = headerRow.CreateCell(i);
            cell.SetCellValue(cabecalhos[i]);
            cell.CellStyle = styleHeader;
        }

        int rowIndex = 1;
        foreach (var item in itensExtraidos)
        {
            var row = sheet.CreateRow(rowIndex++);

            var c0 = row.CreateCell(0);
            c0.SetCellValue(item.ProCodigo);
            c0.CellStyle = styleTexto;

            var c1 = row.CreateCell(1);
            c1.SetCellValue(item.CodR3Magazine);
            c1.CellStyle = styleTexto;

            var c2 = row.CreateCell(2);
            c2.SetCellValue(item.ProDescricao);
            c2.CellStyle = styleTexto;

            var c3 = row.CreateCell(3);
            c3.SetCellValue(item.ProReferencia);
            c3.CellStyle = styleTexto;

            var c4 = row.CreateCell(4);
            c4.SetCellValue((double)item.Quantidade);
            c4.CellStyle = styleNumero;

            var c5 = row.CreateCell(5);
            c5.SetCellValue(item.Venda);
            c5.CellStyle = styleTexto;

            var c6 = row.CreateCell(6);
            c6.SetCellValue((double)item.Custo);
            c6.CellStyle = styleMoeda;

            var c7 = row.CreateCell(7);
            c7.SetCellValue(0);
            c7.CellStyle = styleMoeda;

            var c8 = row.CreateCell(8);
            c8.SetCellValue((double)item.CustoTotal);
            c8.CellStyle = styleMoeda;
        }

        sheet.SetColumnWidth(0, 15 * 256);
        sheet.SetColumnWidth(1, 18 * 256);
        sheet.SetColumnWidth(2, 45 * 256);
        sheet.SetColumnWidth(3, 20 * 256);
        sheet.SetColumnWidth(4, 14 * 256);
        sheet.SetColumnWidth(5, 12 * 256);
        sheet.SetColumnWidth(6, 15 * 256);
        sheet.SetColumnWidth(7, 15 * 256);
        sheet.SetColumnWidth(8, 16 * 256);

        using var stream = new MemoryStream();
        workbook.Write(stream);
        return stream.ToArray();
    }
}
