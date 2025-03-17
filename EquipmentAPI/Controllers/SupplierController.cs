using EquipmentAPI.Repositories.SupplierRepositort;
using EquipmentAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace EquipmentAPI.Controllers
{
    [ApiController]
    public class SupplierController
    {
        private readonly ISupplierRepository _supplierRepository;
        private readonly IPropertyCheckerService _propertyCheckerService;
        private readonly IMemoryCache _memoryCache;

        public SupplierController(ISupplierRepository supplierRepository, IPropertyCheckerService propertyCheckerService, IMemoryCache memoryCache)
        {
            _supplierRepository = supplierRepository ??
                throw new ArgumentNullException(nameof(supplierRepository));
            _propertyCheckerService = propertyCheckerService??
                throw new ArgumentNullException(nameof(propertyCheckerService));
            _memoryCache = memoryCache ??
                throw new ArgumentNullException(nameof(memoryCache));
        }

    }
}
