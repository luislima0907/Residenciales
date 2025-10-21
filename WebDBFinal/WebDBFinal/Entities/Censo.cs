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

    [Column(TypeName = "date")]
    public DateTime FechaInicio { get; set; }

    [Column(TypeName = "date")]
    public DateTime FechaFin { get; set; }

    public int Anio { get; set; }

    [InverseProperty("CodigoCensoNavigation")]
    public virtual ICollection<DetalleCenso> DetalleCensos { get; set; } = new List<DetalleCenso>();
}
