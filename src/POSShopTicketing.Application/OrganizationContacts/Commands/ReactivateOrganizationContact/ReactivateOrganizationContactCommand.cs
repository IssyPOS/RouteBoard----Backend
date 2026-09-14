//using MediatR;
//using POSShopTicketing.Application.Common.Exceptions;
//using POSShopTicketing.Application.Common.Interfaces;
//using POSShopTicketing.Domain.Entities;
//using POSShopTicketing.Domain.Enums;
//using POSShopTicketing.Domain.Exceptions;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace POSShopTicketing.Application.OrganizationContacts.Commands.ReactivateOrganizationContact
//{
//    internal class ReactivateOrganizationContactCommand
//    {
//    }
//}

//using MediatR;
//using POSShopTicketing.Application.Common.Exceptions;
//using POSShopTicketing.Application.Common.Interfaces;
//using POSShopTicketing.Domain.Entities;
//using POSShopTicketing.Domain.Enums;
//using POSShopTicketing.Domain.Exceptions;

//namespace POSShopTicketing.Application.Organizations.Commands.ReactivateOrganizationDepartment;

//public record ReactivateOrganizationDepartmentCommand(Guid OrganizationDepartmentId) : IRequest;

//public class ReactivateOrganizationDepartmentCommandHandler : IRequestHandler<ReactivateOrganizationDepartmentCommand>
//{
//    private readonly IApplicationDbContext _context;

//    public ReactivateOrganizationDepartmentCommandHandler(IApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task Handle(ReactivateOrganizationDepartmentCommand request, CancellationToken cancellationToken)
//    {
//        if (request.OrganizationDepartmentId == Guid.Empty)
//        {
//            throw new DomainException("The reserved Organization Department tenant does not need reactivating.");
//        }

//        var organization = await _context.OrganizationDepartments.FindAsync(new object[] { request.OrganizationDepartmentId }, cancellationToken)
//            ?? throw new NotFoundException(nameof(Organization), request.OrganizationDepartmentId);

//        organization.Status = OrganizationDepartmentStatus.Active;
//        await _context.SaveChangesAsync(cancellationToken);
//    }
//}



