using SmartInvoice.Application.Common;
using SmartInvoice.Application.Customers.DTOs;

namespace SmartInvoice.Application.Customers;

public interface ICustomerAddressService
{
    Task<IReadOnlyList<CustomerAddressResponse>> GetAllAsync(Guid customerId);
    Task<Result<CustomerAddressResponse>> CreateAsync(Guid customerId, CustomerAddressRequest request);
    Task<Result<CustomerAddressResponse>> UpdateAsync(Guid customerId, Guid addressId, CustomerAddressRequest request);
    Task<Result<bool>> DeleteAsync(Guid customerId, Guid addressId);
}
