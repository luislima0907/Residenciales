using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("TipoIntegrante")]
public partial class TipoIntegrante
{
    [Key]
    public byte CodigoTipoIntegrante { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [InverseProperty("CodigoTipoIntegranteNavigation")]
    public virtual ICollection<IntegranteJunta> IntegranteJunta { get; set; } = new List<IntegranteJunta>();
}
