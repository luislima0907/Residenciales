using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WebDBFinal.Context;
using WebDBFinal.Models;

namespace WebDBFinal.Services;

public class StoredProcedureService
{
    private readonly ResidencialesDbContext _context;

    public StoredProcedureService(ResidencialesDbContext context)
    {
        _context = context;
    }

    // Obtener lista de procedimientos almacenados personalizados (que empiezan con sp_Consulta)
    public async Task<List<StoredProcedureInfo>> GetStoredProceduresAsync()
    {
        var procedures = new List<StoredProcedureInfo>();
        
        var query = @"
            SELECT 
                OBJECT_ID(name) as ProcedureId,
                name as ProcedureName
            FROM sys.procedures
            WHERE name LIKE 'sp_Consulta%'
            AND type = 'P'
            ORDER BY name";

        var connection = _context.Database.GetDbConnection();
        
        try
        {
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = query;
            
            // Primero obtener todos los procedimientos en una lista
            var proceduresList = new List<(int Id, string Name)>();
            
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var procedureId = reader.GetInt32(0);
                    var procedureName = reader.GetString(1);
                    proceduresList.Add((procedureId, procedureName));
                }
            } // Cerrar el reader antes de obtener parámetros

            // Ahora obtener los parámetros de cada procedimiento
            int counter = 1;
            foreach (var proc in proceduresList)
            {
                procedures.Add(new StoredProcedureInfo
                {
                    Id = counter++,
                    Name = proc.Name,
                    Description = GetDescriptionFromProcedureName(proc.Name),
                    Parameters = await GetProcedureParametersAsync(proc.Id)
                });
            }
        }
        finally
        {
            if (connection.State == ConnectionState.Open)
                await connection.CloseAsync();
        }

        return procedures;
    }

    // Obtener parámetros de un procedimiento almacenado
    private async Task<List<ProcedureParameter>> GetProcedureParametersAsync(int procedureId)
    {
        var parameters = new List<ProcedureParameter>();
        
        var query = @"
            SELECT 
                p.name as ParameterName,
                TYPE_NAME(p.user_type_id) as DataType,
                p.is_output as IsOutput
            FROM sys.parameters p
            WHERE p.object_id = @ProcedureId
            AND p.name != ''
            ORDER BY p.parameter_id";

        var connection = _context.Database.GetDbConnection();
        
        try
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = query;
            command.Parameters.Add(new SqlParameter("@ProcedureId", procedureId));
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var paramName = reader.GetString(0).TrimStart('@');
                var dataType = reader.GetString(1);
                var isOutput = reader.GetBoolean(2);

                if (!isOutput) // Solo parámetros de entrada
                {
                    parameters.Add(new ProcedureParameter
                    {
                        Name = paramName,
                        Type = MapSqlTypeToCSharpType(dataType),
                        IsRequired = true
                    });
                }
            }
        }
        finally
        {
            // No cerramos la conexión aquí porque la gestiona el contexto
        }

        return parameters;
    }

    // Ejecutar un procedimiento almacenado y obtener resultados
    public async Task<DataTable> ExecuteProcedureAsync(string procedureName, Dictionary<string, string> parameters)
    {
        var dataTable = new DataTable();
        
        // Usar la conexión administrada por el DbContext en lugar de crear una nueva con una connection string que puede ser null
        var connection = _context.Database.GetDbConnection();
        
        try
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = procedureName;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = 120; // 2 minutos de timeout

            // Agregar parámetros
            foreach (var param in parameters)
            {
                var value = string.IsNullOrWhiteSpace(param.Value) ? DBNull.Value : (object)param.Value;
                // Si la conexión es SqlConnection, usamos SqlParameter; de lo contrario creamos un DbParameter genérico
                var dbParam = command.CreateParameter();
                dbParam.ParameterName = $"@{param.Key}";
                dbParam.Value = value;
                command.Parameters.Add(dbParam);
            }

            using var reader = await command.ExecuteReaderAsync();
            dataTable.Load(reader);
        }
        finally
        {
            if (connection.State == ConnectionState.Open)
                await connection.CloseAsync();
        }

        return dataTable;
    }

    // Mapear tipos de SQL a tipos de C#/HTML
    private string MapSqlTypeToCSharpType(string sqlType)
    {
        return sqlType.ToLower() switch
        {
            "int" => "number",
            "bigint" => "number",
            "smallint" => "number",
            "tinyint" => "number",
            "decimal" => "number",
            "numeric" => "number",
            "float" => "number",
            "real" => "number",
            "money" => "number",
            "smallmoney" => "number",
            "date" => "date",
            "datetime" => "datetime-local",
            "datetime2" => "datetime-local",
            "smalldatetime" => "datetime-local",
            "time" => "time",
            "bit" => "checkbox",
            _ => "text"
        };
    }

    // Obtener descripción basada en el nombre del procedimiento
    private string GetDescriptionFromProcedureName(string procedureName)
    {
        // Extraer el número del nombre del procedimiento (ej: sp_Consulta01 -> 1, sp_Consulta23_Nombre -> 23)
        var numberPart = new string(procedureName.Where(char.IsDigit).ToArray());
        
        if (!string.IsNullOrEmpty(numberPart) && int.TryParse(numberPart, out int consultaNumber))
        {
            return GetDescriptionByNumber(consultaNumber);
        }

        // Si no tiene número, devolver el nombre sin el prefijo sp_
        return procedureName.Replace("sp_", "").Replace("_", " ");
    }

    // Mapeo de descripciones según el archivo Proceduras.txt
    private string GetDescriptionByNumber(int number)
    {
        var descriptions = new Dictionary<int, string>
        {
            { 1, "Cuántos vehículos posee cada vivienda" },
            { 2, "Cuántos vehículos ingresan de visitante por hora" },
            { 3, "Reporte de viviendas por clúster con propietarios e inquilinos" },
            { 4, "Vehículos que ingresan entre las 23:00 y 1:00 am" },
            { 5, "Vecino que más ha pagado en un período" },
            { 6, "Propietarios casados menores de 30 años en Diana II" },
            { 7, "Persona que más ha sido presidente" },
            { 8, "Persona que nunca ha sido miembro de junta directiva" },
            { 9, "Guardias que han trabajado más de 24 horas" },
            { 10, "Cantidad de guardias hombres y mujeres" },
            { 11, "Vecino que más ingresa y sale los domingos" },
            { 12, "Vivienda más atrasada en pagos" },
            { 13, "Día del mes que más dinero se recibe" },
            { 14, "Residencia que más recibos ha recibido" },
            { 15, "Reporte de carros que ingresaron por día y mes" },
            { 16, "Persona que más visita el condominio" },
            { 17, "Inquilinos que son propietarios" },
            { 18, "Propietarios con licencia tipo A" },
            { 19, "Personas que deben pagar 150 por exceso de carros" },
            { 20, "Residencia que más visitas recibe" },
            { 21, "Concepto de multa más utilizado" },
            { 22, "Casas pendientes de pagar multas" },
            { 23, "Mes con más multas por desorden" },
            { 24, "Total recaudado por multas" },
            { 25, "Propietario con más multas" }
        };

        if (descriptions.ContainsKey(number))
        {
            return $"{number}. {descriptions[number]}";
        }
        
        return $"Consulta {number}";
    }
}
