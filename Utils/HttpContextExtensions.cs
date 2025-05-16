namespace BlogMVC.Utils
{
    public static class HttpContextExtensions
    {
        public static string ObtenerUrlRetorno(this HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            return context.Request.Path + context.Request.QueryString;
        }
    }
}
