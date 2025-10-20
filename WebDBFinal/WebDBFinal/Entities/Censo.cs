using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("Censo")]
public partial class Censo
{
    [Key]
    public int CodigoCenso { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public int Anio { get; set; }

    [InverseProperty("CodigoCensoNavigation")]
    public virtual ICollection<DetalleCenso> DetalleCensos { get; set; } = new List<DetalleCenso>();
}
