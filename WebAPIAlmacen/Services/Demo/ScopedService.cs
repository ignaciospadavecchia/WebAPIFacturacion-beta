namespace WebAPIAlmacen.Services.Demo
{
    public class ScopedService
    {
        public Guid Guid = Guid.NewGuid();
        public Guid GetGuid()
        {
            return Guid;
        }
    }

}
