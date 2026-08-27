namespace EventService.Api.Auth;

/// <summary>
/// Emisor de JWT "local" para la demo (issuer propio, sin IdP externo).
/// En producción esto se reemplaza por Cognito/Keycloak vía OIDC.
/// </summary>
public sealed record TokenRequest(string Username, string Role);
