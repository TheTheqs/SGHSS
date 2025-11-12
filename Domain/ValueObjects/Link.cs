// Domain/ValueObjects/Link.cs

using System.Text.RegularExpressions;

namespace SGHSS.Domain.ValueObjects
{
    /// <summary>
    /// Value Object para links de teleconsulta.
    /// Case-sensitive: preserva exatamente o case do input.
    /// Nunca contém host/domínio: expõe somente "path[?query][#fragment]". O domínio/host deve ser delegado ao front por flexibilização.
    /// Remove barras iniciais e colapsa múltiplas barras internas.
    /// </summary>
    public readonly record struct Link
    {
        public string Value { get; }

        public Link(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Link vazio.", nameof(input));

            input = input.Trim();

            // 1) Elimina esquema e host se houver (http://, https://, www., subdomínios etc.)
            // Formas aceitas para entrada absoluta: http(s)://host/...  ou  www.host/... 
            string rest = input;
            int schemeIdx = rest.IndexOf("://", StringComparison.Ordinal);
            if (schemeIdx >= 0)
            {
                // Corta "scheme://" e o host até a primeira '/'
                int startAfterScheme = schemeIdx + 3;
                int firstSlash = rest.IndexOf('/', startAfterScheme);
                if (firstSlash < 0)
                    throw new ArgumentException("Link deve conter um caminho após o domínio.", nameof(input));
                rest = rest.Substring(firstSlash); // mantém desde a primeira '/' (inclui query/fragment)
            }
            else if (rest.StartsWith("www.", StringComparison.Ordinal))
            {
                // Remove www.<host> até a primeira '/'
                int firstSlash = rest.IndexOf('/');
                if (firstSlash < 0)
                    throw new ArgumentException("Link deve conter um caminho após o domínio.", nameof(input));
                rest = rest.Substring(firstSlash);
            }

            // Agora 'rest' pode ser: "/path?query#frag" ou "path?query#frag"
            // 2) Separar fragment e query preservando case
            string fragment = string.Empty;
            string query = string.Empty;
            string pathPart = rest;

            // fragment
            int hashIdx = pathPart.IndexOf('#');
            if (hashIdx >= 0)
            {
                fragment = pathPart.Substring(hashIdx);      // inclui '#'
                pathPart = pathPart.Substring(0, hashIdx);
            }

            // query
            int qIdx = pathPart.IndexOf('?');
            if (qIdx >= 0)
            {
                query = pathPart.Substring(qIdx);            // inclui '?'
                pathPart = pathPart.Substring(0, qIdx);
            }

            // 3) Normalizar apenas o PATH:
            // - remover espaços
            // - colapsar barras consecutivas (// -> /)
            // - remover todas as barras iniciais (Value não deve começar com '/')
            string path = (pathPart ?? string.Empty).Trim();
            path = Regex.Replace(path, "/{2,}", "/");
            path = path.TrimStart('/');

            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Link deve conter um caminho após o domínio.", nameof(input));

            // 4) Montar o Value final: path + query + fragment (sem host e sem barra inicial)
            //    Sem qualquer alteração de case.
            Value = path + query + fragment;

            // nunca conter host/esquema
            if (Value.Contains("://", StringComparison.Ordinal))
                throw new ArgumentException("Link não deve conter esquema/domínio.", nameof(input));
        }

        public override string ToString() => Value;
    }
}
