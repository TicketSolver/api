namespace TicketSolver.Infra.Storage.Providers;

public abstract class FileStorageServiceBase
{
    /// <summary>
    /// Gera uma key única para o arquivo baseado no nome original,
    /// adicionando um prefixo com GUID para evitar colisão.
    /// </summary>
    /// <param name="fileName">Nome original do arquivo (ex: foto.jpg)</param>
    /// <returns>Key única, ex: "d6f8e9b2-4a8c-4f02-a7b9-9c84c6e0f9a1_foto.jpg"</returns>
    protected string GenerateUniqueKey(string fileName)
    {
        var guid = Guid.NewGuid().ToString();
        var cleanFileName = Path.GetFileName(fileName).Replace(" ", "_");
        return $"{guid}_{cleanFileName}";
    }

    /// <summary>
    /// Monta a string do caminho do arquivo
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    protected string BuildPath(string fileName, List<string> path)
    {
        var clearedPath = path
            .Select(p => p.Trim().Replace('/', '_').Replace(' ', '_'))
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();
        
        var divider = clearedPath.Count > 0 ? "/" : "";
        return $"{string.Join('/', clearedPath)}{divider}{GenerateUniqueKey(fileName)}";
    }

    /// <summary>
    /// Extrai o nome do arquivo (key) removendo diretórios (se houver).
    /// Útil para padronizar keys.
    /// </summary>
    protected string ExtractFileName(string key)
    {
        return Path.GetFileName(key);
    }

    // Outros métodos utilitários comuns podem entrar aqui
}