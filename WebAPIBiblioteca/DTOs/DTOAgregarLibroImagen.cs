namespace WebAPIAlmacen.DTOs
{
    public class DTOAgregarLibroImagen
    {
        public string Isbn { get; set; }
        public string Titulo { get; set; }
        public int Paginas { get; set; }
        public decimal Precio { get; set; }
        public IFormFile Foto { get; set; }
        public int AutorId { get; set; }
        public int EditorialId { get; set; }
    }
}
