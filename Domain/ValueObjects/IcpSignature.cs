using System.Text.RegularExpressions;

namespace SGHSS.Domain.ValueObjects
{
    /// <summary>
    /// Assinatura digital ICP-Brasil (envelope CMS/PKCS#7 codificado em Base64).
    /// Aceita Base64 com ou sem cabeçalhos PEM (BEGIN/END PKCS7) e com quebras de linha.
    /// Normaliza para o formato Base64 canônico (sem espaços ou quebras de linha).
    /// Valida decodificando e verificando a estrutura ASN.1 DER (deve iniciar com 0x30).
    /// ToString() retorna o Base64 canônico.
    /// </summary>
    public readonly record struct IcpSignature
    {
        public string Value { get; }

        private static readonly Regex PemBlockRegex =
            new(@"-----BEGIN\s+PKCS7-----([\s\S]*?)-----END\s+PKCS7-----",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public IcpSignature(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Empty signature.", nameof(input));

            var s = input.Trim();

            // Extrai conteúdo PEM se presente
            var pemMatch = PemBlockRegex.Match(s);
            if (pemMatch.Success)
                s = pemMatch.Groups[1].Value;

            // Romove espaços em branco e quebras de linha
            s = Regex.Replace(s, @"\s+", "");

            byte[] der;
            try
            {
                der = Convert.FromBase64String(s);
            }
            catch
            {
                throw new ArgumentException("Invalid signature: malformed Base64.", nameof(input));
            }

            if (der.Length < 2 || der[0] != 0x30) // 0x30 = ASN.1 SEQUENCE
                throw new ArgumentException("Invalid signature: not a DER CMS/PKCS#7 structure.", nameof(input));

            // Base64 canônica
            Value = Convert.ToBase64String(der);
        }

        public override string ToString() => Value;
    }
}
