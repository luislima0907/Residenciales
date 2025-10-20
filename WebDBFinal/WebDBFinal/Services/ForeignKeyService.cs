using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using WebDBFinal.Context;

namespace WebDBFinal.Services;

public class ForeignKeyService
{
    private readonly ResidencialesDbContext _context;

    public ForeignKeyService(ResidencialesDbContext context)
    {
        _context = context;
    }

    // Obtener información de llaves foráneas para una propiedad
    public ForeignKeyInfo? GetForeignKeyInfo(PropertyInfo property, Type entityType)
    {
        // Buscar si la propiedad tiene el atributo ForeignKey
        var foreignKeyAttr = property.GetCustomAttribute<ForeignKeyAttribute>();
        
        if (foreignKeyAttr != null)
        {
            // Obtener el nombre de la propiedad de navegación
            var navigationPropertyName = foreignKeyAttr.Name;
            var navigationProperty = entityType.GetProperty(navigationPropertyName);
            
            if (navigationProperty != null && navigationProperty.PropertyType.IsClass)
            {
                return new ForeignKeyInfo
                {
                    PropertyName = property.Name,
                    RelatedEntityType = navigationProperty.PropertyType,
                    NavigationPropertyName = navigationPropertyName
                };
            }
        }

        // Buscar propiedades de navegación que referencien esta propiedad
        foreach (var prop in entityType.GetProperties())
        {
            if (prop.PropertyType.IsClass && !prop.PropertyType.IsGenericType && 
                prop.PropertyType.Namespace == entityType.Namespace &&
                prop.GetCustomAttribute<InversePropertyAttribute>() == null)
            {
                var fkAttr = prop.GetCustomAttribute<ForeignKeyAttribute>();
                if (fkAttr != null)
                {
                    // Verificar si esta propiedad es parte de la FK
                    var fkParts = fkAttr.Name.Split(',').Select(s => s.Trim()).ToList();
                    if (fkParts.Contains(property.Name))
                    {
                        return new ForeignKeyInfo
                        {
                            PropertyName = property.Name,
                            RelatedEntityType = prop.PropertyType,
                            NavigationPropertyName = prop.Name,
                            IsComposite = fkParts.Count > 1,
                            CompositeKeyParts = fkParts
                        };
                    }
                }
            }
        }

        return null;
    }

    // Obtener todos los registros de una entidad relacionada (con caché para evitar consultas duplicadas)
    private readonly Dictionary<Type, List<DropdownItem>> _cache = new();
    
    public async Task<List<DropdownItem>> GetRelatedRecordsAsync(Type relatedEntityType)
    {
        // Verificar caché primero
        if (_cache.ContainsKey(relatedEntityType))
        {
            return _cache[relatedEntityType];
        }

        var items = new List<DropdownItem>();
        
        try
        {
            // Obtener el DbSet dinámicamente
            var dbSetProperty = _context.GetType().GetProperties()
                .FirstOrDefault(p => p.PropertyType.IsGenericType &&
                                    p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                                    p.PropertyType.GetGenericArguments()[0] == relatedEntityType);

            if (dbSetProperty == null)
                return items;

            var dbSet = dbSetProperty.GetValue(_context);
            if (dbSet == null)
                return items;

            // Obtener todos los registros
            var queryable = (IQueryable<object>)dbSet.GetType().GetMethod("AsQueryable")!.Invoke(dbSet, null)!;
            var records = await queryable.Cast<object>().ToListAsync();

            // Obtener las propiedades clave de la entidad
            var keyProperties = relatedEntityType.GetProperties()
                .Where(p => p.GetCustomAttribute<System.ComponentModel.DataAnnotations.KeyAttribute>() != null)
                .ToList();

            // Obtener propiedades de texto para mostrar
            var displayProperties = relatedEntityType.GetProperties()
                .Where(p => p.PropertyType == typeof(string) && 
                           (p.Name.ToLower().Contains("nombre") || 
                            p.Name.ToLower().Contains("descripcion") ||
                            p.Name.ToLower().Contains("title")))
                .Take(2)
                .ToList();

            // Si no hay propiedades de texto, usar las primeras propiedades no complejas
            if (!displayProperties.Any())
            {
                displayProperties = relatedEntityType.GetProperties()
                    .Where(p => !p.PropertyType.IsGenericType && 
                               (!p.PropertyType.IsClass || p.PropertyType == typeof(string)))
                    .Where(p => p.GetCustomAttribute<InversePropertyAttribute>() == null)
                    .Where(p => p.GetCustomAttribute<System.ComponentModel.DataAnnotations.KeyAttribute>() != null)
                    .Take(4)
                    .ToList();
            }

            foreach (var record in records)
            {
                // Construir el valor (llaves concatenadas)
                var keyValue = string.Join(",", keyProperties.Select(k => k.GetValue(record)?.ToString() ?? ""));

                // Construir el texto a mostrar
                var displayText = string.Join(" - ", displayProperties.Select(d => d.GetValue(record)?.ToString() ?? ""));
                
                if (string.IsNullOrEmpty(displayText))
                {
                    displayText = $"ID: {keyValue}";
                }

                items.Add(new DropdownItem
                {
                    Value = keyValue,
                    Text = displayText
                });
            }

            // Guardar en caché
            _cache[relatedEntityType] = items;
        }
        catch (Exception ex)
        {
            // Log error si es necesario
            Console.WriteLine($"Error en GetRelatedRecordsAsync: {ex.Message}");
        }

        return items;
    }
}

public class ForeignKeyInfo
{
    public string PropertyName { get; set; } = string.Empty;
    public Type RelatedEntityType { get; set; } = null!;
    public string NavigationPropertyName { get; set; } = string.Empty;
    public bool IsComposite { get; set; }
    public List<string> CompositeKeyParts { get; set; } = new();
}

public class DropdownItem
{
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
