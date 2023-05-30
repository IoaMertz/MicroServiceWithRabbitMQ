using MicroRabbit.Banking.Application.Interfaces;
using MicroRabbit.Banking.Application.Models_dtos_;
using MicroRabbit.Banking.Domain.Commands;
using MicroRabbit.Banking.Domain.Interfaces;
using MicroRabbit.Banking.Domain.Models;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Banking.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IEventBus _bus;
        public AccountService(IAccountRepository accountRepository, IEventBus bus)
        {
            _accountRepository = accountRepository;
            _bus = bus;
            
        }
        public IEnumerable<Account> GetAccounts()
        {
            return _accountRepository.GetAccounts();
        }

        public void Tranfer(AccountTransfer accountTranfer)
        {
            var createTransferCommand = new CreateTransferCommand(
                     accountTranfer.FromAccount,
                     accountTranfer.ToAccount,
                     accountTranfer.TranferAmount
                );

            // ---> _mediator.Send(command); 
            _bus.SendCommand( createTransferCommand );

        }
    }
}
