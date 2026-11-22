using System.Diagnostics;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace ContinentalSistemas.App.Diagnostics;

public sealed class MonitorCircuito(ILogger<MonitorCircuito> logger) : CircuitHandler
{
    private long? inicioCircuito;
    private long? inicioDesconexao;

    public string Id { get; } = Guid.NewGuid().ToString("N");

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        inicioCircuito = Stopwatch.GetTimestamp();
        logger.LogInformation("Circuito aberto: circuitoId={CircuitoId}", Id);
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        inicioDesconexao = Stopwatch.GetTimestamp();
        logger.LogInformation("Circuito desconectado: circuitoId={CircuitoId}", Id);
        return Task.CompletedTask;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        if (inicioDesconexao is long inicio)
        {
            logger.LogInformation("Circuito reconectado: circuitoId={CircuitoId}, desconectadoMs={DesconectadoMs}",
                Id, Stopwatch.GetElapsedTime(inicio).TotalMilliseconds);
            inicioDesconexao = null;
        }
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        double? duracaoMs = inicioCircuito is long inicio ? Stopwatch.GetElapsedTime(inicio).TotalMilliseconds : null;
        double? desconectadoMs = inicioDesconexao is long desconexao ? Stopwatch.GetElapsedTime(desconexao).TotalMilliseconds : null;
        logger.LogInformation("Circuito encerrado: circuitoId={CircuitoId}, duracaoMs={DuracaoMs}, desconectadoMs={DesconectadoMs}",
            Id, duracaoMs, desconectadoMs);
        return Task.CompletedTask;
    }
}
