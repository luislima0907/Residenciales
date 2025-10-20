using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

public partial class TipoGaritum
{
    [Key]
    public byte CodigoTipoGarita { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [InverseProperty("CodigoTipoGaritaNavigation")]
    public virtual ICollection<GaritaSeguridad> GaritaSeguridads { get; set; } = new List<GaritaSeguridad>();
}
