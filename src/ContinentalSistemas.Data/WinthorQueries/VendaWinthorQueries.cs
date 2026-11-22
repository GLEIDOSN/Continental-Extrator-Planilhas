namespace ContinentalSistemas.Data.WinthorQueries;

public static class VendaWinthorQueries
{
    public static string VendasPorPeriodo(string numeroVendas) => $@"
        SELECT 
            '' AS PRO_CODIGO,
            I.CODPROD AS COD_R3MAGAZINE,
            P.DESCRICAO AS PRO_DESCRICAO,
            P.CODAUXILIAR AS PRO_REFERENCIA,
            SUM(I.QT) AS QUANTIDADE,
            '' AS VENDA,
            NVL(I.PVENDA, 0) AS CUSTO,
            '' AS PRECO_LOJA,
            SUM(I.QT * NVL(I.PVENDA, 0)) AS CUSTO_TOTAL
        FROM PCPEDI I
        INNER JOIN PCPEDC C ON I.NUMPED = C.NUMPED
        INNER JOIN PCPRODUT P ON I.CODPROD = P.CODPROD
        WHERE C.NUMPED IN ({numeroVendas})
          AND C.POSICAO <> 'C' 
        GROUP BY 
            I.CODPROD,
            P.DESCRICAO,
            P.CODAUXILIAR,
            NVL(I.PVENDA, 0)
        ORDER BY 2";

    public static string NumerosVendasExistentes(string numerosVendas) => $@"
        SELECT NUMPED
        FROM PCPEDC
        WHERE NUMPED IN ({numerosVendas})
          AND POSICAO <> 'C'";
}
