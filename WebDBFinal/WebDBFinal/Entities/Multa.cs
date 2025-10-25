using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

public partial class Multa
{
    [Key]
    public int CodigoMulta { get; set; }

    public byte CodigoTipoMulta { get; set; }

    public int NumeroCasa { get; set; }

    public int CodigoCluster { get; set; }

    public int CodigoSucursal { get; set; }

    public int CodigoSector { get; set; }

    public DateOnly Fecha { get; set; }

    public bool? Pagada { get; set; }

    [InverseProperty("CodigoMultaNavigation")]
    public virtual ICollection<AplicacionDocumento> AplicacionDocumentos { get; set; } = new List<AplicacionDocumento>();

    [ForeignKey("NumeroCasa, CodigoCluster, CodigoSucursal, CodigoSector")]
    [InverseProperty("Multa")]
    public virtual Casa Casa { get; set; } = null!;

    [ForeignKey("CodigoTipoMulta")]
    [InverseProperty("Multa")]
    public virtual TipoMultum CodigoTipoMultaNavigation { get; set; } = null!;
}
