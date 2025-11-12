// Domain/ValueObjects/IpAddress.cs

using System.Net;
using System.Text.RegularExpressions;

namespace SGHSS.Domain.ValueObjects
{
    /// <summary>
    /// Value Object para endereço IP (IPv4 ou IPv6). Aceita ambos os formatos.
    /// Normaliza usando System.Net.IPAddress.ToString():
    /// * IPv4: remove zeros à esquerda (ex.: 192.168.000.001 → 192.168.0.1)
    /// * IPv6: forma canônica (RFC 5952): hex minúsculo, sem zeros à esquerda, compressão "::"
    /// Trim do input e recusa endereços inválidos.
    /// </summary>
    public readonly record struct IpAddress
    {
        // Regex para detectar IPv4 incompleto (1 a 3 octetos), usado para validação inicial
        private static readonly Regex Ipv4IncompleteRegex = new(@"^\d{1,3}(\.\d{1,3}){0,2}$", RegexOptions.Compiled);

        public string Value { get; }

        public IpAddress(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("IP vazio.", nameof(input));

            if (Ipv4IncompleteRegex.IsMatch(input))
                throw new ArgumentException("IPv4 incompleto (deve conter 4 octetos).", nameof(input));

            input = input.Trim();

            if (!IPAddress.TryParse(input, out var ip))
                throw new ArgumentException("IP inválido.", nameof(input));

            // Canonicaliza via ToString() do próprio IPAddress:
            //  - IPv4: "192.168.0.1"
            //  - IPv6: "2001:db8::ff00:42:8329"
            Value = ip.ToString();
        }

        public override string ToString() => Value;
    }
}
