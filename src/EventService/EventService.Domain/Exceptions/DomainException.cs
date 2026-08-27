namespace EventService.Domain.Exceptions;

/// <summary>
/// Excepción para violaciones de reglas de negocio del dominio.
/// Se traduce a un 400/422 en la capa API, nunca expone detalles internos.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
