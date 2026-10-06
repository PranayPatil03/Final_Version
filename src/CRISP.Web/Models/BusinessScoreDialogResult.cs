using CRISP.Application.DTOs;

namespace CRISP.Web.Models;

public sealed record BusinessScoreDialogResult(Dictionary<int, FrameworkParameterDto> Parameters, EvaluateResponse? Evaluation);
