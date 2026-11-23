namespace Platform.ServiceDefaults.Grpc;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using global::Grpc.Core;
using global::Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

public class LoggingInterceptor : Interceptor
{
    private readonly ILogger<LoggingInterceptor> _logger;

    public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var response = await continuation(request, context);
            sw.Stop();
            _logger.LogInformation(
                "gRPC {Method} from {Peer}: OK in {Elapsed} ms",
                context.Method,
                context.Peer,
                sw.ElapsedMilliseconds);
            return response;
        }
        catch (RpcException ex)
        {
            sw.Stop();
            _logger.LogWarning(
                ex,
                "gRPC {Method} from {Peer}: {Status} in {Elapsed} ms",
                context.Method,
                context.Peer,
                ex.Status.StatusCode,
                sw.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(
                ex,
                "gRPC {Method} from {Peer} failed in {Elapsed} ms",
                context.Method,
                context.Peer,
                sw.ElapsedMilliseconds);
            throw;
        }
    }
}
