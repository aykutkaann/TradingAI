using MediatR;

namespace TradingAI.Application.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest<Unit>;
