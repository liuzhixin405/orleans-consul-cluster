namespace eapi.Data
{
    public class DbFactory : IDisposable
    {
        private bool _disposed;
        private Func<ProductDbContext> _instanceFunc;
        private ProductDbContext _context;
        public ProductDbContext DbContext => _context ?? (_context = _instanceFunc.Invoke());

        public DbFactory(Func<ProductDbContext> func)
        {
            _instanceFunc = func;
        }
        public void Dispose()
        {
            if (!_disposed && _context != null)
            {
                _disposed = true;
                _context.Dispose();
            }
        }
    }
}
