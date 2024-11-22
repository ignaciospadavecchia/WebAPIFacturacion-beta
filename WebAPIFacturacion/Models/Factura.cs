using System;
using System.Collections.Generic;

namespace WebAPIFacturacion.Models;

public partial class Factura
{
    public int Nfactura { get; set; }

    public DateOnly Fecha { get; set; }

    public decimal Importe { get; set; }

    public bool Pagada { get; set; }

    public int IdCliente { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;
}
