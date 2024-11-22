namespace WebAPIAlmacen.Services
{
    public class ContadorPeticionesService
    {
        int Contador = 0;
        public void Incrementar()
        {
            Contador++;
        }

        public int GetContador()
        {
            return Contador;
        }
    }
}
