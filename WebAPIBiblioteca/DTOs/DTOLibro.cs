using WebAPIBiblioteca.Models;

namespace WebAPIBiblioteca.DTOs
{
    public class DTOLibro
    {
        public string Isbn { get; set; }
        public string Titulo { get; set; }
        public int Paginas { get; set; }
        public decimal Precio { get; set; }
        public int AutorId { get; set; }
        public int EditorialId { get; set; }
        public bool? Descatalogados { get; set; }
    }
}
