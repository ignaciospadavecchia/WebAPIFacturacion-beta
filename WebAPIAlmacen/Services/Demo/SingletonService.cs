namespace WebAPIAlmacen.Services.Demo
{
    public class SingletonService
    {
        public Guid Guid = Guid.NewGuid();
        public Guid GetGuid()
        {
            return Guid;
        }
    }

}
