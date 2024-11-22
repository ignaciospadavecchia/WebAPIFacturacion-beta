namespace WebAPIAlmacen.DTOs
{
    public class DTOAgregarProducto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public bool Descatalogado { get; set; }
        public int FamiliaId { get; set; }
    }
}
