using CRISP.Application.DTOs;
namespace CRISP.Application.Interfaces;
public interface IRuleEvaluationService
{
    Task<EvaluateResponse> EvaluateAsync(EvaluateRequest request, CancellationToken ct = default);
}
