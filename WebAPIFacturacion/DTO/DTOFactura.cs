namespace WebAPIFacturacion.DTO
{
    public class DTOFactura
    {
        public int NFactura { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Importe { get; set; }
        public bool Pagada { get; set; }
        public int IdCliente { get; set; }
    }
}
