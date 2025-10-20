using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[PrimaryKey("CodigoTelefonoSucursal", "CodigoSucursal")]
[Table("TelefonoSucursal")]
public partial class TelefonoSucursal
{
    [Key]
    public int CodigoTelefonoSucursal { get; set; }

    [Key]
    public int CodigoSucursal { get; set; }

    public byte CodigoTipoTelefono { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string Telefono { get; set; } = null!;

    [ForeignKey("CodigoSucursal")]
    [InverseProperty("TelefonoSucursals")]
    public virtual Sucursal CodigoSucursalNavigation { get; set; } = null!;

    [ForeignKey("CodigoTipoTelefono")]
    [InverseProperty("TelefonoSucursals")]
    public virtual TipoTelefono CodigoTipoTelefonoNavigation { get; set; } = null!;
}
