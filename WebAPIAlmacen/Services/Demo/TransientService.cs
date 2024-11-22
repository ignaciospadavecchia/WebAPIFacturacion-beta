namespace WebAPIAlmacen.Services.Demo
{
    public class TransientService
    {
        public Guid Guid = Guid.NewGuid();
        public Guid GetGuid()
        {
            return Guid;
        }
    }

}
