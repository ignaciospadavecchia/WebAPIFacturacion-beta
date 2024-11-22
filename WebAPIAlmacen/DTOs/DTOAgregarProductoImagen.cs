namespace WebAPIAlmacen.DTOs
{
    public class DTOAgregarProductoImagen
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public IFormFile Foto { get; set; }
        public int FamiliaId { get; set; }
    }
}
