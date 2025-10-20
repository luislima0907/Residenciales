namespace WebDBFinal.Models;

public class StoredProcedureInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ProcedureParameter> Parameters { get; set; } = new();
}

public class ProcedureParameter
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = true;
}

public class ProcedureExecutionRequest
{
    public string ProcedureName { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new();
}

