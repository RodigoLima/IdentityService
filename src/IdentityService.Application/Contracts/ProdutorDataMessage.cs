namespace AgroSolutions.Medicoes.Application.Contracts;

public record ProdutorDataMessage(
    Guid ProdutorId,
    string Email
);
